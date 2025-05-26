using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Models;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Helpers;

public static class InvoiceValidationHelper
{
    public static async Task<InvoiceValidationResult> ValidateInvoiceAsync(
        string xml,
        INodeJSService nodeJsService,
        IMemoryCache memoryCache,
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        bool stopOnSchemaError = false)
    {
        var result = await InvoiceValidationResultAsync(xml, nodeJsService, memoryCache, eInvoiceXmlSchemaSet, stopOnSchemaError);

        var successful = result.SchemaValidationResult?.ErrorMessages.Count == 0 &&
                         result.SchematronValidationResult?.ErrorFailedAsserts.Count == 0;
        var hasWarnings = result.SchematronValidationResult?.WarningFailedAsserts.Count > 0;

        return result with { Successful = successful, HasWarnings = hasWarnings };
    }

    public static async Task<InvoiceValidationResult> ValidateInvoiceAsync(
        Stream xmlStream,
        INodeJSService nodeJsService,
        IMemoryCache memoryCache,
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        bool skipSchemaValidation = false,
        bool stopOnSchemaError = false)
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
            await xmlStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            reusableStream = memoryStream;
        }

        var invoiceFormat = await InvoiceFormatHelper.DetectFormatAsync(reusableStream);

        SchemaValidationResult schema = null;
        if (!skipSchemaValidation)
        {
            // Reset again before passing to schema validation
            reusableStream.Position = 0;

            schema = await SchemaValidationHelper.ValidateXmlAgainstSchemaAsync(eInvoiceXmlSchemaSet, reusableStream, invoiceFormat);
            if (stopOnSchemaError && schema.ErrorMessages.Any())
            {
                return new InvoiceValidationResult(schema, SchematronValidationResult: null);
            }
        }

        // Reset again before passing to schematron validation
        reusableStream.Position = 0;
        using var reader = new StreamReader(xmlStream);
        var xmlText = await reader.ReadToEndAsync();
        var schematron = await nodeJsService.ExecuteSchematronValidationAsync(memoryCache, xmlText, invoiceFormat);

        // Check if there were any errors in the schema or schematron validation.
        var failed = (schema != null && schema.ErrorMessages.Any()) || schematron.ErrorFailedAsserts.Count > 0;

        return new InvoiceValidationResult(schema, schematron, !failed);
    }

    private static async Task<InvoiceValidationResult> InvoiceValidationResultAsync(
        string xml,
        INodeJSService nodeJsService,
        IMemoryCache memoryCache,
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        bool stopOnSchemaError)
    {
        var invoiceFormat = await InvoiceFormatHelper.DetectFormatAsync(xml);
        var schema = await SchemaValidationHelper.ValidateXmlAgainstSchemaAsync(eInvoiceXmlSchemaSet, xml, invoiceFormat);
        if (stopOnSchemaError && schema.ErrorMessages.Any())
        {
            return new InvoiceValidationResult(schema, SchematronValidationResult: null);
        }

        var schematron = await nodeJsService.ExecuteSchematronValidationAsync(memoryCache, xml, invoiceFormat);

        return new InvoiceValidationResult(schema, schematron);
    }
}
