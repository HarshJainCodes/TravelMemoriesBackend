using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace TravelMemories.Configuration.AppInsights
{
    public class AppInsightsConfiguration : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = "TravelMemoriesBackend";
        }
    }
}
