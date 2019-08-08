using System.Threading;
using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: Apartment(ApartmentState.STA)]

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [ZoneDefinition]
  public interface ICyclomaticComplexityTestZone : ITestsEnvZone, IRequire<PsiFeatureTestZone>
  {
  }

  [SetUpFixture]
  public class TestEnvironment : ExtensionTestEnvironmentAssembly<ICyclomaticComplexityTestZone>
  {
  }
}
