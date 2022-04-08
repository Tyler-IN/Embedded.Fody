using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Fody;
using JetBrains.Annotations;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using System.Diagnostics.CodeAnalysis;

[PublicAPI]
public sealed partial class ModuleWeaver : BaseModuleWeaver {

#if DEBUG
  static ModuleWeaver()
    => AppDomain.CurrentDomain.FirstChanceException += (_, args) => {
      var thisModule = typeof(ModuleWeaver).Module;
      var ex = args.Exception;
      var st = new StackTrace(ex, true);
      var fc = st.FrameCount;
      var relevant = false;
      for (var i = 0; i < fc; ++i) {
        var f = st.GetFrame(i);
        if (f.GetMethod()?.Module == thisModule)
          relevant = true;
      }

      if (!relevant)
        return;

      if (ex is NullReferenceException) {
        if (!Debugger.IsAttached)
          Debugger.Launch();
        Debugger.Break();
      }
      Console.Error.WriteLine(st);
    };
#endif

  private TypeReference _trType = null!;

  private TypeReference _trAssembly = null!;

  private TypeReference _trStream = null!;

  private TypeReference _trUnmanagedMemoryStream = null!;

  private PointerType _trBytePtr = null!;

  private TypeReference _trObject = null!;

  private TypeReference _trInt = null!;

  private TypeReference _trVoid = null!;

  private GenericInstanceType _trReadOnlySpanByte = null!;

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
    _binLitAttrName = GetStringConfigValueOrDefault("valueLiteralCustomAttributeName", "ValueLiteralAttribute");
    _hexLitAttrName = GetStringConfigValueOrDefault("hexadecimalLiteralCustomAttributeName", "HexLiteralAttribute");
    _b64LitAttrName = GetStringConfigValueOrDefault("base64LiteralCustomAttributeName", "Base64LiteralAttribute");
    _encLitAttrName = GetStringConfigValueOrDefault("encodedLiteralCustomAttributeName", "EncodedLiteralAttribute");
    _utf8StringsResourceName = GetStringConfigValueOrDefault("utf8StringsResourceName", "UTF8_STRINGS");
    _binLiteralsResourceName = GetStringConfigValueOrDefault("binaryLiteralResourceName", "RDATA");
    _utf8StringTableAlignBits = (int)GetIntegerConfigValueOrDefault("utf8StringsAlignBits", 4);
    _utf8StringTableAlignSize = 1L << _utf8StringTableAlignBits; // 1<<4 == 16
    _utf8StringTableAlignMask = _utf8StringTableAlignSize - 1;
    _defBinLiteralTableAlignBits = (int)GetIntegerConfigValueOrDefault("defaultBinaryLiteralAlignBits", 2); // 1<<2 == 4

    _binLitAttrNames = new() {
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
    _trBytePtr = TypeSystem.ByteReference.MakePointerType();
    _trReadOnlySpanByte = (GenericInstanceType)ModuleDefinition.ImportReference((
        ModuleDefinition.GetType("System.ReadOnlySpan`1")
        ?? FindTypeDefinition("System.ReadOnlySpan`1")
      )
      .MakeGenericInstanceType(TypeSystem.ByteReference)!);

    _trType = ModuleDefinition.ImportReference(new TypeReference("System", "Type", _trVoid.Module, _trVoid.Scope, false));
    if (_trType.Resolve() == null)
      _trType = ModuleDefinition.ImportReference(typeof(Type));
    _trAssembly = ModuleDefinition.ImportReference(new TypeReference("System.Reflection", "Assembly", _trVoid.Module, _trVoid.Scope, false));
    if (_trAssembly.Resolve() == null)
      _trAssembly = ModuleDefinition.ImportReference(typeof(Assembly));
    _trStream = ModuleDefinition.ImportReference(new TypeReference("System.IO", "Stream", _trVoid.Module, _trVoid.Scope, false));
    if (_trStream.Resolve() == null)
      _trStream = ModuleDefinition.ImportReference(typeof(Stream));
    _trUnmanagedMemoryStream = ModuleDefinition.ImportReference(new TypeReference("System.IO", "UnmanagedMemoryStream", _trVoid.Module, _trVoid.Scope, false));
    if (_trUnmanagedMemoryStream.Resolve() == null)
      _trUnmanagedMemoryStream = ModuleDefinition.ImportReference(typeof(UnmanagedMemoryStream));

    /*
    _trUtf8Attr = GetTypeReference(_attrAsmName, _utf8AttrName);
    _trbinLitAttr = GetTypeReference(_attrAsmName, _binLitAttrName);
    _trhexLitAttr = GetTypeReference(_attrAsmName, _hexLitAttrName);
    _trb64LitAttr = GetTypeReference(_attrAsmName, _b64LitAttrName);
    _trEncLitAttr = GetTypeReference(_attrAsmName, _encLitAttrName);
    */
  }

  [ContractAnnotation("defaultValue:notnull => notnull; defaultValue:null => canbenull")]
#if NETSTANDARD2_1
  [return: NotNullIfNotNull("defaultValue")]
  public string? GetStringConfigValueOrDefault(string attributeName, string? defaultValue) {
#else
  public string GetStringConfigValueOrDefault(string attributeName, string? defaultValue) {
#endif
    if (attributeName is null)
      throw new ArgumentNullException(nameof(attributeName));

    // ReSharper disable once ConstantConditionalAccessQualifier
    var valueStr = Config?.Attribute(attributeName)?.Value?.Trim();
    if (string.IsNullOrEmpty(valueStr))
      valueStr = null;
#if NETSTANDARD2_1
    return valueStr ?? defaultValue;
#else
    return valueStr ?? defaultValue!;
#endif
  }

  public long GetIntegerConfigValueOrDefault(string attributeName, long defaultValue) {
    var valueStr = GetStringConfigValueOrDefault(attributeName, null);
    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
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