using OpenTelemetry;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TelemetryAppInsights
{
    public  class ActivityEnrichingProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            // The updated activity will be available to all processors which are called after this processor.
            activity.DisplayName = "UpdatedWithActivityProcessor->" + activity.DisplayName;
            activity.SetTag("CustomDimension1", "Value1");
            activity.SetTag("CustomDimension2", "Value2");
            if (activity.Kind == ActivityKind.Server)
                activity.SetTag("http.client.ip", GetIPAddress());
        }

        private static string? GetIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach(var ip in host.AddressList)
            {
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }

            return "Unknown";
        }
    }
}
