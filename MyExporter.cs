// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Text;
using OpenTelemetry;
using OpenTelemetry.Metrics;

internal class MyExporter : BaseExporter<Metric>
{
    private readonly string name;

    public MyExporter(string name = "MyExporter")
    {
        this.name = name;
    }

    public override ExportResult Export(in Batch<Metric> batch)
    {
        // SuppressInstrumentationScope should be used to prevent exporter
        // code from generating telemetry and causing live-loop.
        using var scope = SuppressInstrumentationScope.Begin();

        var sb = new StringBuilder();
        sb.AppendLine("Exporting batch");
        sb.AppendLine("===============");

        foreach (var metric in batch)
        {
            sb.AppendLine($"Metric = {metric.Name}");

            foreach (ref readonly var metricPoint in metric.GetMetricPoints())
            {
                sb.AppendLine($"Start time = {metricPoint.StartTime}");
                foreach (var metricPointTag in metricPoint.Tags)
                {
                    sb.AppendLine($"    Tag: {metricPointTag.Key} = {metricPointTag.Value}");
                }
                if (metricPoint.TryGetExemplars(out var exemplars))
                {
                    foreach (var exemplar in exemplars)
                    {
                        sb.AppendLine($"    Exemplar: SpanId = {exemplar.SpanId}");
                    }
                }
            }
        }

        Console.WriteLine(sb.ToString());
        return ExportResult.Success;
    }

    protected override bool OnShutdown(int timeoutMilliseconds)
    {
        Console.WriteLine($"{this.name}.OnShutdown(timeoutMilliseconds={timeoutMilliseconds})");
        return true;
    }

    protected override void Dispose(bool disposing)
    {
        Console.WriteLine($"{this.name}.Dispose({disposing})");
    }
}
