using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

/// <summary>
/// Represents a binary literal.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Field)]
[ExcludeFromCodeCoverage]
public sealed class ValueLiteralAttribute : Attribute {

  /// <summary>
  /// A literal array of <see cref="SByte"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="SByte"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params sbyte[] values) { }

  /// <summary>
  /// A literal array of <see cref="Byte"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="Byte"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params byte[] values) { }

  /// <summary>
  /// A literal array of <see cref="Int16"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="Int16"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params short[] values) { }

  /// <summary>
  /// A literal array of <see cref="UInt16"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="UInt16"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params ushort[] values) { }

  /// <summary>
  /// A literal array of <see cref="Int32"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="Int32"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params int[] values) { }

  /// <summary>
  /// A literal array of <see cref="UInt32"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="UInt32"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params uint[] values) { }

  /// <summary>
  /// A literal array of <see cref="Int64"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="Int64"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params long[] values) { }

  /// <summary>
  /// A literal array of <see cref="UInt64"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="UInt64"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params ulong[] values) { }

  /// <summary>
  /// A literal array of <see cref="Single"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="Single"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params float[] values) { }

  /// <summary>
  /// A literal array of <see cref="Double"/>.
  /// </summary>
  /// <param name="alignBits">
  /// The least significant bits to align to.
  /// Raise this number to a power of two to determine the alignment boundary in bytes.
  /// (0: unaligned, 1: 2-byte, 2: 4-byte, 3: 8-byte, 4: 16-byte, ...)
  /// </param>
  /// <param name="values">
  /// An array of <see cref="Double"/>.
  /// Alignment is not applied between values.
  /// </param>
  public ValueLiteralAttribute(int alignBits, params double[] values) { }

}