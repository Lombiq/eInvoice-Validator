using Lombiq.EInvoiceValidator.Benchmark.Helpers;
using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.DependencyInjection;

// Add the services to the DI container.
var services = new ServiceCollection();

// Step 1. Add the necessary services for eInvoice validation.
services.AddEInvoiceValidationServices();

// Build the service provider.
var serviceProvider = services.BuildServiceProvider();
var invoiceValidationService = serviceProvider.GetRequiredService<IInvoiceValidationService>();

// Start the benchmark.
// Step 2. Validate the eInvoice XML file read into a stream using the helper method.
await ValidationBenchmarkHelpers.RunBenchMarkAsync(async stream =>
    await invoiceValidationService.ValidateInvoiceAsync(stream));
