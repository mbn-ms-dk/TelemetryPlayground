﻿using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using TelemetryAppInsights.Samples;

namespace TelemetryAppInsights
{
    class Program
    {
        private static readonly ActivitySource source = new("Telemetry.AzureMonitor.Demo");
        static async Task Main(string[] args)
        {
            var resourceAttributes = new Dictionary<string, object> { 
                { "service.name", "TelemetryDemo" }, 
                { "service.instance.id", "AdvancedService" }, 
                { "service.namespace", "Telemetry.TelemetryAppInsights" } 
            };

            var resourceBuilder = ResourceBuilder.CreateDefault().AddAttributes(resourceAttributes);


            using var tracerProvider = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder)
                .AddSource("Telemetry.AzureMonitor.Demo")
                .AddSource("IdentityTelemetry")
                .AddSource("Telemetry.Samples.SampleServer")
                .AddSource("Telemetry.Samples.SampleClient")
                .AddSource("CustomTestActivity")
                .AddProcessor(new ActivityEnrichingProcessor())
                .AddProcessor(new ActivityFilteringProcessor())
                //.AddConsoleExporter()
                .AddAzureMonitorTraceExporter(o =>
                {
                    o.ConnectionString = Settings();
                })
                .AddZipkinExporter(z =>
                {
                    z.Endpoint = new Uri("http://localhost:9411/api/v2/spans");
                })
                .AddJaegerExporter(j =>
                {
                    j.AgentHost = "localhost";
                    j.AgentPort = 6831;

                    j.MaxPayloadSizeInBytes = 4096;
                    j.ExportProcessorType = OpenTelemetry.ExportProcessorType.Batch;
                    j.BatchExportProcessorOptions = new OpenTelemetry.BatchExportProcessorOptions<Activity>()
                    {
                        MaxQueueSize = 2048,
                        ScheduledDelayMilliseconds = 5000,
                        ExporterTimeoutMilliseconds = 30000,
                        MaxExportBatchSize = 512
                    };
                })
                .Build();
           
            using (var activity = source.StartActivity("CustomTestActivity"))
            {
                activity?.SetTag("CustomTag1", $"Value1");
                activity?.SetTag("CustomTag2", $"Value2");
                using (var childSpan = source.StartActivity("ChildActivity", ActivityKind.Producer))
                {
                    childSpan?.AddEvent(new ActivityEvent("Add Baggage:Started"));
                    childSpan?.AddBaggage("Key", "Value");
                    childSpan?.AddEvent(new ActivityEvent("Add Baggage:Ended"));
                }
            }


            using var sample = new Orchestrator();
            sample.Start();

            Console.WriteLine("Press Enter key to exit.");
            Console.ReadLine();
        }

        private static string Settings()
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"application.json");

            var config = configuration.Build();
            var connectionString = config["AppInsights"];
            return connectionString;
        }
    }
}