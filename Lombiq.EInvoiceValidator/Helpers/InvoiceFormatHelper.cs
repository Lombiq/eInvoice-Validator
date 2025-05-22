using Lombiq.EInvoiceValidator.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Lombiq.EInvoiceValidator.Helpers;

public static class InvoiceFormatHelper
{
    public static async Task<InvoiceFormat> DetectFormatAsync(string xmlContent)
    {
        using var reader = XmlReader.Create(new StringReader(xmlContent), new XmlReaderSettings
        {
            Async = true,
        });

        return await DetermineInvoiceFormatAsync(reader);
    }

    public static async Task<InvoiceFormat> DetectFormatAsync(Stream xmlStream)
    {
        // Ensure stream is at beginning.
        if (xmlStream.CanSeek) xmlStream.Position = 0;

        using var reader = XmlReader.Create(xmlStream, new XmlReaderSettings
        {
            Async = true,
        });

        return await DetermineInvoiceFormatAsync(reader);
    }

    private static async Task<InvoiceFormat> DetermineInvoiceFormatAsync(XmlReader reader)
    {
        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element) continue;

            var localName = reader.LocalName;
            var ns = reader.NamespaceURI;

            return (localName, ns) switch
            {
                ("Invoice", { } s)
                    when s.StartsWithOrdinalIgnoreCase("urn:oasis:names:specification:ubl:schema:xsd")
                    => InvoiceFormat.UBL,

                ("CrossIndustryInvoice", { } s)
                    when s.StartsWithOrdinalIgnoreCase("urn:un:unece:uncefact:data:standard:CrossIndustryInvoice")
                    => InvoiceFormat.CII,

                _ => InvoiceFormat.Unknown,
            };
        }

        return InvoiceFormat.Unknown;
    }
}
