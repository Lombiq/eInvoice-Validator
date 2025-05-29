using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Benchmark.Models;
using Lombiq.EInvoiceValidator.Models;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Benchmark.Helpers;

public static class ValidationBenchmarkHelpers
{
    // Set the benchmark parameters.
    private const int BatchSize = 200;
    private const int BatchCount = 10;
    private const int MinDelayBetweenBatchesMs = 1000;
    private const bool DoWarmup = true;

    public static async Task RunBenchMarkAsync(
        IServiceProvider serviceProvider,
        Func<Stream, INodeJSService, IMemoryCache, IEInvoiceXmlSchemaSet, Task<InvoiceValidationResult>> action)
    {
        // Warm-up run before benchmark.
        if (DoWarmup)
        {
            Console.WriteLine($"Warming up by running {BatchSize} validations...");
            await RunValidationForBatchAsync(serviceProvider, action, 1);
        }

        var results = new List<BenchmarkRunResult>();
        for (int batchIndex = 0; batchIndex < BatchCount; batchIndex++)
        {
            Console.WriteLine($"Starting batch {(batchIndex + 1).ToTechnicalString()} of {BatchCount}...");
            var stopwatch = Stopwatch.StartNew();

            var batchResults = await RunValidationForBatchAsync(serviceProvider, action, batchIndex);

            stopwatch.Stop();

            results.AddRange(batchResults);

            var remainingDelay = MinDelayBetweenBatchesMs - stopwatch.ElapsedMilliseconds;
            if (remainingDelay > 0)
            {
                Console.WriteLine($"Waiting {remainingDelay.ToTechnicalString()} ms before next batch.");
                await Task.Delay((int)remainingDelay);
            }
        }

        // Get average durations and save to file.
        var formattedLog = await SaveToFileAsync(results);
        Console.WriteLine(formattedLog);
    }

    private static async Task<BenchmarkRunResult[]> RunValidationForBatchAsync(
        IServiceProvider serviceProvider,
        Func<Stream, INodeJSService, IMemoryCache, IEInvoiceXmlSchemaSet, Task<InvoiceValidationResult>> action,
        int batchIndex)
    {
        var filePaths = Directory.GetFiles(
            Path.Combine("..", "Lombiq.EInvoiceValidator.Sample", "SampleInvoices"),
            "*.xml",
            SearchOption.AllDirectories);
        var filesInBatch = Enumerable
            .Range(0, BatchSize)
            .Select(i => filePaths[((batchIndex * BatchSize) + i) % filePaths.Length])
            .ToList();

        // Run the validations in parallel.
        var batchResults = await Task.WhenAll(filesInBatch.Select(filePath => ValidateAsync(filePath, serviceProvider, action)));
        return batchResults;
    }

    private static async Task<BenchmarkRunResult> ValidateAsync(
        string filePath,
        IServiceProvider serviceProvider,
        Func<Stream, INodeJSService, IMemoryCache, IEInvoiceXmlSchemaSet, Task<InvoiceValidationResult>> action)
    {
        var nodeJsService = serviceProvider.GetRequiredService<INodeJSService>();
        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        var eInvoiceXmlSchemaSet = serviceProvider.GetRequiredService<IEInvoiceXmlSchemaSet>();

        using var streamReaderInner = new StreamReader(filePath);

        var stopwatch = Stopwatch.StartNew();
        // Call validation.
        var result = await action(streamReaderInner.BaseStream, nodeJsService, memoryCache, eInvoiceXmlSchemaSet);
        stopwatch.Stop();

        return new BenchmarkRunResult { Result = result, ElapsedMilliseconds = stopwatch.ElapsedMilliseconds };
    }

    private static async Task<string> SaveToFileAsync(IList<BenchmarkRunResult> results)
    {
        var averageDurations = AverageDurations(results);
        var logBuilder = new StringBuilder();

        logBuilder.AppendLine("# Benchmark Results");
        logBuilder.AppendLine();
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Run Timestamp:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Batch Size:** {BatchSize}");
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Batch Count:** {BatchCount}");
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Minimum Delay Between Batches:** {MinDelayBetweenBatchesMs} ms");
        logBuilder.AppendLine();
        logBuilder.AppendLine(MarkdownTableHeader());

        for (int i = 0; i < BatchCount; i++)
        {
            var batch = results.Skip(i * BatchSize).Take(BatchSize).ToList();
            var batchAverages = AverageDurations(batch);
            logBuilder.AppendLine(FormatMarkdownRow(i, batchAverages));
        }

        // Final overall summary row
        logBuilder.AppendLine(FormatSummaryRow(averageDurations));
        logBuilder.AppendLine();

        // Create the BenchmarkResults directory if it doesn't exist.
        if (!Directory.Exists("BenchmarkResults"))
        {
            Directory.CreateDirectory("BenchmarkResults");
        }

        var outputPath = Path.Combine("BenchmarkResults", "test_1.md");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        if (File.Exists(outputPath))
        {
            var i = 2;
            string newPath;
            do
            {
                newPath = Path.Combine("BenchmarkResults", $"test_{i.ToTechnicalString()}.md");
                i++;
            }
            while (File.Exists(newPath));

            outputPath = newPath;
        }

        var formattedString = logBuilder.ToString();
        await File.WriteAllTextAsync(outputPath, formattedString);

        return formattedString;
    }

    private static string MarkdownTableHeader() =>
        "| Batch | Schematron Inner (ms) | Total (ms) |\n" +
        "|-------|-----------------------|------------|";

    private static string FormatMarkdownRow(int batchIndex, AverageDurations durations) =>
        $"| {(batchIndex + 1).ToTechnicalString()} | {durations.SchematronInnerMs} | {durations.TotalMs} |";

    private static string FormatSummaryRow(AverageDurations durations) =>
        $"| **AVG** | **{durations.SchematronInnerMs}** | **{durations.TotalMs}** |";

    private static AverageDurations AverageDurations(IList<BenchmarkRunResult> results) =>
        new()
        {
            SchematronInnerMs = ToAverageString(results.Select(item => (long)item.Result.SchematronValidationResult!.InnerValidationDurationMs)),
            TotalMs = ToAverageString(results.Select(item => item.ElapsedMilliseconds)),
        };

    private static string ToAverageString(IEnumerable<long> numbers) =>
        Math.Round(numbers.Average(), 3).ToTechnicalString()!;
}
