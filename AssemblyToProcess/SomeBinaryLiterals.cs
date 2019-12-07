using System;
using Embedded;
using JetBrains.Annotations;

[PublicAPI]
public static unsafe class SomeBinaryLiterals {

  public static readonly byte* BinNonLiteral;

  [ValueLiteral(0, new byte[] {1, 2, 3})]
  public static readonly byte* BinLiteral1;

  [ValueLiteral(4, new byte[] {15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1})]
  public static readonly byte* BinLiteral2;

  [ValueLiteral(0, 0x04030201)]
  public static readonly IntPtr BinLiteral3;

  [ValueLiteral(4, new byte[] {15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1})]
  public static byte* BinLiteral4;

  [ValueLiteral(3, 0x01020304u)]
  public static readonly (IntPtr, int) BinLiteral5;

  [ValueLiteral(3, new byte[] {1, 2, 3})]
  public static readonly ReadOnlyData<byte> BinLiteral6;

  [Base64Literal(0, "AQ==")]
  public static readonly byte* BinLiteral7; // [ 0x01 ]

  [Base64Literal(0, "Vao=")]
  public static readonly ushort* BinLiteral8; // [ 0x55, 0xAA ]

  [HexLiteral(0, "0x1234")]
  public static readonly byte* BinLiteral9; // [ 0x12, 0x34 ]

  [HexLiteral(0, "1234")]
  public static readonly byte* BinLiteral10; // [ 0x12, 0x34 ]

  [HexLiteral(0, "12 34")]
  public static readonly byte* BinLiteral11; // [ 0x12, 0x34 ]

  [HexLiteral(0, "ABCD")]
  public static readonly byte* BinLiteral12; // [ 0xAB, 0xCD ]

  [HexLiteral(0, "abcd")]
  public static readonly (IntPtr, int) BinLiteral13; // [ 0xAB, 0xCD ]

  [HexLiteral(0, "aBcD", "Ef01")]
  public static readonly byte* BinLiteral14; // [ 0xAB, 0xCD, 0xEF, 0x01 ]

  [EncodedLiteral(0, 28591, "This is ISO-8559-1.\0")]
  public static readonly (IntPtr,long) BinLiteral15;

  [EncodedLiteral(0, 65000, "This is UTF-7.\0")]
  public static readonly (IntPtr,int) BinLiteral16;

  [Base64Literal(0,
    "MBu0Iclx+", "7ftAdzDqZ", "ds5T3wNAI", "rqYK5fQ8n", "1IxPA4g6q",
    "/fGvHeKp8", "ODBi9oIwR", "abUG4pyCv", "u4qWB2kPi", "fvhpw=="
  )]
  public static readonly ReadOnlyData<byte> BinLiteral17; // SHA3-512 of "Test"

  public static readonly byte* NonBinLiteral;

}