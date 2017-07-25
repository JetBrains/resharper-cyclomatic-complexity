using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TestFramework;
using JetBrains.TestFramework;
using JetBrains.TestFramework.Application.Zones;
using NUnit.Framework;

[assembly: RequiresSTA]

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity.Tests
{
  [ZoneDefinition]
  public interface ICyclomaticComplexityTestZone : ITestsZone, IRequire<PsiFeatureTestZone>, IRequire<ILanguageCppZone>
  {
  }

  [SetUpFixture]
  public class TestEnvironment : ExtensionTestEnvironmentAssembly<ICyclomaticComplexityTestZone>
  {
  }
}
