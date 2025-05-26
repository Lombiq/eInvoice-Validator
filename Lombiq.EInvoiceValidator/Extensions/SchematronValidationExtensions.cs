using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Helpers;
using Lombiq.EInvoiceValidator.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lombiq.EInvoiceValidator.Extensions;

public static class SchematronValidationExtensions
{
    private const string CiiSefJson = "Lombiq.EInvoiceValidator.JsonStylesheets.EN16931-CII-validation.sef.json";
    private const string UblSefJson = "Lombiq.EInvoiceValidator.JsonStylesheets.EN16931-UBL-validation.sef.json";
    private const string ValidatorJs = "validator.js";
    private const string ExportName = "validateAsText";
    private const string En16931CiiValidationSefJson = "EN16931-CII-validation.sef.json";
    private const string En16931UblValidationSefJson = "EN16931-UBL-validation.sef.json";
    private const string FailedAssert = "failed-assert";
    private const string HttpPurlOclcOrgDsdlSvrl = "http://purl.oclc.org/dsdl/svrl";

    /// <inheritdoc cref="INodeJSService.InvokeFromFileAsync{T}"/>
    public static async Task<SchematronValidationResult> ExecuteSchematronValidationAsync(
        this INodeJSService nodeJsService,
        IMemoryCache memoryCache,
        string xmlFileToValidate,
        InvoiceFormat format,
        CancellationToken cancellationToken = default)
    {
        var resourceName = format switch
        {
            InvoiceFormat.CII => CiiSefJson,
            InvoiceFormat.UBL => UblSefJson,
            _ => throw new NotSupportedException("Unsupported format"),
        };

        var convertedSchematronFilePath = ResourceHelper.ExtractResourceToTempFile(
            memoryCache,
            resourceName,
            format == InvoiceFormat.CII ? En16931CiiValidationSefJson : En16931UblValidationSefJson);

        var result = await nodeJsService.InvokeFromFileAsync<ScriptValidationResult>(
            ValidatorJs,
            ExportName,
            [convertedSchematronFilePath, xmlFileToValidate],
            cancellationToken);

        if (result.Error != null)
        {
            throw new InvalidOperationException("An unexpected fatal error happened.");
        }

        var schematronValidationResult = new SchematronValidationResult { InnerValidationDurationMs = result.DurationMs };

        using var reader = XmlReader.Create(new StringReader(result.OutputXml), new XmlReaderSettings { Async = true });
        while (await reader.ReadAsync())
        {
            if (IsElement(reader, FailedAssert))
            {
                var failedAssert = ReadFailedAssert(reader);
                (failedAssert.IsError ? schematronValidationResult.ErrorFailedAsserts : schematronValidationResult.WarningFailedAsserts)
                    .Add(failedAssert);
            }
        }

        return schematronValidationResult;
    }

    private static bool IsElement(XmlReader reader, string localName) =>
        reader.NodeType == XmlNodeType.Element &&
        reader.LocalName == localName &&
        reader.NamespaceURI == HttpPurlOclcOrgDsdlSvrl;

    private static FailedAssert ReadFailedAssert(XmlReader reader)
    {
        var id = reader.GetAttribute("id");
        var location = reader.GetAttribute("location");
        var test = reader.GetAttribute("test");
        var flag = reader.GetAttribute("flag");
        var isError = flag?.EqualsOrdinalIgnoreCase("fatal") == true;

        var text = string.Empty;
        int startDepth = reader.Depth;

        // Read to the end of this <failed-assert> element
        while (reader.Read() &&
               !(reader.NodeType == XmlNodeType.EndElement &&
                 reader.LocalName == FailedAssert &&
                 reader.Depth == startDepth))
        {
            if (IsElement(reader, "text"))
            {
                text = reader.ReadElementContentAsString();
            }
        }

        return new FailedAssert(id, location, test, isError, text);
    }
}
