using System;
using System.IO;
using System.Reflection;
using System.Text;
using Embedded;
using Xunit;
using Xunit.Abstractions;

[Collection("WeaverTests")]
public unsafe class Utf8StringWeaverTests : WeaverTests {

  public Utf8StringWeaverTests(ITestOutputHelper outputHelper, TestResultFixture fixture)
    : base(outputHelper, fixture) {
  }

  [Fact]
  public void ValidateUtf8StringsResource() {
    var ums = (UnmanagedMemoryStream) TestResult.Assembly.GetManifestResourceStream("UTF8_STRINGS")!;
    Assert.NotNull(ums);
    var bufSize = ums.Length;
    Assert.NotEqual(0, bufSize);
    var buffer = new byte[bufSize];
    Assert.Equal(bufSize, ums.Read(buffer, 0, checked((int) bufSize)));
    var s = Encoding.UTF8.GetString(buffer);
    Assert.Equal("Utf8Literal1 String\0\0\0\0\0\0\0\0\0\0\0\0\0Utf8Literal2 String\0\0\0\0\0\0\0\0\0\0\0\0\0", s);
  }

  [Fact]
  public void ValidateUtf8Literal1() {
    var t = TestResult.Assembly.GetType(nameof(SomeUtf8StringLiterals))!;
    var v = (byte*) Pointer.Unbox(t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal1))!.GetValue(null)!);
    Assert.NotEqual((IntPtr) 0, (IntPtr) v);
    var s = Encoding.UTF8.GetString(v, 32);
    Assert.Equal("Utf8Literal1 String\0\0\0\0\0\0\0\0\0\0\0\0\0", s);
    // verify same pointer other field same value
    var d = (byte*) Pointer.Unbox(t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal1Dupe))!.GetValue(null)!);
    Assert.Equal((IntPtr) v, (IntPtr) d);
    var g = ((IntPtr, int)) t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal1Tuple))!.GetValue(null)!;
    Assert.Equal((IntPtr) v, g.Item1);
    Assert.Equal(19, g.Item2);
  }

  [Fact]
  public void ValidateUtf8Literal2() {
    var t = TestResult.Assembly.GetType(nameof(SomeUtf8StringLiterals))!;
    var v = (byte*) Pointer.Unbox(t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal2))!.GetValue(null)!);
    Assert.NotEqual((IntPtr) 0, (IntPtr) v);
    var s = Encoding.UTF8.GetString(v, 32);
    Assert.Equal("Utf8Literal2 String\0\0\0\0\0\0\0\0\0\0\0\0\0", s);
    // verify same pointer other field same value
    var d = (byte*) Pointer.Unbox(t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal2Dupe))!.GetValue(null)!);
    Assert.Equal((IntPtr) v, (IntPtr) d);
    var g = (ReadOnlyData<byte>) t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal2ReadOnlyData))!.GetValue(null)!;
    Assert.Equal((IntPtr) v, g.Pointer);
    Assert.Equal(19, g.Length);
  }

  [Fact]
  public void ValidateUtf8Literal3() {
    var t = TestResult.Assembly.GetType(nameof(SomeUtf8StringLiterals))!;
    var v = (byte*) Pointer.Unbox(t.GetField(nameof(SomeUtf8StringLiterals.Utf8Literal3))!.GetValue(null)!);
    Assert.Equal((IntPtr) 0, (IntPtr) v);
    var x = (byte*) Pointer.Unbox(t.GetField(nameof(SomeUtf8StringLiterals.NonUtf8Literal))!.GetValue(null)!);
    Assert.Equal((IntPtr) 0, (IntPtr) x);
  }

}