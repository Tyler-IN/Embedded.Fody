using Fody;
using JetBrains.Annotations;
using Xunit;
using Xunit.Abstractions;

[PublicAPI]
public abstract class WeaverTests : IClassFixture<TestResultFixture> {

  protected readonly ITestOutputHelper OutputHelper;

  protected ModuleWeaver ModuleWeaver => TestResultFixture.ModuleWeaver;

  protected TestResult TestResult => TestResultFixture.TestResult;

  protected readonly TestResultFixture TestResultFixture;

  protected WeaverTests(ITestOutputHelper outputHelper, TestResultFixture fixture) {
    OutputHelper = outputHelper;
    TestResultFixture = fixture;
  }

}