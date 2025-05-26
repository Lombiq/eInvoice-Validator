using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Helpers;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

// Step 0. We use DI to manage the services required for e-invoice validation.
var services = new ServiceCollection();

// Step 1. Add the necessary services for e-invoice validation.
services.AddEInvoiceValidationServices();

var serviceProvider = services.BuildServiceProvider();

// Step 2. Get the required services from the service provider.
var nodeJsService = serviceProvider.GetRequiredService<INodeJSService>();
var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
var eInvoiceXmlSchemaSet = serviceProvider.GetRequiredService<IEInvoiceXmlSchemaSet>();

// Step 3. Get the invoices in the SampleInvoices directory.
var filePaths = Directory.GetFiles(Path.Combine("SampleInvoices"), "*.xml", SearchOption.AllDirectories);

foreach (var filePath in filePaths)
{
    using var streamReaderInner = new StreamReader(filePath);

    // Step 4. Validate each invoice using the InvoiceValidationHelper.
    var result = await InvoiceValidationHelper.ValidateInvoiceAsync(streamReaderInner.BaseStream, nodeJsService, memoryCache, eInvoiceXmlSchemaSet);

    // Step 5. Check the validation result and print the appropriate messages.
    if (result.Successful)
    {
        Console.WriteLine($"The invoice in {filePath} is valid.");
        continue;
    }

    // The files with failing in their name are expected to be invalid.
    Console.WriteLine($"The invoice in {filePath} is invalid.");
    LogValidationErrors(result.SchematronValidationResult?.ErrorFailedAsserts);
    LogValidationErrors(result.SchematronValidationResult?.WarningFailedAsserts);
    LogValidationErrors(result.SchemaValidationResult?.ErrorMessages);
}

static void LogValidationErrors<T>(IList<T> failedAsserts)
    where T : class
{
    if (failedAsserts != null)
    {
        foreach (var failedAssert in failedAsserts)
        {
            Console.WriteLine(failedAssert);
        }
    }
}
