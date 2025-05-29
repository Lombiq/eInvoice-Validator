using Lombiq.EInvoiceValidator.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Services;

public interface IInvoiceValidationService
{
    Task<InvoiceValidationResult> ValidateInvoiceAsync(
        string xml,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default);

    Task<InvoiceValidationResult> ValidateInvoiceAsync(
        Stream xmlStream,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default);

    Task<InvoiceFormat> DetectFormatAsync(string xmlContent);

    Task<InvoiceFormat> DetectFormatAsync(Stream xmlStream);
}
