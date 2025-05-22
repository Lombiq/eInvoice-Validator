#nullable enable
namespace Lombiq.EInvoiceValidator.Models;

public record FailedAssert(string? Id, string? Location, string? Test, bool IsError, string? Text);
