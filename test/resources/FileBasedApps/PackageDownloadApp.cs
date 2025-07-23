using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;

// Advanced file-based app that downloads and inspects a NuGet package
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("File-based app: Package Download and Inspection Demo");
        
        string packageId = "Microsoft.NETCore.App.Ref";
        string version = "8.0.0"; // Use a well-known stable version
        
        Console.WriteLine($"Downloading package: {packageId} version {version}");
        
        try
        {
            using var httpClient = new HttpClient();
            
            // Download the package using NuGet API
            string downloadUrl = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/{version}/{packageId.ToLowerInvariant()}.{version}.nupkg";
            Console.WriteLine($"Download URL: {downloadUrl}");
            
            byte[] packageData = await httpClient.GetByteArrayAsync(downloadUrl);
            Console.WriteLine($"Downloaded package size: {packageData.Length} bytes");
            
            // Save the package temporarily
            string tempFile = Path.GetTempFileName();
            await File.WriteAllBytesAsync(tempFile, packageData);
            
            try
            {
                // Extract and inspect the package
                using var zip = new ZipArchive(new MemoryStream(packageData), ZipArchiveMode.Read);
                
                Console.WriteLine($"Package contains {zip.Entries.Count} files:");
                
                int fileCount = 0;
                foreach (var entry in zip.Entries)
                {
                    if (fileCount < 10) // Show first 10 files
                    {
                        Console.WriteLine($"  - {entry.FullName} ({entry.Length} bytes)");
                    }
                    fileCount++;
                }
                
                if (fileCount > 10)
                {
                    Console.WriteLine($"  ... and {fileCount - 10} more files");
                }
                
                // Look for specific files
                var nuspecEntry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".nuspec"));
                if (nuspecEntry != null)
                {
                    Console.WriteLine($"Found nuspec file: {nuspecEntry.FullName}");
                    
                    // Read a portion of the nuspec file
                    using var stream = nuspecEntry.Open();
                    using var reader = new StreamReader(stream);
                    string nuspecContent = await reader.ReadToEndAsync();
                    
                    // Extract some basic info
                    if (nuspecContent.Contains("<id>"))
                    {
                        int startIndex = nuspecContent.IndexOf("<id>") + 4;
                        int endIndex = nuspecContent.IndexOf("</id>", startIndex);
                        if (endIndex > startIndex)
                        {
                            string extractedId = nuspecContent.Substring(startIndex, endIndex - startIndex);
                            Console.WriteLine($"Package ID from nuspec: {extractedId}");
                        }
                    }
                }
                
                Console.WriteLine("Package inspection completed successfully.");
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("Package download demo completed.");
    }
}