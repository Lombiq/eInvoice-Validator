using EInvoiceValidator.Benchmark.Helpers;
using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

// Set the benchmark parameters.
const int batchSize = 200;
const int batchCount = 10;
const int minDelayBetweenBatchesMs = 1000;
const bool doWarmup = true;
var filePaths = Directory.GetFiles(Path.Combine("SampleInvoices"), "*.xml", SearchOption.AllDirectories);

// Add the services to the DI container.
var services = new ServiceCollection();
services.AddEInvoiceValidationServices();
services.AddMemoryCache();

// Build the service provider.
var serviceProvider = services.BuildServiceProvider();

// Warmup run for benchmark.
if (doWarmup)
{
    Console.WriteLine("Warming up by running a few validations...");
    await Task.WhenAll(Enumerable
        .Range(0, batchSize)
        .Select(i => filePaths[(batchSize + i) % filePaths.Length])
        .Select(filePath => ValidationBenchmarkHelpers.ValidateAsync(filePath, serviceProvider)));
}

// Start the benchmark.
var results = new List<InvoiceValidationResult>();
for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
{
    Console.WriteLine($"Starting batch {(batchIndex + 1).ToTechnicalString()} of {batchCount}...");
    var stopwatch = Stopwatch.StartNew();

    var filesInBatch = Enumerable
        .Range(0, batchSize)
        .Select(i => filePaths[((batchIndex * batchSize) + i) % filePaths.Length])
        .ToList();

    // Run the validations in parallel.
    var batchTasks = filesInBatch.Select(filePath => ValidationBenchmarkHelpers.ValidateAsync(filePath, serviceProvider));
    var batchResults = await Task.WhenAll(batchTasks);

    stopwatch.Stop();

    if (batchResults.Any(item =>
            item.SchematronValidationResult?.ErrorFailedAsserts.Count > 0 ||
            item.SchemaValidationResult?.ErrorMessages.Count > 0))
    {
        foreach (var (result, filePath) in batchResults.Zip(filesInBatch))
        {
            if (!result.Successful)
            {
                Console.WriteLine($"Errors in {filePath}:");
            }

            if (result.HasWarnings)
            {
                Console.WriteLine($"Warnings in {filePath}:");
            }
        }
    }

    results.AddRange(batchResults);

    var remainingDelay = minDelayBetweenBatchesMs - stopwatch.ElapsedMilliseconds;
    if (remainingDelay > 0)
    {
        Console.WriteLine($"Waiting {remainingDelay.ToTechnicalString()} ms before next batch.");
        await Task.Delay((int)remainingDelay);
    }
}

// Get average durations and save to file.
var formattedLog = ValidationBenchmarkHelpers.SaveToFileAsync(results, batchSize, batchCount, minDelayBetweenBatchesMs);
Console.WriteLine(formattedLog);
