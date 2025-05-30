using Lombiq.EInvoiceValidator.Models;
using System.IO;
using System.Threading.Tasks;

namespace Lombiq.EInvoiceValidator.Services;

/// <summary>
/// Provides services for validating XML documents against schemas for different invoice formats.
/// </summary>
public interface ISchemaValidationServices
{
    /// <summary>
    /// Validates the given XML string against the schema for the specified invoice format.
    /// </summary>
    Task<SchemaValidationResult> ValidateXmlAgainstSchemaAsync(string xml, InvoiceFormat format);

    /// <summary>
    /// Validates the given XML stream against the schema for the specified invoice format.
    /// </summary>
    Task<SchemaValidationResult> ValidateXmlAgainstSchemaAsync(Stream xmlStream, InvoiceFormat format);
}
