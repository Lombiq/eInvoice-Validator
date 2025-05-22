using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;

namespace Lombiq.EInvoiceValidator.Helpers;

public static class ResourceHelper
{
    public static string ExtractResourceToTempFile(IMemoryCache memoryCache, string resourceName, string targetFileName) =>
        memoryCache.GetOrCreate(resourceName, _ =>
        {
            var tempDir = Path.Combine(Path.GetTempPath(), "EInvoiceValidator");
            Directory.CreateDirectory(tempDir);

            var targetPath = Path.Combine(tempDir, targetFileName);

            if (!File.Exists(targetPath))
            {
                var assembly = typeof(ResourceHelper).Assembly;
                using var resourceStream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Resource not found: {resourceName}");

                using var fileStream = File.Create(targetPath);
                resourceStream.CopyTo(fileStream);
            }

            return targetPath;
        });
}
