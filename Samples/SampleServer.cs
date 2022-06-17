using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;

namespace TelemetryAppInsights.Samples
{
    public class SampleServer : IDisposable
    {
        private readonly HttpListener listener = new();

        public void Start(string url)
        {
            this.listener.Prefixes.Add(url);
            this.listener.Start();
            Task.Run(() =>
            {
                using var source = new ActivitySource("Simcorp.Samples.SampleServer");
                while(this.listener.IsListening)
                {
                    try
                    {
                        var context = this.listener.GetContext();

                        using var activity = source.StartActivity(
                            $"{context.Request.HttpMethod}:{context.Request.Url?.AbsolutePath}",
                            ActivityKind.Server);

                        var headerKeys = context.Request.Headers.AllKeys;
                        foreach(var headerKey in headerKeys)
                        {
                            var headerValue = context.Request.Headers[headerKey];
                            activity?.SetTag($"http.header.{headerKey}", headerValue);
                        }

                        activity?.SetTag("http.url", context.Request.Url?.ToString());
                        activity?.SetTag("http.host", context.Request.Url?.Host);

                        string requestContent;
                        using(var childSpan = source.StartActivity("ReadStream", ActivityKind.Consumer))
                        using(var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                        {
                            requestContent = reader.ReadToEnd();
                            childSpan?.AddEvent(new ActivityEvent("Streamreader.ReadToEnd"));
                        }

                        activity?.SetTag("request.content", requestContent);
                        activity?.SetTag("request.length", requestContent.Length.ToString(CultureInfo.InvariantCulture));
                        activity?.SetTag("http.status_code", $"{context.Response.StatusCode:D}");

                        var echo = Encoding.UTF8.GetBytes("echo: " + requestContent);
                        context.Response.ContentEncoding = Encoding.UTF8;
                        context.Response.ContentLength64 = echo.Length;
                        context.Response.OutputStream.Write(echo, 0, echo.Length);
                        context.Response.Close();
                    }
                    catch (Exception ex) 
                    {
                        Console.WriteLine("OOOPS");
                        Console.WriteLine(ex.Message);
                    }
                }
            });
        }
        public void Dispose()
        {
            this.listener.Close();
        }
    }
}
