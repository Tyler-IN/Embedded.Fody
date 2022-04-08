using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using JetBrains.Annotations;

/// <summary>
/// Represents a Base64 encoded binary literal.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
[ExcludeFromCodeCoverage]
public sealed class Base64LiteralAttribute : Attribute {

  /// <summary>
  /// Represents a Base64 encoded binary literal.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="data">
  /// The literal data represented by Base64 text.
  /// If multiple strings are specified, they are concatenated before being parsed.
  /// </param>
  public Base64LiteralAttribute(int alignBits, params string[] data) { }

}