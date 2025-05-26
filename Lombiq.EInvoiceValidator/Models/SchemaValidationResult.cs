using System.Collections.Generic;
using System.Xml.Schema;

namespace Lombiq.EInvoiceValidator.Models;

public class SchemaValidationResult
{
    public IList<string> ErrorMessages { get; } = [];
    public IList<ValidationEventArgs> ValidationEventArgs { get; } = [];
}
