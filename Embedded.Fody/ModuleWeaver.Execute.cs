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

  public override void Execute() {
    InitializeTypeReferences();

    // TODO: allow duplicate strings
    IDictionary<string, long> utf8StringOffset = new Dictionary<string, long>();

    var embeddedResources = ModuleDefinition.Resources.OfType<EmbeddedResource>().ToArray();

    var existingUtf8StringTable = embeddedResources.FirstOrDefault(r => r.Name == _utf8StringsResourceName);
#if !SUPPORT_EXISTING_UTF8_STRINGS_RESOURCE
    if (existingUtf8StringTable != null)
      throw new NotSupportedException("Appending to an existing string table is not currently supported.");
#else
    var existingUtf8StringTableData = existingUtf8StringTable?.GetResourceData();
    ModuleDefinition.Resources.Remove(existingUtf8StringTable);

    var existingBinLiteralTable = embeddedResources.FirstOrDefault(r => r.Name == binLiteralsResourceName);
    var existingBinLiteralTableData = existingBinLiteralTable?.GetResourceData();
    ModuleDefinition.Resources.Remove(existingBinLiteralTable);

    var utf8StringResourceStreamInitCap = existingUtf8StringTableData?.Length ?? 4096;

    if ((utf8StringResourceStreamInitCap & utf8StringTableAlignMask) != 0) {
      utf8StringResourceStreamInitCap = (
        (utf8StringResourceStreamInitCap + utf8StringTableAlignMask)
        >> utf8StringTableAlignBits
      ) << utf8StringTableAlignBits;
    }

    var utf8StringResourceStream = new MemoryStream(utf8StringResourceStreamInitCap);

    if (existingUtf8StringTableData != null) {
      //utf8StringResourceStream.SetLength(utf8StringResourceStreamInitCap);
      //utf8StringResourceStream.Write(existingUtf8StringTableData, 0, existingUtf8StringTableData.Length);
      var cursor = 0;
      for (;;) {
        // skip any leading or trailing null padding
        while (existingUtf8StringTableData[cursor] == 0)
          continue;

        var terminator = Array.IndexOf(existingUtf8StringTableData, 0, cursor);
        var noMoreTerminators = terminator == -1;

        var size = noMoreTerminators ? existingUtf8StringTableData.Length - cursor : terminator - cursor;
        var s = Encoding.UTF8.GetString(existingUtf8StringTableData, cursor, size);
        utf8StringOffset.Add(s, utf8StringResourceStream.Position);
        utf8StringResourceStream.Write(existingUtf8StringTableData, cursor, size);
        if ((size & utf8StringTableAlignMask) != 0) {
          var pad = utf8StringTableAlignSize - (size & utf8StringTableAlignMask);
          for (var i = 0; i < pad; ++i)
            utf8StringResourceStream.WriteByte(0);
        }

        if (noMoreTerminators) break;

        cursor = terminator + 1;
      }
    }
#endif

#if !SUPPORT_EXISTING_UTF8_STRINGS_RESOURCE
    var utf8StringResourceStream = new MemoryStream(65536);
#endif

    var binaryLiteralResourceStream = new MemoryStream(65536);
    // TODO: configure null terminator

    ICollection<(CustomAttribute, FieldDefinition)> utf8Fields
      = new LinkedList<(CustomAttribute, FieldDefinition)>();

    ICollection<(CustomAttribute, FieldDefinition)> binLitFields
      = new LinkedList<(CustomAttribute, FieldDefinition)>();

    foreach (var type in ModuleDefinition.GetAllTypes()) {
      foreach (var field in type.Fields.Where(field => field.IsStatic).ToArray())
      foreach (var ca in (_attrAsmName == null
        ? field.CustomAttributes
        : field.CustomAttributes
          .Where(ca => ca.AttributeType.Resolve().Module.Assembly.Name.Name == _attrAsmName)).ToArray()) {
        if (ca.AttributeType.Name == _utf8AttrName)
          utf8Fields.Add((ca, field));
        else if (_binLitAttrNames.Contains(ca.AttributeType.Name))
          binLitFields.Add((ca, field));
        else
          throw new NotSupportedException(ca.AttributeType.Name);
      }
    }

    foreach (var (ca, field) in utf8Fields)
      ProcessUtf8Field(ca, field, utf8StringOffset, utf8StringResourceStream);
    foreach (var (ca, field) in binLitFields)
      ProcessBinLitField(ca, field, binaryLiteralResourceStream);

    utf8StringResourceStream.Position = 0;
    if (utf8StringResourceStream.Length > 0)
      ModuleDefinition.Resources.Add(new EmbeddedResource(_utf8StringsResourceName, ManifestResourceAttributes.Private, utf8StringResourceStream));

    binaryLiteralResourceStream.Position = 0;
    if (binaryLiteralResourceStream.Length > 0)
      ModuleDefinition.Resources.Add(new EmbeddedResource(_binLiteralsResourceName, ManifestResourceAttributes.Private, binaryLiteralResourceStream));

    /*
    var attrTypeRefs = utf8Fields.Select(x => x.Item1.AttributeType)
      .Union(binLitFields.Select(x => x.Item1.AttributeType));
    */
  }

}