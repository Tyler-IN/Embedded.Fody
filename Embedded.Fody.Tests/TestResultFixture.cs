using Fody;
using JetBrains.Annotations;

[PublicAPI]
public class TestResultFixture {

  public TestResult TestResult;

  public readonly ModuleWeaver ModuleWeaver = new ModuleWeaver();

  public TestResultFixture()
    => TestResult = ModuleWeaver.ExecuteTestRun("AssemblyToProcess.dll", runPeVerify: false);

}