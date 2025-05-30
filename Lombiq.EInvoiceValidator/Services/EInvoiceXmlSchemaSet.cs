using Lombiq.EInvoiceValidator.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Lombiq.EInvoiceValidator.Services;

public class EInvoiceXmlSchemaSet : IEInvoiceXmlSchemaSet
{
    private readonly Assembly _assembly;
    private XmlSchemaSet CiiSchemaSet { get; }
    private XmlSchemaSet UblSchemaSet { get; }

    public EInvoiceXmlSchemaSet()
    {
        _assembly = typeof(EInvoiceXmlSchemaSet).Assembly;
        CiiSchemaSet = LoadSchemaSet("SchemaFiles.CII");
        UblSchemaSet = LoadSchemaSet("SchemaFiles.UBL");
    }

    public XmlSchemaSet GetSchemaSet(InvoiceFormat format) =>
        format == InvoiceFormat.CII ? CiiSchemaSet : UblSchemaSet;

    private XmlSchemaSet LoadSchemaSet(string schemaFolderNamespace)
    {
        var schemaSet = new XmlSchemaSet();

        var xmlReaderSettings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore };

        var resourceNames = _assembly.GetManifestResourceNames()
            .Where(name =>
                name.ContainsOrdinalIgnoreCase(schemaFolderNamespace) &&
                name.EndsWithOrdinalIgnoreCase(".xsd"))
            .ToList();

        foreach (var resourceName in resourceNames)
        {
            using var stream = _assembly.GetManifestResourceStream(resourceName);
            if (stream is null) continue;

            using var reader = XmlReader.Create(stream, xmlReaderSettings);
            schemaSet.Add(targetNamespace: null, reader);
        }

        schemaSet.Compile();
        return schemaSet;
    }
}
