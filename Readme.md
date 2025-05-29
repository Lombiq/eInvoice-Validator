# Lombiq eInvoice Validator

[![Lombiq EInvoiceValidator NuGet](https://img.shields.io/nuget/v/Lombiq.EInvoiceValidator?label=Lombiq.EInvoiceValidator)](https://www.nuget.org/packages/Lombiq.EInvoiceValidator/)

## About

.NET library to validate EU eInvoices that follow the EN 16931 standard.

Uses [CEN/TC 434 - EN-16931 - Validation artifacts](https://github.com/ConnectingEurope/eInvoicing-EN16931) v1.3.14.1 release validation files for schema (xsd) and schematron (xslt) validation. Currently, ubl - UBL 2.1 (ISO/IEC 19845:2015) and cii - Cross Industry Invoice (D16B) eInvoicing formats are supported.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

We have a benchmark project to measure the performance of the validation process. You can find it here: [`Lombiq.EInvoiceValidator.Benchmark`](./Lombiq.EInvoiceValidator.Benchmark/Readme.md).

## Prerequisites

- Node.js

## Getting Started

These samples don't contain the common setup necessary before you can use the validation methods. For those, and to see them in action, check out our sample project: [`Lombiq.EInvoiceValidator.Sample`](./Lombiq.EInvoiceValidator.Sample/Program.cs).

1. Add the NuGet package to your project or use it as a submodule in your solution.
2. Perform full validation (schema + schematron)

    ```csharp
    var validationResult = await InvoiceValidationHelper.ValidateInvoiceAsync(
        xmlString,
        nodeJsService,
        memoryCache,
        eInvoiceXmlSchemaSet);
    
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

## Validation Helpers

### `SchemaValidationHelper`

Validate an XML invoice against the XSD schema only:

```csharp
// For string XML.
var schemaResult = await SchemaValidationHelper.ValidateXmlAgainstSchemaAsync(
    xmlString,
    InvoiceFormat.CII,
    eInvoiceXmlSchemaSet);

// Or using stream.
var schemaResult = await SchemaValidationHelper.ValidateXmlAgainstSchemaAsync(
    xmlStream,
    InvoiceFormat.CII,
    eInvoiceXmlSchemaSet);

if (schemaResult.ErrorMessages.Any())
{
    // Handle schema validation errors.
}
```

### `SchematronValidationHelper`

Validate business rules using schematron only:

```csharp
using Lombiq.EInvoiceValidator.Helpers;

var schematronResult = await SchematronValidationHelper.ExecuteSchematronValidationAsync(
    xmlString,
    InvoiceFormat.Ubl,
    nodeJsService,
    memoryCache);

if (schematronResult.ErrorFailedAsserts.Any())
{
    // Handle schematron errors.
}

if (schematronResult.WarningFailedAsserts.Any())
{
    // Handle schematron warnings.
}
```

### `InvoiceFormatHelper`

The `InvoiceFormatHelper` class is used to detect the format of an e-invoice XML (either UBL or CII). It provides async methods to analyze either a string or a stream containing XML and returns the detected `InvoiceFormat` enum value.

```csharp
// For XML string.
var format = await InvoiceFormatHelper.DetectFormatAsync(xmlString);

// For XML stream.
var format = await InvoiceFormatHelper.DetectFormatAsync(xmlStream);

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

This helper is typically used before validation to determine which schema and schematron to apply.

## Validation Artifacts

The validation artifacts (XSD and Schematron files) are included in the NuGet package. If you are using this library as a submodule, you can regenerate them by running the __Generate-Validation-Files.ps1__ PowerShell script in the root of the project. This script will download the latest released CII validation files from the [CEN/TC 434 - EN-16931 - Validation artifacts](https://github.com/ConnectingEurope/eInvoicing-EN16931) repository and download the UBL 2.1 validation files from [OASIS UBL 2.1](https://docs.oasis-open.org/ubl/os-UBL-2.1/), then places them in the correct folder.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
