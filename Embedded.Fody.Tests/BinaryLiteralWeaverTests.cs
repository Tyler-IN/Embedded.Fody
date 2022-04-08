using System;
using System.IO;
using System.Reflection;
using System.Text;
using Embedded;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

[PublicAPI]
[Collection("WeaverTests")]
public unsafe class BinaryLiteralWeaverTests : WeaverTests {

  public BinaryLiteralWeaverTests(ITestOutputHelper outputHelper, TestResultFixture fixture)
    : base(outputHelper, fixture) { }

  [Fact]
  public void ValidateBinLiteralsResource() {
    var ums = (UnmanagedMemoryStream)TestResult.Assembly.GetManifestResourceStream("RDATA")!;
    Assert.NotNull(ums);
    var bufSize = ums.Length;
    Assert.NotEqual(0, bufSize);
    var buffer = new byte[bufSize];
    Assert.Equal(bufSize, ums.Read(buffer, 0, checked((int)bufSize)));
    //OutputHelper.WriteLine(BitConverter.ToString(buffer));
  }

  [Fact]
  public void ValidateBinLiteral1() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral1))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(1, v[0]);
    Assert.Equal(2, v[1]);
    Assert.Equal(3, v[2]);
    Assert.Equal(0, v[3]); // next item's alignment pre-padding
    Assert.Equal(0, v[4]);
    Assert.Equal(0, v[5]);
    Assert.Equal(0, v[6]);
    Assert.Equal(0, v[7]);
    Assert.Equal(0, v[8]);
    Assert.Equal(0, v[9]);
    Assert.Equal(0, v[10]);
    Assert.Equal(0, v[11]);
    Assert.Equal(0, v[12]);
    Assert.Equal(0, v[13]);
    Assert.Equal(0, v[14]);
    Assert.Equal(0, v[15]);
    Assert.Equal(15, v[16]); // bleed into next defined item after it's alignment
  }

  [Fact]
  public void ValidateBinLiteral2() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral2))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(15, v[0]);
    Assert.Equal(14, v[1]);
    Assert.Equal(13, v[2]);
    Assert.Equal(12, v[3]);
    Assert.Equal(11, v[4]);
    Assert.Equal(10, v[5]);
    Assert.Equal(9, v[6]);
    Assert.Equal(8, v[7]);
    Assert.Equal(7, v[8]);
    Assert.Equal(6, v[9]);
    Assert.Equal(5, v[10]);
    Assert.Equal(4, v[11]);
    Assert.Equal(3, v[12]);
    Assert.Equal(2, v[13]);
    Assert.Equal(1, v[14]);
    Assert.Equal(0, v[15]); // due to alignment requirement
    Assert.Equal(1, v[16]); // bleed into next defined item after it's alignment
  }

  [Fact]
  public void ValidateBinLiteral3() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (IntPtr)t.GetField(nameof(SomeBinaryLiterals.BinLiteral3))!.GetValue(null)!;
    var p = (byte*)v;
    Assert.NotEqual((IntPtr)0, v);
    Assert.Equal(1, p[0]);
    Assert.Equal(2, p[1]);
    Assert.Equal(3, p[2]);
    Assert.Equal(4, p[3]);
    Assert.Equal(0, p[4]); // next item's alignment pre-padding
  }

  [Fact]
  public void ValidateBinLiteral4() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral4))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(15, v[0]);
    Assert.Equal(14, v[1]);
    Assert.Equal(13, v[2]);
    Assert.Equal(12, v[3]);
    Assert.Equal(11, v[4]);
    Assert.Equal(10, v[5]);
    Assert.Equal(9, v[6]);
    Assert.Equal(8, v[7]);
    Assert.Equal(7, v[8]);
    Assert.Equal(6, v[9]);
    Assert.Equal(5, v[10]);
    Assert.Equal(4, v[11]);
    Assert.Equal(3, v[12]);
    Assert.Equal(2, v[13]);
    Assert.Equal(1, v[14]);
    Assert.Equal(0, v[15]); // due to alignment requirement
  }

  [Fact]
  public void ValidateBinLiteral5() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var (v, l) = ((IntPtr, int))t.GetField(nameof(SomeBinaryLiterals.BinLiteral5))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, v);
    Assert.Equal(4, l);
    var p = (byte*)v;
    Assert.Equal(4, p[0]);
    Assert.Equal(3, p[1]);
    Assert.Equal(2, p[2]);
    Assert.Equal(1, p[3]);
  }

  [Fact]
  public void ValidateBinLiteral6() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (ReadOnlyData<byte>)t.GetField(nameof(SomeBinaryLiterals.BinLiteral6))!.GetValue(null)!;
    var x = v.Pointer;
    Assert.NotEqual((IntPtr)0, x);
    var p = (byte*)v;
    Assert.Equal(x, (IntPtr)p);
    Assert.Equal(x, (IntPtr)v.TypedPointer);
    Assert.Equal(1, p[0]);
    Assert.Equal(2, p[1]);
    Assert.Equal(3, p[2]);
  }

  [Fact]
  public void ValidateBinLiteral7() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral7))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0x01, v[0]);
  }

  [Fact]
  public void ValidateBinLiteral8() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (ushort*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral8))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0xAA55, v[0]);
  }

  [Fact]
  public void ValidateBinLiteral9() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral9))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0x12, v[0]);
    Assert.Equal(0x34, v[1]);
  }

  [Fact]
  public void ValidateBinLiteral10() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral10))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0x12, v[0]);
    Assert.Equal(0x34, v[1]);
  }

  [Fact]
  public void ValidateBinLiteral11() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral11))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0x12, v[0]);
    Assert.Equal(0x34, v[1]);
  }

  [Fact]
  public void ValidateBinLiteral12() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral12))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0xAB, v[0]);
    Assert.Equal(0xCD, v[1]);
  }

  [Fact]
  public void ValidateBinLiteral13() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var (v, l) = ((IntPtr, int))t.GetField(nameof(SomeBinaryLiterals.BinLiteral13))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    var p = (byte*)v;
    Assert.Equal(0xAB, p[0]);
    Assert.Equal(0xCD, p[1]);
  }

  [Fact]
  public void ValidateBinLiteral14() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinLiteral14))!.GetValue(null)!);
    Assert.NotEqual((IntPtr)0, (IntPtr)v);
    Assert.Equal(0xAB, v[0]);
    Assert.Equal(0xCD, v[1]);
    Assert.Equal(0xEF, v[2]);
    Assert.Equal(0x01, v[3]);
  }

  [Fact]
  public unsafe void ValidateBinLiteral15() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var (v, l) = ((IntPtr, long))t.GetField(nameof(SomeBinaryLiterals.BinLiteral15))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, v);
    Assert.Equal(20, l);
    var s = Encoding.GetEncoding(28591).GetString((byte*)v, (int)l);
    Assert.Equal("This is ISO-8559-1.\0", s);
  }

  /*
  [Fact]
  public unsafe void ValidateBinLiteral16() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var (v, l) = ((IntPtr, int))t.GetField(nameof(SomeBinaryLiterals.BinLiteral16))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, v);
    Assert.Equal(19, l);
    var s = Encoding.GetEncoding(65000).GetString((byte*)v, l);
    Assert.Equal("This is UTF-7.\0", s);
  }
  */

  [Fact]
  public unsafe void ValidateBinLiteral17() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (ReadOnlyData<byte>)t.GetField(nameof(SomeBinaryLiterals.BinLiteral17))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, v.Pointer);
    Assert.Equal(64, v.Length);
    // SHA3-512 of "Test"
    Assert.Equal(new byte[] {
      0x30, 0x1b, 0xb4, 0x21, 0xc9, 0x71, 0xfb, 0xb7, 0xed, 0x01, 0xdc, 0xc3, 0xa9, 0x97, 0x6c, 0xe5,
      0x3d, 0xf0, 0x34, 0x02, 0x2b, 0xa9, 0x82, 0xb9, 0x7d, 0x0f, 0x27, 0xd4, 0x8c, 0x4f, 0x03, 0x88,
      0x3a, 0xab, 0xf7, 0xc6, 0xbc, 0x77, 0x8a, 0xa7, 0xc3, 0x83, 0x06, 0x2f, 0x68, 0x23, 0x04, 0x5a,
      0x6d, 0x41, 0xb8, 0xa7, 0x20, 0xaf, 0xbb, 0x8a, 0x96, 0x07, 0x69, 0x0f, 0x89, 0xfb, 0xe1, 0xa7
    }, ((ReadOnlySpan<byte>)v).ToArray());
  }

  [Fact]
  public unsafe void ValidateReadOnlyData() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v6 = (ReadOnlyData<byte>)t.GetField(nameof(SomeBinaryLiterals.BinLiteral6))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, v6.Pointer);
    var v17 = (ReadOnlyData<byte>)t.GetField(nameof(SomeBinaryLiterals.BinLiteral17))!.GetValue(null)!;
    Assert.NotEqual((IntPtr)0, v17.Pointer);
    Assert.NotEqual(v6.Pointer, v17.Pointer);
    Assert.True(v6 != v17);
    Assert.False(v6 == v17);
    var v6str = v6.ToString();
    Assert.NotNull(v6str);
    Assert.NotEmpty(v6str);
    var s = (ReadOnlySpan<byte>)v17;
    Assert.Equal(s.Length, v17.Length);
    Assert.Equal(s[0], v17[0]);
    Assert.Equal(v17[0], v17[0L]);
    var ti = ((IntPtr p, int l))v17;
    var tl = ((IntPtr p, long l))v17;
    var rc = v17.Recast<sbyte>();
    var us = v17.Recast<ushort>();
    Assert.Equal(v17.Pointer, ti.p);
    Assert.Equal(v17.Length, ti.l);
    Assert.Equal(v17.Pointer, tl.p);
    Assert.Equal(v17.Length, tl.l);
    Assert.Equal(v17.Pointer, rc.Pointer);
    Assert.Equal(v17.Length, rc.Length);
    Assert.Equal(v17.Pointer, us.Pointer);
    Assert.Equal(v17.Length / 2, us.Length);
    var dd = v17.GetDebugDisplay();
    OutputHelper.WriteLine(dd);
    Assert.NotNull(dd);
    Assert.NotEmpty(dd);
  }

  [Fact]
  public void ValidateNonBinLiterals() {
    var t = TestResult.Assembly.GetType(nameof(SomeBinaryLiterals))!;
    var v = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.NonBinLiteral))!.GetValue(null)!);
    Assert.Equal((IntPtr)0, (IntPtr)v);
    var x = (byte*)Pointer.Unbox(t.GetField(nameof(SomeBinaryLiterals.BinNonLiteral))!.GetValue(null)!);
    Assert.Equal((IntPtr)0, (IntPtr)x);
  }

}