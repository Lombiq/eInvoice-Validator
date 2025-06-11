# Lombiq eInvoice Validator

[![Lombiq EInvoiceValidator NuGet](https://img.shields.io/nuget/v/Lombiq.EInvoiceValidator?label=Lombiq.EInvoiceValidator)](https://www.nuget.org/packages/Lombiq.EInvoiceValidator/) [![Lombiq EInvoiceValidator Sample NuGet](https://img.shields.io/nuget/v/Lombiq.EInvoiceValidator.Sample?label=Lombiq.EInvoiceValidator.Sample)](https://www.nuget.org/packages/Lombiq.EInvoiceValidator.Sample/)

## About

.NET library to validate EU eInvoices that follow the EN 16931 standard.

Uses [CEN/TC 434 - EN-16931 - Validation artifacts](https://github.com/ConnectingEurope/eInvoicing-EN16931) v1.3.14.1 release validation files for schema (xsd) and schematron (xslt) validation. Currently, ubl - UBL 2.1 (ISO/IEC 19845:2015) and cii - Cross Industry Invoice (D16B) eInvoicing formats are supported.

See a blog post about the library on [Orchard Dojo](https://orcharddojo.net/blog/validate-your-en-16931-einvoices-in-net-with-the-lombiq-einvoice-validator).

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Projects

- The main library for validating eInvoices: [`Lombiq.EInvoiceValidator`](./Lombiq.EInvoiceValidator).
- A sample console application demonstrating how to use the library: [`Lombiq.EInvoiceValidator.Sample`](./Lombiq.EInvoiceValidator.Sample).
- A benchmark project to measure the performance of the validation process: [`Lombiq.EInvoiceValidator.Benchmark`](./Lombiq.EInvoiceValidator.Benchmark).
- Unit tests for the library: [`Lombiq.EInvoiceValidator.Tests`](./Lombiq.EInvoiceValidator.Tests).

## Prerequisites

Node.js is required to run the schematron validation with [SaxonJS](https://www.npmjs.com/package/saxon-js). To have validation files converted from XSLT to SaxonJS compatible `.sef.json` format, we use the [xslt3](https://www.npmjs.com/package/xslt3) command line tool.

You can install Node.js (LTS version recommended) from [here](https://nodejs.org/en/download).

## Getting Started

These samples don't contain the common setup necessary before you can use the validation methods. For those, and to see them in action, check out our sample project: [`Lombiq.EInvoiceValidator.Sample`](./Lombiq.EInvoiceValidator.Sample/Program.cs).

1. Add the NuGet package to your project or use it as a submodule in your solution.
2. Perform full validation (schema + schematron)

    ```csharp
    // DI in constructor the IInvoiceValidationService or get it from the service provider.
    var invoiceValidationService = serviceProvider.GetRequiredService<IInvoiceValidationService>();
    
    // For XML string.
    var validationResult = await invoiceValidationService.ValidateInvoiceAsync(xmlString);
    
    // For XML stream.
    var validationResult = await invoiceValidationService.ValidateInvoiceAsync(xmlStream);
    
    if (validationResult.Successful)
    {
       // Invoice is valid.
    }
    
    if (validationResult.HasWarnings)
    {
       // Handle warnings.
    }
    else
    {
       // Access validationResult.SchemaValidationResult and validationResult.SchematronValidationResult for details.
    }
    ```

## Validation Services

### `InvoiceValidationService`

Validate an XML invoice against both the XSD schema and the schematron rules, as the previous example shows.

But you can also detect the invoice format (UBL or CII) and validate it accordingly with the [`SchemaValidationService`](#schemavalidationservice) and [`SchematronValidationService`](#schematronvalidationservice):

```csharp
// DI in constructor the IInvoiceValidationService or get it from the service provider.
var invoiceValidationService = serviceProvider.GetRequiredService<IInvoiceValidationService>();

// For XML string.
var format = await invoiceValidationService.DetectFormatAsync(xmlString);

// For XML stream.
var format = await invoiceValidationService.DetectFormatAsync(xmlStream);

if (format == InvoiceFormat.UBL)
{
    // Handle UBL invoice.
}
else if (format == InvoiceFormat.CII)
{
    // Handle CII invoice.
}
else
{
    // Unknown or unsupported format.
}
```

### `SchemaValidationService`

Validate an XML invoice against the XSD schema only:

```csharp
// DI in constructor the ISchemaValidationService or get it from the service provider.
var schemaValidationService = serviceProvider.GetRequiredService<ISchemaValidationService>();

// For string XML.
var schemaResult = await schemaValidationService.ValidateXmlAgainstSchemaAsync(
    xmlString,
    InvoiceFormat.CII);

// Or using stream.
var schemaResult = await schemaValidationService.ValidateXmlAgainstSchemaAsync(
    xmlStream,
    InvoiceFormat.CII);

if (schemaResult.ErrorMessages.Any())
{
    // Handle schema validation errors.
}
```

### `SchematronValidationService`

Validate business rules using schematron only:

```csharp
// DI in constructor the ISchematronValidationService or get it from the service provider.
var schematronValidationService = serviceProvider.GetRequiredService<ISchematronValidationService>();

var schematronResult = await schematronValidationService.ExecuteSchematronValidationAsync(
    xmlString,
    InvoiceFormat.Ubl);

if (schematronResult.ErrorFailedAsserts.Any())
{
    // Handle schematron errors.
}

if (schematronResult.WarningFailedAsserts.Any())
{
    // Handle schematron warnings.
}
```

## Validation Artifacts

The validation artifacts (XSD and schematron files) are included in the NuGet package. If you are using this library as a submodule, you can regenerate them by running the _Generate-Validation-Files.ps1_ PowerShell script in the root of the project. This script will download the latest released CII validation files from the [CEN/TC 434 - EN-16931 - Validation artifacts](https://github.com/ConnectingEurope/eInvoicing-EN16931) repository and download the UBL 2.1 validation files from [OASIS UBL 2.1](https://docs.oasis-open.org/ubl/os-UBL-2.1/), then places them in the correct folder.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
