using System;
using Embedded;
using JetBrains.Annotations;

[PublicAPI]
public static unsafe class SomeUtf8StringLiterals {

  [Utf8Literal("Utf8Literal1 String")]
  public static readonly byte* Utf8Literal1;

  [Utf8Literal("Utf8Literal1 String")]
  public static byte* Utf8Literal1Dupe;

  [Utf8Literal("Utf8Literal1 String")]
  public static (IntPtr, int) Utf8Literal1Tuple;

  [Utf8Literal("Utf8Literal2 String")]
  public static readonly byte* Utf8Literal2;

  [Utf8Literal("Utf8Literal2 String")]
  public static byte* Utf8Literal2Dupe;

  [Utf8Literal("Utf8Literal2 String")]
  public static ReadOnlyData<byte> Utf8Literal2ReadOnlyData; 

  [Utf8Literal(null)]
  public static readonly byte* Utf8Literal3;

  public static readonly byte* NonUtf8Literal;

}