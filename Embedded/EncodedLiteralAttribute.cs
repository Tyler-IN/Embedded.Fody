using System;
using System.Diagnostics.CodeAnalysis;
using PublicAPI = JetBrains.Annotations.PublicAPIAttribute;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;
using ItemNotNull = JetBrains.Annotations.ItemNotNullAttribute;

/// <summary>
/// Represents a encoded text binary literal.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
[ExcludeFromCodeCoverage]
public sealed class EncodedLiteralAttribute : Attribute {

  /// <summary>
  /// Represents a encoded text binary literal.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="codePage">
  /// The code page to transcode the text into for embedding.
  /// </param>
  /// <param name="text">
  /// The literal data represented in text.
  /// If multiple strings are specified, they are concatenated before being transcoded.
  /// </param>
  public EncodedLiteralAttribute(int alignBits, int codePage, [NotNull, ItemNotNull] params string[] text) {
  }

}