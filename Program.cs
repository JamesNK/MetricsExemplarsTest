// Copyright The OpenTelemetry Authors
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Threading.Tasks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ExtendingTheSdk;

public class Program
{
    private static readonly Meter MyMeter = new("MyCompany.MyProduct.MyLibrary", "1.0");
    private static readonly Counter<long> MyFruitCounter = MyMeter.CreateCounter<long>("MyFruitCounter");

    static Program()
    {
        var process = Process.GetCurrentProcess();

        MyMeter.CreateObservableGauge(
            "MyProcessWorkingSetGauge",
            () => new List<Measurement<long>>()
            {
                new(process.WorkingSet64, new("process.id", process.Id), new("process.bitness", IntPtr.Size << 3)),
            });
    }

    public static async Task Main()
    {
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter("MyCompany.MyProduct.MyLibrary")
            .AddReader(new BaseExportingMetricReader(new MyExporter("ExporterX")))
            .AddHttpClientInstrumentation()
            .AddMyExporter()
            //.AddConsoleExporter()
            .SetExemplarFilter(ExemplarFilterType.TraceBased)
            .Build();

        using var traceProvider = Sdk.CreateTracerProviderBuilder()
            .AddHttpClientInstrumentation()
            //.AddConsoleExporter()
            .SetSampler<AlwaysOnSampler>()
            .Build();

        MyFruitCounter.Add(1, new("name", "apple"), new("color", "red"));
        MyFruitCounter.Add(2, new("name", "lemon"), new("color", "yellow"));
        MyFruitCounter.Add(1, new("name", "lemon"), new("color", "yellow"));
        MyFruitCounter.Add(2, new("name", "apple"), new("color", "green"));
        MyFruitCounter.Add(5, new("name", "apple"), new("color", "red"));
        MyFruitCounter.Add(4, new("name", "lemon"), new("color", "yellow"));

        var httpClient = new HttpClient();
        await httpClient.GetAsync("https://www.google.com");
    }
}
