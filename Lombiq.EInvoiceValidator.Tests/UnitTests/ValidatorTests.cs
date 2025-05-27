using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Helpers;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Lombiq.EInvoiceValidator.Tests.UnitTests;

public class ValidatorTests
{
    [Fact]
    public async Task TestInvoiceValidationHelper()
    {
        var services = new ServiceCollection();
        services.AddEInvoiceValidationServices();
        var serviceProvider = services.BuildServiceProvider();

        var nodeJsService = serviceProvider.GetRequiredService<INodeJSService>();
        var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
        var eInvoiceXmlSchemaSet = serviceProvider.GetRequiredService<IEInvoiceXmlSchemaSet>();

        var filePaths = Directory.GetFiles(Path.Combine("UnitTests", "SampleInvoices"), "*.xml", SearchOption.AllDirectories);

        foreach (var filePath in filePaths)
        {
            using var streamReaderInner = new StreamReader(filePath);
            var result = await InvoiceValidationHelper.ValidateInvoiceAsync(
                streamReaderInner.BaseStream,
                nodeJsService,
                memoryCache,
                eInvoiceXmlSchemaSet,
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
