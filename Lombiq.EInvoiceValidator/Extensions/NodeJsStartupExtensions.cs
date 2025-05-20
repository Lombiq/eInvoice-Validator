using Jering.Javascript.NodeJS;
using Microsoft.Extensions.DependencyInjection;

namespace Lombiq.EInvoiceValidator.Extensions;

public static class NodeJsStartupExtensions
{
    public static void AddNodeJsWrapper(this IServiceCollection services)
    {
        services.AddNodeJS();
        services.Configure<OutOfProcessNodeJSServiceOptions>(options => options.Concurrency = Concurrency.MultiProcess);
    }
}
