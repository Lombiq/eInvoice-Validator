using Lombiq.EInvoiceValidator.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Services;

/// <summary>
/// Provides a service for executing schematron validation on XML strings for eInvoices.
/// </summary>
public interface ISchematronValidationService
{
    /// <summary>
    /// Executes schematron validation on the provided XML string for the specified invoice format. Calls a Node.js process to perform the validation
    /// with SaxonJs.
    /// </summary>
    Task<SchematronValidationResult> ExecuteSchematronValidationAsync(
        string xml,
        InvoiceFormat format,
        CancellationToken cancellationToken = default);
}
