using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

public sealed partial class ModuleWeaver {

  private void ProcessUtf8Field(CustomAttribute ca, FieldDefinition field, IDictionary<string, long> utf8StringOffset, MemoryStream utf8StringResourceStream) {
    var td = field.DeclaringType;

    string s;
    if (ca.ConstructorArguments[0].Value is CustomAttributeArgument[] a)
      s = a[0].Value as string;
    else
      s = (string) ca.ConstructorArguments[0].Value;

    if (s == null) {
      // TODO: optional removal of attribute
      field.CustomAttributes.Remove(ca);
      return;
    }

    // this will probably never be used, but here for paranoia
    var prePad = _utf8StringTableAlignMask & (_utf8StringTableAlignSize - (utf8StringResourceStream.Position & _utf8StringTableAlignMask));
    for (var i = 0; i < prePad; ++i)
      utf8StringResourceStream.WriteByte(0);

    int size;
    if (!utf8StringOffset.TryGetValue(s, out var offset)) {
      utf8StringOffset.Add(s, offset = utf8StringResourceStream.Position);
      var utf8 = Encoding.UTF8.GetBytes(s);
      size = utf8.Length;

      // write utf8 string
      utf8StringResourceStream.Write(utf8, 0, size);

      // write null terminator too
      utf8StringResourceStream.WriteByte(0);

      var sizeTermed = size + 1;

      // write alignment padding
      if ((sizeTermed & _utf8StringTableAlignMask) != 0) {
        var pad = _utf8StringTableAlignMask & (_utf8StringTableAlignSize - (sizeTermed & _utf8StringTableAlignMask));
        for (var i = 0; i < pad; ++i)
          utf8StringResourceStream.WriteByte(0);
      }
    }
    else {
      size = Encoding.UTF8.GetByteCount(s);
    }

    var fPtr = GetEmbeddedResourceAccessorType(_utf8StringsResourceName);

    td.Attributes |= TypeAttributes.BeforeFieldInit;

    // TODO: modify constructor, add/replace initializer
    var staticCtor = td.GetStaticConstructor();

    if (staticCtor == null) {
      staticCtor = new MethodDefinition(".cctor", MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName | MethodAttributes.HideBySig, _trVoid);
      {
        var ilp = staticCtor.Body.GetILProcessor();
        ilp.Emit(OpCodes.Ret);
      }
      td.Methods.Add(staticCtor);
    }

    var il = staticCtor.Body.Instructions;

    var storeStaticFld = il.FirstOrDefault(ins => ins.OpCode == OpCodes.Stsfld && ins.Operand is FieldReference f && f.Resolve() == field);

    var fieldType = field.FieldType;
    var fieldIsPtr = fieldType.IsPointer || (fieldType.Namespace == "System" && fieldType.Name == "IntPtr");

    {
      var ilp = staticCtor.Body.GetILProcessor();
      if (storeStaticFld == null) {
        var first = il.First();

        {
          storeStaticFld = ilp.Create(OpCodes.Stsfld, field);
          ilp.InsertBefore(il.First(), storeStaticFld);
          ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Ldsfld, fPtr));
          //ilp.InsertBefore(storeStaticFld, ilp.Create(OpCodes.Newobj));
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