using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Helpers;

public static class TimingHelpers
{
    public static async Task<long> MeasureTimeAsync(Func<Task> action)
    {
        var stopwatch = Stopwatch.StartNew();
        await action();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }

    public static async Task<(TResult Result, long ElapsedMs)> MeasureTimeAsync<TResult>(Func<Task<TResult>> action)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();
        return (result, stopwatch.ElapsedMilliseconds);
    }
}
