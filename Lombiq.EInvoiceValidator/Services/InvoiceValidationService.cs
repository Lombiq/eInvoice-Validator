using Lombiq.EInvoiceValidator.Helpers;
using Lombiq.EInvoiceValidator.Models;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Services;

public class InvoiceValidationService : IInvoiceValidationService
{
    private readonly ISchematronValidationService _schematronValidationService;
    private readonly ISchemaValidationServices _schemaValidationServices;

    public InvoiceValidationService(
        ISchemaValidationServices schemaValidationServices,
        ISchematronValidationService schematronValidationService)
    {
        _schemaValidationServices = schemaValidationServices;
        _schematronValidationService = schematronValidationService;
    }

    public async Task<InvoiceValidationResult> ValidateInvoiceAsync(
        string xml,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default)
    {
        var invoiceFormat = await InvoiceFormatHelper.DetectFormatAsync(xml);
        var schema = await _schemaValidationServices.ValidateXmlAgainstSchemaAsync(xml, invoiceFormat);
        if (stopOnSchemaError && schema.ErrorMessages.Any())
        {
            return new InvoiceValidationResult(schema, SchematronValidationResult: null, invoiceFormat);
        }

        var schematron = await _schematronValidationService.ExecuteSchematronValidationAsync(xml, invoiceFormat, cancellationToken);

        var (failed, hasWarnings) = DetermineValidationStatus(schema, schematron);

        return new InvoiceValidationResult(schema, schematron, invoiceFormat, Successful: !failed, HasWarnings: hasWarnings);
    }

    public async Task<InvoiceValidationResult> ValidateInvoiceAsync(
        Stream xmlStream,
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

        var schema = await _schemaValidationServices.ValidateXmlAgainstSchemaAsync(reusableStream, invoiceFormat);
        if (stopOnSchemaError && schema.ErrorMessages.Any())
        {
            return new InvoiceValidationResult(schema, SchematronValidationResult: null, invoiceFormat);
        }

        ResetStreamPosition(reusableStream);

        using var reader = new StreamReader(xmlStream);
        var xmlText = await reader.ReadToEndAsync(cancellationToken);

        var schematron = await _schematronValidationService.ExecuteSchematronValidationAsync(xmlText, invoiceFormat, cancellationToken);

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
