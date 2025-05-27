using Lombiq.EInvoiceValidator.Models;
using System.Xml.Schema;

namespace Lombiq.EInvoiceValidator.Services;

/// <summary>
/// Provides the XML schema set for validating e-invoices against the specified invoice format.
/// </summary>
public interface IEInvoiceXmlSchemaSet
{
    /// <summary>
    /// Gets the XML schema set for the specified invoice format.
    /// </summary>
    XmlSchemaSet GetSchemaSet(InvoiceFormat format);
}
