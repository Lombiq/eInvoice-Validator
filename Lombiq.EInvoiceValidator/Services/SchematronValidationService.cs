using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lombiq.EInvoiceValidator.Services;

public class SchematronValidationService : ISchematronValidationService
{
    private const string CiiSefJson = "Lombiq.EInvoiceValidator.JsonStylesheets.EN16931-CII-validation.sef.json";
    private const string UblSefJson = "Lombiq.EInvoiceValidator.JsonStylesheets.EN16931-UBL-validation.sef.json";
    private const string ValidatorJs = "validator.js";
    private const string ExportName = "validateAsText";
    private const string En16931CiiValidationSefJson = "EN16931-CII-validation.sef.json";
    private const string En16931UblValidationSefJson = "EN16931-UBL-validation.sef.json";
    private const string FailedAssert = "failed-assert";
    private const string HttpPurlOclcOrgDsdlSvrl = "http://purl.oclc.org/dsdl/svrl";

    private readonly INodeJSService _nodeJsService;
    private readonly IMemoryCache _memoryCache;

    public SchematronValidationService(INodeJSService nodeJSService, IMemoryCache memoryCache)
    {
        _nodeJsService = nodeJSService;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Executes the Schematron validation for the given XML using the specified format.
    /// </summary>
    /// <exception cref="NotSupportedException">Thrown when the <see cref="InvoiceFormat"/> is <see cref="InvoiceFormat.Unknown"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the schematron validator SaxonJs returns with an exception.</exception>
    public async Task<SchematronValidationResult> ExecuteSchematronValidationAsync(
        string xml,
        InvoiceFormat format,
        CancellationToken cancellationToken = default)
    {
        var resourceName = format switch
        {
            InvoiceFormat.CII => CiiSefJson,
            InvoiceFormat.UBL => UblSefJson,
            _ => throw new NotSupportedException("Unsupported format"),
        };

        var convertedSchematronFilePath = ExtractResourceToTempFile(
            resourceName,
            format == InvoiceFormat.CII ? En16931CiiValidationSefJson : En16931UblValidationSefJson);

        var result = await _nodeJsService.InvokeFromFileAsync<ScriptValidationResult>(
            ValidatorJs,
            ExportName,
            [convertedSchematronFilePath, xml],
            cancellationToken);

        if (result.Error != null)
        {
            throw new InvalidOperationException($"An unexpected fatal error happened: {result.Error}");
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

    private string ExtractResourceToTempFile(string resourceName, string targetFileName) =>
        _memoryCache.GetOrCreate(resourceName, _ =>
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "EInvoiceValidator");
            Directory.CreateDirectory(tempDir);

            var targetPath = Path.Combine(tempDir, targetFileName);

            if (!File.Exists(targetPath))
            {
                var assembly = typeof(SchematronValidationService).Assembly;
                using var resourceStream = assembly.GetManifestResourceStream(resourceName)
                                           ?? throw new InvalidOperationException($"Resource not found: {resourceName}");

                using var fileStream = File.Create(targetPath);
                resourceStream.CopyTo(fileStream);
            }

            return targetPath;
        });
}
