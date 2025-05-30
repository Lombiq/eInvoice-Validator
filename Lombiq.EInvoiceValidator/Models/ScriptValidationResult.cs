namespace Lombiq.EInvoiceValidator.Models;

public record ScriptValidationResult(string OutputXml, int DurationMs, string? Error = null);
