using Lombiq.EInvoiceValidator.Models;
using Lombiq.EInvoiceValidator.Services;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace Lombiq.EInvoiceValidator.Helpers;

public static class SchemaValidationHelper
{
    public static async Task<SchemaValidationResult> ValidateXmlAgainstSchemaAsync(
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        string xml,
        InvoiceFormat format)
    {
        var result = new SchemaValidationResult();
        var xmlSchemaSet = eInvoiceXmlSchemaSet.GetSchemaSet(format);
        var xmlReaderSettings = CreateReaderSettings(xmlSchemaSet, ValidationEventHandler(result));

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings);
        await ValidateWithReaderAsync(xmlReader);

        return result;
    }

    public static async Task<SchemaValidationResult> ValidateXmlAgainstSchemaAsync(
        IEInvoiceXmlSchemaSet eInvoiceXmlSchemaSet,
        Stream xmlStream,
        InvoiceFormat format)
    {
        var result = new SchemaValidationResult();

        if (xmlStream.CanSeek) xmlStream.Position = 0;

        var xmlSchemaSet = eInvoiceXmlSchemaSet.GetSchemaSet(format);
        var xmlReaderSettings = CreateReaderSettings(xmlSchemaSet, ValidationEventHandler(result));

        using var xmlReader = XmlReader.Create(xmlStream, xmlReaderSettings);
        await ValidateWithReaderAsync(xmlReader);

        return result;
    }

    private static ValidationEventHandler ValidationEventHandler(SchemaValidationResult result) =>
        (_, args) =>
        {
            result.ValidationEventArgs.Add(args);
            result.ErrorMessages.Add(args.Message);
        };

    private static XmlReaderSettings CreateReaderSettings(XmlSchemaSet schemaSet, ValidationEventHandler validationEventHandler)
    {
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = schemaSet,
            Async = true,
        };

        settings.ValidationEventHandler += validationEventHandler;

        return settings;
    }

    private static async Task ValidateWithReaderAsync(XmlReader reader)
    {
        while (await reader.ReadAsync())
        {
            // Trigger validation passively while going through the XML.
        }
    }
}
