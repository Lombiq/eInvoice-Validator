#nullable enable
namespace Lombiq.EInvoiceValidator.Models;

public class FailedAssert
{
    public string? Id { get; set; }
    public string? Location { get; set; }
    public string? Test { get; set; }
    public bool IsError { get; set; }
    public string? Text { get; set; }
}
