using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.TestUtilities.Fixtures;
public class PerformanceTestFixture : IDisposable
{
    public System.Diagnostics.Stopwatch Stopwatch { get; private set; }
    public List<TimeSpan> Measurements { get; private set; }
    public ServiceFixture ServiceFixture { get; private set; }

    public PerformanceTestFixture()
    {
        Stopwatch = new System.Diagnostics.Stopwatch();
        Measurements = new List<TimeSpan>();
        ServiceFixture = new ServiceFixture();
    }

    public void StartMeasurement()
    {
        Stopwatch.Restart();
    }

    public void StopMeasurement()
    {
        Stopwatch.Stop();
        Measurements.Add(Stopwatch.Elapsed);
    }

    public TimeSpan GetAverageTime()
    {
        if (Measurements.Count == 0) return TimeSpan.Zero;
        var totalTicks = Measurements.Sum(m => m.Ticks);
        return new TimeSpan(totalTicks / Measurements.Count);
    }

    public TimeSpan GetMaxTime()
    {
        return Measurements.Count == 0 ? TimeSpan.Zero : Measurements.Max();
    }

    public TimeSpan GetMinTime()
    {
        return Measurements.Count == 0 ? TimeSpan.Zero : Measurements.Min();
    }

    public void AssertPerformance(TimeSpan maxExpectedTime, string operation = "Operation")
    {
        var avgTime = GetAverageTime();
        var maxTime = GetMaxTime();

        avgTime.Should().BeLessThan(maxExpectedTime,
            $"{operation} average time should be less than {maxExpectedTime.TotalMilliseconds}ms, but was {avgTime.TotalMilliseconds}ms");

        maxTime.Should().BeLessThan(maxExpectedTime.Add(TimeSpan.FromMilliseconds(100)),
            $"{operation} max time should be reasonable");
    }

    public void Dispose()
    {
        ServiceFixture?.Dispose();
        GC.SuppressFinalize(this);
    }
}

