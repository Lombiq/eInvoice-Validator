using Lombiq.EInvoiceValidator.Models;
using System.IO;
using System.Threading.Tasks;

public interface ISchemaValidationServices
{
    Task<SchemaValidationResult> ValidateXmlAgainstSchemaAsync(string xml, InvoiceFormat format);

    Task<SchemaValidationResult> ValidateXmlAgainstSchemaAsync(Stream xmlStream, InvoiceFormat format);
}
