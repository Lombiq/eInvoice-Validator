using Lombiq.EInvoiceValidator.Models;

namespace EInvoiceValidator.Benchmark.Models;

public class BenchmarkRunResult
{
    public InvoiceValidationResult Result { get; set; }
    public long ElapsedMilliseconds { get; set; }
}
