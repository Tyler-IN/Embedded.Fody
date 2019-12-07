using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

public sealed partial class ModuleWeaver : BaseModuleWeaver {

  private TypeReference _trType;

  private TypeReference _trAssembly;

  private TypeReference _trStream;

  private TypeReference _trUnmanagedMemoryStream;

  private PointerType _trBytePtr;

  private TypeReference _trObject;

  private TypeReference _trInt;

  private TypeReference _trVoid;

  private GenericInstanceType _trReadOnlySpanByte;

  private readonly string _attrAsmName;

  private readonly string _utf8AttrName;

  private readonly string _binLitAttrName;

  private readonly string _hexLitAttrName;

  private readonly string _b64LitAttrName;

  private readonly string _encLitAttrName;

  private readonly string _utf8StringsResourceName;

  private readonly string _binLiteralsResourceName;

  private readonly int _utf8StringTableAlignBits;

  private readonly long _utf8StringTableAlignSize;

  private readonly long _utf8StringTableAlignMask;

  private readonly int _defBinLiteralTableAlignBits;

  private readonly HashSet<string> _binLitAttrNames;

  public ModuleWeaver() {
    _attrAsmName = GetStringConfigValueOrDefault("customAttributeAssemblyName", "Embedded");
    _utf8AttrName = GetStringConfigValueOrDefault("utf8CustomAttributeName", "Utf8LiteralAttribute");
    _binLitAttrName = GetStringConfigValueOrDefault("valueLiteralCustomAttributeName",  "ValueLiteralAttribute");
    _hexLitAttrName = GetStringConfigValueOrDefault("hexadecimalLiteralCustomAttributeName", "HexLiteralAttribute");
    _b64LitAttrName = GetStringConfigValueOrDefault("base64LiteralCustomAttributeName", "Base64LiteralAttribute");
    _encLitAttrName = GetStringConfigValueOrDefault("encodedLiteralCustomAttributeName", "EncodedLiteralAttribute");
    _utf8StringsResourceName = GetStringConfigValueOrDefault("utf8StringsResourceName", "UTF8_STRINGS");
    _binLiteralsResourceName = GetStringConfigValueOrDefault("binaryLiteralResourceName", "RDATA");
    _utf8StringTableAlignBits = (int) GetIntegerConfigValueOrDefault("utf8StringsAlignBits", 4);
    _utf8StringTableAlignSize = 1L << _utf8StringTableAlignBits; // 1<<4 == 16
    _utf8StringTableAlignMask = _utf8StringTableAlignSize - 1;
    _defBinLiteralTableAlignBits = (int) GetIntegerConfigValueOrDefault("defaultBinaryLiteralAlignBits", 2); // 1<<2 == 4

    _binLitAttrNames = new HashSet<string> {
      _binLitAttrName,
      _hexLitAttrName,
      _b64LitAttrName,
      _encLitAttrName
    };
  }

  private void InitializeTypeReferences() {
    _trInt = TypeSystem.Int32Reference;
    _trObject = TypeSystem.ObjectReference;
    _trVoid = TypeSystem.VoidReference;

    _trType = ModuleDefinition.ImportReference(new TypeReference("System", "Type", _trVoid.Module, _trVoid.Scope, false));
    _trAssembly = ModuleDefinition.ImportReference(new TypeReference("System.Reflection", "Assembly", _trVoid.Module, _trVoid.Scope, false));
    _trStream = ModuleDefinition.ImportReference(new TypeReference("System.IO", "Stream", _trVoid.Module, _trVoid.Scope, false));
    _trUnmanagedMemoryStream = ModuleDefinition.ImportReference(new TypeReference("System.IO", "UnmanagedMemoryStream", _trVoid.Module, _trVoid.Scope, false));
    _trBytePtr = TypeSystem.ByteReference.MakePointerType();
    _trReadOnlySpanByte = ModuleDefinition.GetType("System.ReadOnlySpan")?.MakeGenericInstanceType(TypeSystem.ByteReference);

    /*
    _trUtf8Attr = GetTypeReference(_attrAsmName, _utf8AttrName);
    _trbinLitAttr = GetTypeReference(_attrAsmName, _binLitAttrName);
    _trhexLitAttr = GetTypeReference(_attrAsmName, _hexLitAttrName);
    _trb64LitAttr = GetTypeReference(_attrAsmName, _b64LitAttrName);
    _trEncLitAttr = GetTypeReference(_attrAsmName, _encLitAttrName);
    */
  }

  public string GetStringConfigValueOrDefault(string attributeName, string defaultValue) {
    var valueStr = Config?.Attribute(attributeName)?.Value?.Trim();
    if (string.IsNullOrEmpty(valueStr))
      valueStr = null;
    return valueStr ?? defaultValue;
  }

  public long GetIntegerConfigValueOrDefault(string attributeName, long defaultValue) {
    var valueStr = GetStringConfigValueOrDefault(attributeName, null);
    if (valueStr != null && long.TryParse(valueStr, out var parsed))
      return parsed;

    return defaultValue;
  }

  public override IEnumerable<string> GetAssembliesForScanning() {
    yield return "netstandard";
    yield return "mscorlib";

    if (!string.IsNullOrEmpty(_attrAsmName))
      yield return _attrAsmName;
  }

  public override bool ShouldCleanReference
    // don't clean it if the module contains any non-attribute references
    => !ModuleDefinition.GetTypeReferences()
      .Select(tr => tr.Resolve())
      .Any(td => {
        if (td.Module.Assembly.Name.Name != "Embedded")
          return false;

        var trBase = td.BaseType;
        return trBase == null || !(trBase.Namespace == "System" && trBase.Name == "Attribute");
      });

}