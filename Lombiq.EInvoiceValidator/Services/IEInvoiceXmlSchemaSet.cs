using Lombiq.EInvoiceValidator.Models;
using System.Xml.Schema;

namespace Lombiq.EInvoiceValidator.Services;

public interface IEInvoiceXmlSchemaSet
{
    XmlSchemaSet GetSchemaSet(InvoiceFormat format);
}
