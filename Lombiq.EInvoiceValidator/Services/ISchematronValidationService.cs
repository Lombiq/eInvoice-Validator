using Lombiq.EInvoiceValidator.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Services;

public interface ISchematronValidationService
{
    Task<SchematronValidationResult> ExecuteSchematronValidationAsync(
        string xml,
        InvoiceFormat format,
        CancellationToken cancellationToken = default);
}
