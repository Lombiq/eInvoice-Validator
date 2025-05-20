using Jering.Javascript.NodeJS;
using Lombiq.EInvoiceValidator.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Lombiq.EInvoiceValidator.Extensions;

public static class NodeJsServiceExtensions
{
    /// <inheritdoc cref="INodeJSService.InvokeFromFileAsync{T}"/>
    public static async Task<SchematronValidationResult> ExecuteSchematronValidationAsync(
        this INodeJSService nodeJsService,
        string format,
        string xmlFileToValidate,
        CancellationToken cancellationToken = default)
    {
        // Temporary.
        var basePath = Path.Combine("..", "Libraries", "Lombiq.EInvoiceValidator", "Lombiq.EInvoiceValidator");

        var validatorScriptFilePath = Path.Join(basePath, "Assets", "Scripts", "validator.js");
        var functionToRunInTheScriptFile = "validateAsText";

        var convertedSchematronFilePath = Directory.GetFiles(
            Path.Join(basePath, "SaxonJsStylesheets"),
            format.EqualsOrdinalIgnoreCase("cii") ? "EN16931-CII-validation.sef.json" : "EN16931-UBL-validation.sef.json")[0];
        var result = await nodeJsService.InvokeFromFileAsync<string>(
            validatorScriptFilePath,
            functionToRunInTheScriptFile,
            [convertedSchematronFilePath, xmlFileToValidate],
            cancellationToken);

        var deserializedResult = JsonSerializer.Deserialize<string>(result);
        if (result.StartsWithOrdinalIgnoreCase("An unexpected fatal error happened:"))
        {
            throw new InvalidOperationException("An unexpected fatal error happened.");
        }

        var schematronValidationResult = new SchematronValidationResult();

        using var reader = XmlReader.Create(new StringReader(deserializedResult), new XmlReaderSettings { Async = true });
        while (await reader.ReadAsync())
        {
            if (IsElement(reader, "failed-assert"))
            {
                var failedAssert = ReadFailedAssert(reader);
                (failedAssert.IsError ? schematronValidationResult.ErrorFailedAsserts : schematronValidationResult.WarningFailedAsserts)
                    .Add(failedAssert);
            }
        }

        return schematronValidationResult;
    }

    private static bool IsElement(XmlReader reader, string localName) =>
        reader.NodeType == XmlNodeType.Element && reader.LocalName == localName && reader.NamespaceURI == "http://purl.oclc.org/dsdl/svrl";

    private static FailedAssert ReadFailedAssert(XmlReader reader)
    {
        var id = reader.GetAttribute("id");
        var location = reader.GetAttribute("location");
        var test = reader.GetAttribute("test");
        var flag = reader.GetAttribute("flag");
        var isError = flag?.EqualsOrdinalIgnoreCase("warning") != true;

        var text = string.Empty;
        int startDepth = reader.Depth;

        // Read to the end of this <failed-assert> element
        while (reader.Read() &&
               !(reader.NodeType == XmlNodeType.EndElement &&
                 reader.LocalName == "failed-assert" &&
                 reader.Depth == startDepth))
        {
            if (IsElement(reader, "text"))
            {
                text = reader.ReadElementContentAsString();
            }
        }

        return new FailedAssert
        {
            Id = id,
            Location = location,
            Test = test,
            IsError = isError,
            Text = text,
        };
    }
}
