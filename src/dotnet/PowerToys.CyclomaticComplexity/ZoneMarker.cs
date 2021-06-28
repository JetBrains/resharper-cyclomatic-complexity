using JetBrains.Application.BuildScript.Application.Zones;
using JetBrains.ReSharper.Resources.Shell;
#if RIDER
using JetBrains.Rider.Backend.Env;
#endif

namespace JetBrains.ReSharper.Plugins.CyclomaticComplexity
{
    [ZoneMarker]
    public class ZoneMarker :
#if RIDER
        IRequire<IRiderPlatformZone>,
#endif
        IRequire<PsiFeaturesImplZone>
    {
    }
}