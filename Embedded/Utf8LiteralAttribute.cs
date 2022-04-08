using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

/// <summary>
/// Represents a UTF-8 string literal.
/// </summary>
/// <remarks>
/// Unless otherwise specified, string literals that are identical will be represented by the same pointer.
/// </remarks>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
[ExcludeFromCodeCoverage]
public sealed class Utf8LiteralAttribute : Attribute {

  /// <summary>
  /// Represents a UTF-8 string literal.
  /// </summary>
  /// <param name="value">
  /// Represents a value of the string literal.
  /// If multiple strings are specified, they are concatenated before being transcoded and embedded.
  /// </param>
  /// <remarks>
  /// Unless otherwise specified, string literals that are identical will be represented by the same pointer.
  /// </remarks>
  public Utf8LiteralAttribute(params string[] value) { }

}