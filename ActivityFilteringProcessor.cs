using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
