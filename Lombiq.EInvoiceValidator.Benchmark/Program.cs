using Lombiq.EInvoiceValidator.Benchmark.Helpers;
using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Helpers;
using Microsoft.Extensions.DependencyInjection;

// Add the services to the DI container.
var services = new ServiceCollection();

// Step 1. Add the necessary services for eInvoice validation.
services.AddEInvoiceValidationServices();

// Build the service provider.
var serviceProvider = services.BuildServiceProvider();

// Start the benchmark.
await ValidationBenchmarkHelpers.RunBenchMarkAsync(
    serviceProvider,
    async (stream, nodeJsService, memoryCache, eInvoiceXmlSchemaSet) =>
        // Step 2. Validate the eInvoice XML file read into a stream using the helper method.
        await InvoiceValidationHelper.ValidateInvoiceAsync(stream, nodeJsService, memoryCache, eInvoiceXmlSchemaSet));
