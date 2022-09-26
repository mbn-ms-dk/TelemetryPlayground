using OpenTelemetry;
using System.Diagnostics;

namespace TelemetryAppInsights
{
    public  class ActivityFilteringProcessor: BaseProcessor<Activity>
    {
        public override void OnEnd(Activity data)
        {
            //prevent all exporters from exporting internal activities
            if (data.Kind == ActivityKind.Internal)
                data.IsAllDataRequested = false;
        }
    }
}
