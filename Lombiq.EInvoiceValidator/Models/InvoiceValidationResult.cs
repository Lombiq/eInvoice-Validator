#nullable enable

using Lombiq.EInvoiceValidator.Constants;

namespace Lombiq.EInvoiceValidator.Models;

public record InvoiceValidationResult(
    SchemaValidationResult? SchemaValidationResult,
    SchematronValidationResult? SchematronValidationResult,
    InvoiceFormat Format,
    string Standard = Standards.EN16931,
    bool Successful = false,
    bool HasWarnings = false);
