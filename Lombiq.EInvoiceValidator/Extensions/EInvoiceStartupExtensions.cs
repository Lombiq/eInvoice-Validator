using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Services;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace Lombiq.EInvoiceValidator.Extensions;

public static class EInvoiceStartupExtensions
{
    public static void AddEInvoiceValidationServices(this IServiceCollection services)
    {
        // Add Node.js wrapper services.
        services.AddNodeJS();
        services.Configure<NodeJSProcessOptions>(options =>
            options.ProjectPath = Path.GetDirectoryName(typeof(EInvoiceXmlSchemaSet).Assembly.Location) ?? Directory.GetCurrentDirectory());
        services.Configure<OutOfProcessNodeJSServiceOptions>(options => options.Concurrency = Concurrency.MultiProcess);

        // Add eInvoice validation services.
        services.AddSingleton<IEInvoiceXmlSchemaSet, EInvoiceXmlSchemaSet>();
        services.AddMemoryCache();
        services.AddScoped<IInvoiceValidationService, InvoiceValidationService>();
        services.AddScoped<ISchematronValidationService, SchematronValidationService>();
        services.AddScoped<ISchemaValidationServices, SchemaValidationServices>();
    }
}
