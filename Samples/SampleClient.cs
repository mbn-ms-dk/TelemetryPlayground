﻿using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace TelemetryAppInsights.Samples
{
    public class SampleClient : IDisposable
    {
        private CancellationTokenSource? cts;
        private Task? requestTask;
        private const string RequestPath = "/api/request";

        public void Start(string url)
        {
            this.cts = new CancellationTokenSource();
            var cancellationToken = this.cts.Token;

            this.requestTask = Task.Run(async () =>
            {
                using var source = new ActivitySource("Telemetry.Samples.SampleClient");
                using var client = new HttpClient();

                var count = 1;
                while(!cancellationToken.IsCancellationRequested)
                {
                    using var cs = source.StartActivity("SetContent", ActivityKind.Client);
                    cs?.AddEvent(new ActivityEvent("ContentWrite"));
                    var content = new StringContent($"client message: {DateTime.Now}", Encoding.UTF8);
                    cs?.AddEvent(new ActivityEvent("ContentWriteDone"));


                    using var activity = source.StartActivity("POST: " + RequestPath, ActivityKind.Client);
                    count++;

                    using var childSpan = source.StartActivity("PostAsync", ActivityKind.Client);
                    childSpan?.AddEvent(new ActivityEvent("PostAsync:Started"));
                    using var response = await client.PostAsync(url, content, cancellationToken);
                    childSpan?.AddEvent(new ActivityEvent("PostAsync:Ended"));

                    activity?.SetTag("http.url", url);
                    activity?.SetTag("http.status_code", $"{response.StatusCode:D}");

                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    activity?.SetTag("response.content", responseContent);
                    activity?.SetTag("response.length", responseContent.Length.ToString(CultureInfo.InvariantCulture));

                    foreach (var header in response.Headers)
                    {
                        if (header.Value is IEnumerable<object> enumerable)
                            activity?.SetTag($"http.header.{header.Key}", string.Join(",", enumerable));
                        else
                            activity?.SetTag($"http.header.{header.Key}", header.Value.ToString());
                    }
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
                catch(TaskCanceledException)
                {
                    return;
                }
            }, cancellationToken);
        }
        public void Dispose()
        {
            if(this.cts != null)
            {
                this.cts.Cancel();
                this.requestTask?.Wait();
                this.requestTask?.Dispose();
                this.cts.Dispose();
            }
        }
    }
}
