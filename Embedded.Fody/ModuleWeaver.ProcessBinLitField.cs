using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public sealed partial class ModuleWeaver {

  private static readonly Regex RxNonHexChars = new(@"(?:^0x|[^0-9A-Fa-f]+)", RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private byte[] ParseHexStrings(string[] ss)
    => HexStringToBytes(RxNonHexChars.Replace(string.Concat(ss), ""));

  private static readonly Regex RxNonBase64Chars = new(@"[^0-9A-Za-z\+\/=]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private byte[] ParseBase64Strings(string[] ss) {
    string? s = null;
    try {
      s = string.Concat(ss);
      s = RxNonBase64Chars.Replace(s, "");
      return Convert.FromBase64String(s);
    }
    catch (FormatException fe) {
      throw new FormatException($"Not Base 64: {s}", fe);
    }
  }

  // 0 = 0x30
  // 9 = 0x39
  // A = 0x41
  // F = 0x46
  // a = 0x61
  // f = 0x66

  private static unsafe byte[] HexStringToBytes(string hexChars) {
    var l = hexChars.Length;
    if ((l & 1) != 0)
      throw new NotSupportedException("Hex string must be of even length.");

    var buf = new byte[l >> 1];
    fixed (byte* pBuf = buf)
    fixed (char* pChars = hexChars) {
      var i = 0;
      while (l - i >= 4) {
        var read = *(ulong*) &pChars[i];
        var ch0 = (byte) (read & 0xFF);
        var ch1 = (byte) ((read >> 16) & 0xFF);
        var ch2 = (byte) ((read >> 32) & 0xFF);
        var ch3 = (byte) ((read >> 48) & 0xFF);

        var ch0N = (ch0 >> 6) * 9 + (ch0 & 0xF);
        var ch1N = (ch1 >> 6) * 9 + (ch1 & 0xF);
        var ch2N = (ch2 >> 6) * 9 + (ch2 & 0xF);
        var ch3N = (ch3 >> 6) * 9 + (ch3 & 0xF);

        var twoBytes = (ushort) ((ch2N << 12) | (ch3N << 8) | (ch0N << 4) | ch1N);

        *(ushort*) &pBuf[i >> 1] = twoBytes;

        i += 4;
      }

      // ReSharper disable once InvertIf
      if (l > i) {
        var read = *(uint*) &pChars[i];
        var ch0 = (byte) (read & 0xFF);
        var ch1 = (byte) ((read >> 16) & 0xFF);
        var ch0N = (ch0 >> 6) * 9 + (ch0 & 0xF);
        var ch1N = (ch1 >> 6) * 9 + (ch1 & 0xF);
        var oneByte = (byte) ((ch0N << 4) | ch1N);
        pBuf[i >> 1] = oneByte;
      }
    }

    return buf;
  }

  private byte[] TranscodeStrings(int codePage, string[] ss) {
    var enc = Encoding.GetEncoding(codePage);
    var s = string.Concat(ss);
    return enc.GetBytes(s);
  }

  private void ProcessBinLitField(CustomAttribute ca, FieldDefinition field, MemoryStream binaryLiteralResourceStream) {
    var td = field.DeclaringType;
    int alignment;
    byte[] data;
    var attrName = ca.AttributeType.Name;
    var args = ca.ConstructorArguments;
    if (attrName == _binLitAttrName) {
      alignment = (int) args[0].Value;
      if (args[1].Value is CustomAttributeArgument[] caa) {
        var firstArrayVal = caa[0].Value;
        data = firstArrayVal switch {
          sbyte => caa.Select(x => (byte) (sbyte) x.Value).ToArray(),
          byte => caa.Select(x => (byte) x.Value).ToArray(),
          short => caa.SelectMany(x => BitConverter.GetBytes((short) x.Value)).ToArray(),
          ushort => caa.SelectMany(x => BitConverter.GetBytes((ushort) x.Value)).ToArray(),
          int => caa.SelectMany(x => BitConverter.GetBytes((int) x.Value)).ToArray(),
          uint => caa.SelectMany(x => BitConverter.GetBytes((uint) x.Value)).ToArray(),
          long => caa.SelectMany(x => BitConverter.GetBytes((long) x.Value)).ToArray(),
          ulong => caa.SelectMany(x => BitConverter.GetBytes((ulong) x.Value)).ToArray(),
          float => caa.SelectMany(x => BitConverter.GetBytes((float) x.Value)).ToArray(),
          double => caa.SelectMany(x => BitConverter.GetBytes((double) x.Value)).ToArray(),
          _ => throw new NotSupportedException()
        };
      }
      else
        data = (byte[]) args[1].Value;
    }
    else if (attrName == _hexLitAttrName) {
      alignment = (int) args[0].Value;
      var strings = args[1].Value is CustomAttributeArgument[] caa
        ? caa.Select(x => (string) x.Value).ToArray()
        : (string[]) args[1].Value;
      data = ParseHexStrings(strings);
    }
    else if (attrName == _b64LitAttrName) {
      alignment = (int) args[0].Value;
      var strings = args[1].Value is CustomAttributeArgument[] caa
        ? caa.Select(x => (string) x.Value).ToArray()
        : (string[]) args[1].Value;
      data = ParseBase64Strings(strings);
    }
    else if (attrName == _encLitAttrName) {
      alignment = (int) args[0].Value;
      var codePage = (int) args[1].Value;
      data = TranscodeStrings(codePage, args[2].Value is CustomAttributeArgument[] caa
        ? caa.Select(x => (string) x.Value).ToArray()
        : (string[]) args[2].Value);
    }
    else
      throw new NotSupportedException(attrName);

    if (data == null) {
      // TODO: optional removal of attribute
      field.CustomAttributes.Remove(ca);
      return;
    }

    var binLiteralTableAlignSize = 1L << alignment;
    var binLiteralTableAlignMask = binLiteralTableAlignSize - 1;

    var size = data.Length;
    var prePad = binLiteralTableAlignMask & (binLiteralTableAlignSize - (binaryLiteralResourceStream.Position & binLiteralTableAlignMask));
    for (var i = 0; i < prePad; ++i)
      binaryLiteralResourceStream.WriteByte(0);
    var offset = binaryLiteralResourceStream.Position;
    // write binary literal
    binaryLiteralResourceStream.Write(data, 0, size);
    // write padding
    if ((size & binLiteralTableAlignMask) != 0) {
      var postPad = binLiteralTableAlignMask & (binLiteralTableAlignSize - (size & binLiteralTableAlignMask));
      for (var i = 0; i < postPad; ++i)
        binaryLiteralResourceStream.WriteByte(0);
    }

    td.Attributes |= TypeAttributes.BeforeFieldInit;

    // TODO: modify constructor, add/replace initializer
    var staticCtor = td.GetStaticConstructor();

    if (staticCtor == null) {
      staticCtor = new(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, _trVoid);
      {
        var ilp = staticCtor.Body.GetILProcessor();
        ilp.Emit(OpCodes.Ret);
      }
      td.Methods.Add(staticCtor);
    }

    var fPtr = GetEmbeddedResourceAccessorType(_binLiteralsResourceName);

    var il = staticCtor.Body.Instructions;
    var storeStaticFld = il.FirstOrDefault(ins => ins.OpCode == OpCodes.Stsfld && ins.Operand is FieldReference f && f.Resolve() == field);

    var fieldType = field.FieldType;
    var fieldIsPtr = fieldType.IsPointer || (fieldType.Namespace == "System" && fieldType.Name == "IntPtr");

    {
      var ilp = staticCtor.Body.GetILProcessor();
      if (storeStaticFld == null) {
        //var first = il.First();
        {
          storeStaticFld = ilp.Create(OpCodes.Stsfld, field);
          ilp.InsertBefore(il.First(), storeStaticFld);
          ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Ldsfld, fPtr));
          ilp.InsertBefore(storeStaticFld, CreateLoadConstInt(ilp, offset));
          ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Add));
          if (!fieldIsPtr) {
            var ctor = GetPointerLengthCtor(fieldType, out var ctorParamCount);

            if (ctorParamCount == 2)
              ilp.InsertBefore(storeStaticFld, CreateLoadConstInt(ilp, size));
            ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Newobj, ctor));
          }
        }
      }
      else {
        /*
          var prevInst = storeStaticFld.Previous;
          var prevInstStr = prevInst.OpCode.ToString();
          */

        // TODO: 1 stack depth based removal of prior instructions
        ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Pop));

        ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Ldsfld, fPtr));
        ilp.InsertBefore(storeStaticFld, CreateLoadConstInt(ilp, offset));
        ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Add));
        if (!fieldIsPtr) {
          var ctor = GetPointerLengthCtor(fieldType, out var ctorParamCount);

          if (ctorParamCount == 2)
            ilp.InsertBefore(storeStaticFld, CreateLoadConstInt(ilp, size));
          ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Newobj, ctor));
        }
      }
    }

    staticCtor.Body.Optimize();
#if TRACE
    foreach (var ins in il)
      Trace.WriteLine(ins);
#endif

    // TODO: optional removal of attribute
    field.CustomAttributes.Remove(ca);
  }

}