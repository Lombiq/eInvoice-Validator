using System.Collections.Generic;

namespace Lombiq.EInvoiceValidator.Models;

public class ValidationResult
{
    public bool Successful { get; set; }
    public string Standard { get; set; }
    public string Format { get; set; }
    public IList<string> Errors { get; } = [];
    public IList<string> Warnings { get; } = [];
}
