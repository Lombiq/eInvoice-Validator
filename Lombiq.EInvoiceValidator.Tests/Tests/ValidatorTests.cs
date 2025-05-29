using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Lombiq.EInvoiceValidator.Tests.Tests;

public class ValidatorTests
{
    [Fact]
    public async Task TestInvoiceValidationHelper()
    {
        var services = new ServiceCollection();
        services.AddEInvoiceValidationServices();
        var serviceProvider = services.BuildServiceProvider();

        var invoiceValidationService = serviceProvider.GetRequiredService<IInvoiceValidationService>();

        var filePaths = Directory.GetFiles(Path.Combine("UnitTests", "SampleInvoices"), "*.xml", SearchOption.AllDirectories);

        foreach (var filePath in filePaths)
        {
            using var streamReaderInner = new StreamReader(filePath);
            var result = await invoiceValidationService.ValidateInvoiceAsync(
                streamReaderInner.BaseStream,
                cancellationToken: TestContext.Current.CancellationToken);

            if (filePath.Contains("failing"))
            {
                result.Successful.ShouldBeFalse();
            }
            else
            {
                result.Successful.ShouldBeTrue();
            }
        }
    }
}
