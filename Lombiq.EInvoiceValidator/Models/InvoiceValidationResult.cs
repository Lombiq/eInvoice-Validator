#nullable enable

namespace Lombiq.EInvoiceValidator.Models;

public record InvoiceValidationResult(
    SchemaValidationResult? SchemaValidationResult,
    SchematronValidationResult? SchematronValidationResult,
    long TotalValidationDurationMs = 0,
    bool Successful = false,
    bool HasWarnings = false);
