using System.Globalization;

namespace TelemetryAppInsights.Samples
{
    public  class Orchestrator : IDisposable
    {
        private const string RequestPath = "/api/request";
        private readonly SampleServer server = new();
        private readonly SampleClient client = new();

        public void Dispose()
        {
            try
            {
                this.client.Dispose();
                this.server.Dispose();
            }
            catch (Exception) { }
        }

        public void Start(ushort port = 19999)
        {
            var url = $"http://localhost:{port.ToString(CultureInfo.InvariantCulture)}{RequestPath}/";
            this.server.Start(url);
            this.client.Start(url);
        }
    }
}
