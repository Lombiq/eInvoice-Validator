using EInvoiceValidator.Benchmark.Models;
using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Helpers;
using Lombiq.EInvoiceValidator.Models;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text;

namespace EInvoiceValidator.Benchmark.Helpers;

public static class ValidationBenchmarkHelpers
{
    public static async Task<InvoiceValidationResult> ValidateAsync(
        string filePath,
        IServiceProvider serviceProvider)
    {
        var nodeJsService = serviceProvider.GetRequiredService<INodeJSService>();
        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        var eInvoiceXmlSchemaSet = serviceProvider.GetRequiredService<IEInvoiceXmlSchemaSet>();

        using var streamReaderInner = new StreamReader(filePath);

        // Call validation.
        var result = await InvoiceValidationHelper.ValidateInvoiceAsync(
            streamReaderInner.BaseStream,
            nodeJsService,
            memoryCache,
            eInvoiceXmlSchemaSet);

        if (result.SchematronValidationResult!.ErrorFailedAsserts.Count > 0)
        {
            Console.WriteLine($"Errors in {filePath}:");
        }

        return result;
    }

    public static async Task<string> SaveToFileAsync(
        IList<InvoiceValidationResult> results,
        int batchSize,
        int batchCount,
        int minDelayBetweenBatchesMs)
    {
        var averageDurations = AverageDurations(results);
        var logBuilder = new StringBuilder();

        logBuilder.AppendLine("# Benchmark Results");
        logBuilder.AppendLine();
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Run Timestamp:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Batch Size:** {batchSize}");
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Batch Count:** {batchCount}");
        logBuilder.AppendLine(CultureInfo.InvariantCulture, $"- **Minimum Delay Between Batches:** {minDelayBetweenBatchesMs} ms");
        logBuilder.AppendLine();
        logBuilder.AppendLine(MarkdownTableHeader());

        for (int i = 0; i < batchCount; i++)
        {
            var batch = results.Skip(i * batchSize).Take(batchSize).ToList();
            var batchAverages = AverageDurations(batch);
            logBuilder.AppendLine(FormatMarkdownRow(i, batchAverages));
        }

        // Final overall summary row
        logBuilder.AppendLine(FormatSummaryRow(averageDurations));
        logBuilder.AppendLine();

        var outputPath = Path.Combine("BenchmarkResults", "test.md");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        if (File.Exists(outputPath))
        {
            var i = 1;
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
        "| Batch | Schema Inner (ms) | Schema Full (ms) | Schematron Inner (ms) | Schematron Full (ms) | Total (ms) |\n" +
        "|-------|-------------------|------------------|------------------------|-----------------------|------------|";

    private static string FormatMarkdownRow(int batchIndex, AverageDurations durations) =>
        $"| {(batchIndex + 1).ToTechnicalString()} | {durations.SchemaInnerMs} | {durations.SchemaTotalMs} " +
        $"| {durations.SchematronInnerMs} | {durations.SchematronTotalMs} | {durations.TotalMs} |";

    private static string FormatSummaryRow(AverageDurations durations) =>
        $"| **AVG** | **{durations.SchemaInnerMs}** | **{durations.SchemaTotalMs}** | **{durations.SchematronInnerMs}** " +
        $"| **{durations.SchematronTotalMs}** | **{durations.TotalMs}** |";

    private static AverageDurations AverageDurations(IList<InvoiceValidationResult> results) =>
        new()
        {
            SchemaInnerMs = ToAverageString(results.Select(item => item.SchemaValidationResult!.InnerValidationDurationMs)),
            SchemaTotalMs = ToAverageString(results.Select(item => item.SchemaValidationResult!.ValidationDurationMs)),
            SchematronInnerMs = ToAverageString(results.Select(item => (long)item.SchematronValidationResult!.InnerValidationDurationMs)),
            SchematronTotalMs = ToAverageString(results.Select(item => item.SchematronValidationResult!.ValidationDurationMs)),
            TotalMs = ToAverageString(results.Select(item => item.TotalValidationDurationMs)),
        };

    private static string ToAverageString(IEnumerable<long> numbers) =>
        Math.Round(numbers.Average(), 3).ToTechnicalString()!;
}
