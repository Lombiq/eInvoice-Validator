using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Lombiq.EInvoiceValidator.Extensions;

public static class EInvoiceStartupExtensions
{
    public static void AddEInvoiceValidationServices(this IServiceCollection services)
    {
        services.AddNodeJS();
        services.Configure<NodeJSProcessOptions>(options =>
            options.ProjectPath = Path.GetDirectoryName(typeof(EInvoiceXmlSchemaSet).Assembly.Location) ?? Directory.GetCurrentDirectory());
        services.Configure<OutOfProcessNodeJSServiceOptions>(options => options.Concurrency = Concurrency.MultiProcess);

        services.AddSingleton<IEInvoiceXmlSchemaSet, EInvoiceXmlSchemaSet>();
    }
}
