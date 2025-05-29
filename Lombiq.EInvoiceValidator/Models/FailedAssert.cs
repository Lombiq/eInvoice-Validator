namespace Lombiq.EInvoiceValidator.Models;

public record FailedAssert(string? Id, string? Location, string? Test, bool IsError, string? Text)
{
    public override string ToString() =>
        $"{(IsError ? "Error" : "Warning")}: {Text} \n" +
        $"ID: {Id}) \n" +
        $"Location: {Location}) \n" +
        $"Test: {Test}";
}
