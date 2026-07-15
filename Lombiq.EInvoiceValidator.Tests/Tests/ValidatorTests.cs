using Lombiq.EInvoiceValidator.Extensions;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lombiq.EInvoiceValidator.Tests.Tests;

public class ValidatorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public static TheoryData<string> InvoiceFilePaths
    {
        get
        {
            var theoryData = new TheoryData<string>();
            var filePath = Path.Combine("Tests", "SampleInvoices");
            foreach (var file in Directory.GetFiles(filePath, "*.xml", SearchOption.AllDirectories))
            {
                theoryData.Add(file);
            }

            return theoryData;
        }
    }

    public ValidatorTests(ITestOutputHelper testOutputHelper) =>
        _testOutputHelper = testOutputHelper;

    [Theory]
    [MemberData(nameof(InvoiceFilePaths))]
    public async Task TestInvoiceValidation(string filePath)
    {
        var directoryTree = new StringBuilder("node_modules directory contents:");
        WriteTree(directoryTree, new DirectoryInfo("node_modules"), depth: 0);
        _testOutputHelper.WriteLine(directoryTree.ToString());

        var services = new ServiceCollection();
        services.AddEInvoiceValidationServices();
        var serviceProvider = services.BuildServiceProvider();

        var invoiceValidationService = serviceProvider.GetRequiredService<IInvoiceValidationService>();

        using var streamReaderInner = new StreamReader(filePath);
        var result = await invoiceValidationService.ValidateInvoiceAsync(
            streamReaderInner.BaseStream,
            cancellationToken: TestContext.Current.CancellationToken);

        if (filePath.Contains("failing"))
        {
            result.Successful.ShouldBeFalse($"{filePath} should fail validation but did not.");
        }
        else
        {
            result.Successful.ShouldBeTrue($"{filePath} should pass validation but did not.");
        }
    }

    private static void WriteTree(StringBuilder builder, DirectoryInfo info, int depth)
    {
        builder.Append(new string(' ', 2 * depth));
        builder.AppendLine(info.Name);

        var nextDepth = depth + 1;

        foreach (var child in info.GetDirectories())
        {
            WriteTree(builder, child, nextDepth);
        }

        foreach (var child in info.GetFiles())
        {
            builder.Append(new string(' ', 2 * nextDepth));
            builder.AppendLine(child.Name);
        }
    }
}
