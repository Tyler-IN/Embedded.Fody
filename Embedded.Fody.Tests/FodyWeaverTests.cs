using Xunit;
using Xunit.Abstractions;

[Collection("WeaverTests")]
public class FodyWeaverTests : WeaverTests {

  public FodyWeaverTests(ITestOutputHelper outputHelper, TestResultFixture fixture)
    : base(outputHelper, fixture) {
  }

  [Fact]
  public void ValidateProcessedByFody() {
    var t = TestResult.Assembly.GetType("ProcessedByFody");
  }

}