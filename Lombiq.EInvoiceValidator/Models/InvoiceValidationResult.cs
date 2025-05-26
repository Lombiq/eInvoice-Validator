#nullable enable

namespace Lombiq.EInvoiceValidator.Models;

public record InvoiceValidationResult(
    SchemaValidationResult? SchemaValidationResult,
    SchematronValidationResult? SchematronValidationResult,
    bool Successful = false,
    bool HasWarnings = false);
