using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Resources.Shell;

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
  [ZoneMarker]
  public class ZoneMarker : IRequire<PsiFeaturesImplZone>
  {
  }
}