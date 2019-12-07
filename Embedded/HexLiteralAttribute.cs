using System;
using System.Diagnostics.CodeAnalysis;
using PublicAPI = JetBrains.Annotations.PublicAPIAttribute;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;
using ItemNotNull = JetBrains.Annotations.ItemNotNullAttribute;

/// <summary>
/// Represents a hexadecimal encoded binary literal.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
[ExcludeFromCodeCoverage]
public sealed class HexLiteralAttribute : Attribute {

  /// <summary>
  /// Represents a hexadecimal encoded binary literal.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="data">
  /// The literal data represented by hexadecimal text.
  /// If multiple strings are specified, they are concatenated before being parsed.
  /// </param>
  public HexLiteralAttribute(int alignBits, [NotNull,ItemNotNull]params string[] data) {
  }

}