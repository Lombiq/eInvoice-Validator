using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Models;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Helpers;

public static class InvoiceValidationHelper
{
    public static async Task<InvoiceValidationResult> ValidateInvoiceAsync(
        string xml,
        INodeJSService nodeJsService,
        IMemoryCache memoryCache,
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default)
    {
        var invoiceFormat = await InvoiceFormatHelper.DetectFormatAsync(xml);
        var schema = await SchemaValidationHelper.ValidateXmlAgainstSchemaAsync(xml, invoiceFormat, eInvoiceXmlSchemaSet);
        if (stopOnSchemaError && schema.ErrorMessages.Any())
        {
            return new InvoiceValidationResult(schema, SchematronValidationResult: null, invoiceFormat);
        }

        var schematron = await SchematronValidationHelper.ExecuteSchematronValidationAsync(
            xml,
            invoiceFormat,
            nodeJsService,
            memoryCache,
            cancellationToken);

        var (failed, hasWarnings) = DetermineValidationStatus(schema, schematron);

        return new InvoiceValidationResult(schema, schematron, invoiceFormat, Successful: !failed, HasWarnings: hasWarnings);
    }

    public static async Task<InvoiceValidationResult> ValidateInvoiceAsync(
        Stream xmlStream,
        INodeJSService nodeJsService,
        IMemoryCache memoryCache,
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default)
    {
        Stream reusableStream;
        if (xmlStream.CanSeek)
        {
            xmlStream.Position = 0;
            reusableStream = xmlStream;
        }
        else
        {
            var memoryStream = new MemoryStream();
            await xmlStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            reusableStream = memoryStream;
        }

        var invoiceFormat = await InvoiceFormatHelper.DetectFormatAsync(reusableStream);

        ResetStreamPosition(reusableStream);

        var schema = await SchemaValidationHelper.ValidateXmlAgainstSchemaAsync(reusableStream, invoiceFormat, eInvoiceXmlSchemaSet);
        if (stopOnSchemaError && schema.ErrorMessages.Any())
        {
            return new InvoiceValidationResult(schema, SchematronValidationResult: null, invoiceFormat);
        }

        ResetStreamPosition(reusableStream);

        using var reader = new StreamReader(xmlStream);
        var xmlText = await reader.ReadToEndAsync(cancellationToken);
        var schematron = await SchematronValidationHelper.ExecuteSchematronValidationAsync(
            xmlText,
            invoiceFormat,
            nodeJsService,
            memoryCache,
            cancellationToken);

        var (failed, hasWarnings) = DetermineValidationStatus(schema, schematron);

        return new InvoiceValidationResult(schema, schematron, invoiceFormat, Successful: !failed, HasWarnings: hasWarnings);
    }

    private static (bool Failed, bool HasWarnings) DetermineValidationStatus(SchemaValidationResult schema, SchematronValidationResult schematron)
    {
        var failed = (schema != null && schema.ErrorMessages.Any()) || schematron.ErrorFailedAsserts.Count > 0;
        var hasWarnings = schematron?.WarningFailedAsserts.Count > 0;
        return (failed, hasWarnings);
    }

    private static void ResetStreamPosition(Stream reusableStream) => reusableStream.Position = 0;
}
