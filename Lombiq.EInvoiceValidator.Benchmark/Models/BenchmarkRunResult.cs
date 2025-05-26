namespace Lombiq.EInvoiceValidator.Benchmark.Models;

public class BenchmarkRunResult
{
    public InvoiceValidationResult Result { get; set; }
    public long ElapsedMilliseconds { get; set; }
}
