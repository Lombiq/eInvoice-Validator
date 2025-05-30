using Lombiq.EInvoiceValidator.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Services;

/// <summary>
/// Provides methods for validating eInvoices against the eInvoice schema and schematron rules (EN 16931 UBL or CII).
/// </summary>
public interface IInvoiceValidationService
{
    /// <summary>
    /// Validates the given XML content against the eInvoice schema and schematron rules (EN 16931 UBL or CII).
    /// </summary>
    /// <param name="stopOnSchemaError">
    /// If set to <see langword="true"/> schematron validation will be skipped in case there are schema errors.
    /// </param>
    /// <returns>Returns an <see cref="InvoiceValidationResult"/> object with the result.</returns>
    Task<InvoiceValidationResult> ValidateInvoiceAsync(
        string xml,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the given XML stream against the eInvoice schema and schematron rules (EN 16931 UBL or CII).
    /// </summary>
    /// <param name="stopOnSchemaError">
    /// If set to <see langword="true"/> schematron validation will be skipped in case there are schema errors.
    /// </param>
    /// <returns>Returns an <see cref="InvoiceValidationResult"/> object with the result.</returns>
    Task<InvoiceValidationResult> ValidateInvoiceAsync(
        Stream xmlStream,
        bool stopOnSchemaError = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects the invoice format (CII or UBL) based on the provided XML content.
    /// </summary>
    Task<InvoiceFormat> DetectFormatAsync(string xmlContent);

    /// <summary>
    /// Detects the invoice format (CII or UBL) based on the provided XML stream.
    /// </summary>
    Task<InvoiceFormat> DetectFormatAsync(Stream xmlStream);
}
