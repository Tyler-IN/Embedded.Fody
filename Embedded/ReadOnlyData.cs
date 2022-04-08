#pragma warning disable 809
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Embedded; 

/// <summary>
/// Represents a data pointer and length combination with the explicit intent of being readonly.
/// </summary>
/// <typeparam name="T"></typeparam>
[PublicAPI]
[DebuggerDisplay("{" + nameof(GetDebugDisplay) + "()}")]
public readonly unsafe struct ReadOnlyData<T> where T : unmanaged {

  private readonly IntPtr _ptr;

  private readonly int _length;

  /// <summary>
  /// The native integer pointer value.
  /// </summary>
  public IntPtr Pointer => _ptr;

  /// <summary>
  /// The generic typed pointer value.
  /// </summary>
  public T* TypedPointer => (T*) _ptr;

  /// <summary>
  /// The length of the data represented by the pointer.
  /// This represents the size in count of <typeparamref name="T" />, not in bytes.
  /// </summary>
  public int Length => _length;

  /// <summary>
  /// The length of the data represented by the pointer in bytes.
  /// </summary>
  public long Size => _length * sizeof(T);

  /// <summary>
  /// Represents a data pointer and length combination with the explicit intent of being readonly.
  /// </summary>
  /// <param name="ptr">
  /// A generic typed pointer value.
  /// </param>
  /// <param name="length">
  /// The length of the data represented by the pointer.
  /// This represents the size in count of <typeparamref name="T" />, not in bytes.
  /// </param>
  public ReadOnlyData(T* ptr, int length) {
    _ptr = (IntPtr) ptr;
    _length = length;
  }

  /// <summary>
  /// Implicit casting of a <see cref="ReadOnlyData{T}"/> to a pointer of <typeparamref name="T"/>.
  /// Same behavior as accessing <see cref="TypedPointer"/>.
  /// </summary>
  public static implicit operator T*(ReadOnlyData<T> d)
    => (T*) d._ptr;

  /// <summary>
  /// Equality comparison operator for <see cref="ReadOnlyData{T}"/>s.
  /// </summary>
  /// <param name="lhs">The left-hand-side value.</param>
  /// <param name="rhs">The right-hand-side value.</param>
  /// <returns><see langword="true"/> if equal, otherwise <see langword="false"/>.</returns>
  public static bool operator ==(in ReadOnlyData<T> lhs, in ReadOnlyData<T> rhs)
    => lhs._ptr == rhs._ptr && lhs._length == rhs._length;

  /// <summary>
  /// Inequality comparison operator for <see cref="ReadOnlyData{T}"/>s.
  /// </summary>
  /// <param name="lhs">The left-hand-side value.</param>
  /// <param name="rhs">The right-hand-side value.</param>
  /// <returns><see langword="true"/> if unequal, otherwise <see langword="false"/>.</returns>
  public static bool operator !=(in ReadOnlyData<T> lhs, in ReadOnlyData<T> rhs)
    => lhs._ptr != rhs._ptr || lhs._length != rhs._length;

  /// <summary>
  /// Equals() on ReadOnlyData will always throw an exception. Use == instead.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [Obsolete("Equals() on ReadOnlyData will always throw an exception. Use == instead.", true)]
  public override bool Equals(object? obj)
    => throw new NotSupportedException("Equals is not supported on ReadOnlyData structs for the same reasons as ReadOnlySpan.");

  /// <summary>
  /// GetHashCode() on ReadOnlyData will always throw an exception.
  /// </summary>
  [ExcludeFromCodeCoverage]
  [Obsolete("GetHashCode() on ReadOnlyData will always throw an exception.")]
  public override int GetHashCode()
    => throw new NotSupportedException("GetHashCode is not supported on ReadOnlyData structs for the same reasons as ReadOnlySpan.");

  /// <summary>
  /// Implicit casting of a <see cref="ReadOnlyData{T}"/> to a <see cref="ValueTuple{T1,T2}"/> of an <see cref="IntPtr"/> and an <see cref="Int32"/>.
  /// The <see cref="IntPtr"/> is the data pointer.
  /// The <see cref="Int32"/> is the length in count of <typeparamref name="T"/>.
  /// </summary>
  // TODO: theoretically this could be directly reinterpreted?
  public static implicit operator (IntPtr, int)(ReadOnlyData<T> d)
    => (d._ptr, d._length);

  /// <summary>
  /// Implicit casting of a <see cref="ReadOnlyData{T}"/> to a <see cref="ValueTuple{T1,T2}"/> of an <see cref="IntPtr"/> and an <see cref="Int64"/>.
  /// The <see cref="IntPtr"/> is the data pointer.
  /// The <see cref="Int64"/> is the length in count of <typeparamref name="T"/>.
  /// </summary>
  public static implicit operator (IntPtr, long)(ReadOnlyData<T> d)
    => (d._ptr, d._length);

  /// <summary>
  /// Allows between-generic-type casting of <see cref="ReadOnlyData{T}"/> with different <typeparamref name="T"/>.
  /// </summary>
  /// <typeparam name="TOther">A type other than <typeparamref name="T"/>.</typeparam>
  /// <returns>A <see cref="ReadOnlyData{T}"/> of type <typeparamref name="TOther"/> with an accounted-for length conversion.</returns>
  public ReadOnlyData<TOther> Recast<TOther>() where TOther : unmanaged {
    var otherTSize = sizeof(TOther);
    return new((TOther*) _ptr, checked((int) Size / otherTSize));
  }

  /// <summary>
  /// Implicit casting of a <see cref="ReadOnlyData{T}"/> to a <see cref="ReadOnlyData{T}"/>/>.
  /// </summary>
  // TODO: theoretically this could be directly reinterpreted as a heap-resident ReadOnlySpan?
  public static implicit operator ReadOnlySpan<T>(ReadOnlyData<T> d)
    => new(d, d._length);

  private static string _toStringCache
    = $"ReadOnlyData<{typeof(T).Name}>";

  /// <inheritdoc/>
  public override string ToString()
    => _toStringCache;

  internal string GetDebugDisplay() {
    var t = _toStringCache;
    var ptrStr = "0x" + _ptr.ToString(IntPtr.Size == 8 ? "X16" : "X8");
    if (typeof(T) == typeof(char))
      return $"{t} @ 0x{ptrStr}: {System.Net.WebUtility.UrlEncode(new((char*) TypedPointer, 0, _length))}";
    if (typeof(T) == typeof(byte) || typeof(T) == typeof(sbyte))
      return $"{t} @ 0x{ptrStr}: {System.Net.WebUtility.UrlEncode(new((sbyte*) TypedPointer, 0, _length))}";

    return $"{t} @ 0x{ptrStr}: {sizeof(T)}x{_length}";
  }

  /// <summary>
  /// Allows accessing <typeparamref name="T"/>s by an <paramref name="offset"/> contained in the represented data.
  /// </summary>
  /// <param name="offset">An offset into the represented data.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> was outside of the represented data.</exception>
  public ref T this[int offset] {
    get {
      if (offset < 0 || offset >= _length)
        throw new ArgumentOutOfRangeException(nameof(offset));

      return ref TypedPointer[offset];
    }
  }

  /// <summary>
  /// Allows accessing <typeparamref name="T"/>s by an <paramref name="offset"/> contained in the represented data.
  /// </summary>
  /// <param name="offset">An offset into the represented data.</param>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> was outside of the represented data.</exception>
  public ref T this[long offset] {
    get {
      if (offset < 0 || offset >= _length)
        throw new ArgumentOutOfRangeException(nameof(offset));

      return ref TypedPointer[offset];
    }
  }

}