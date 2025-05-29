using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

// Step 0. We use DI to manage the services required for eInvoice validation.
var services = new ServiceCollection();

// Step 1. Add the necessary services for eInvoice validation.
services.AddEInvoiceValidationServices();

var serviceProvider = services.BuildServiceProvider();

// Step 2. Get the required service from the service provider.
var invoiceValidationService = serviceProvider.GetRequiredService<IInvoiceValidationService>();

// Step 3. Get the example EN 16931 CII and EN 16931 UBL invoices in the SampleInvoices directory.
var filePaths = Directory.GetFiles(Path.Combine("SampleInvoices"), "*.xml", SearchOption.AllDirectories);

foreach (var filePath in filePaths)
{
    using var streamReaderInner = new StreamReader(filePath);

    // Step 4. Validate each invoice using the InvoiceValidationHelper.
    var result = await invoiceValidationService.ValidateInvoiceAsync(streamReaderInner.BaseStream);

    // Step 5. Check the validation result and print the appropriate messages.
    if (result.Successful)
    {
        Console.WriteLine($"The invoice in {filePath} is valid.");
    }
    else
    {
        // The files with failing in their name are expected to be invalid in the example.
        Console.WriteLine($"The invoice in {filePath} is invalid.");
        LogValidationErrors(result.SchematronValidationResult?.ErrorFailedAsserts);
        LogValidationErrors(result.SchematronValidationResult?.WarningFailedAsserts);
        LogValidationErrors(result.SchemaValidationResult?.ErrorMessages);
    }
}

static void LogValidationErrors<T>(IList<T> failedAsserts)
    where T : class
{
    if (failedAsserts == null) return;

    foreach (var failedAssert in failedAsserts)
    {
        Console.WriteLine(failedAssert);
    }
}
