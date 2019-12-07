using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Unicode;
using Embedded;
using Fody;
using Xunit;
using Xunit.Abstractions;

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