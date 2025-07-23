using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// Complex file-based app that searches for NuGet packages and retrieves information
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("File-based app: Package Search Demo");
        
        string packageId = "Microsoft.NETCore.App.Ref";
        Console.WriteLine($"Searching for package: {packageId}");
        
        try
        {
            using var httpClient = new HttpClient();
            
            // Search for the package using NuGet API
            string searchUrl = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json";
            Console.WriteLine($"Querying: {searchUrl}");
            
            string response = await httpClient.GetStringAsync(searchUrl);
            
            using JsonDocument doc = JsonDocument.Parse(response);
            JsonElement root = doc.RootElement;
            
            if (root.TryGetProperty("versions", out JsonElement versionsElement))
            {
                Console.WriteLine($"Found package: {packageId}");
                Console.WriteLine("Available versions:");
                
                int count = 0;
                foreach (JsonElement version in versionsElement.EnumerateArray())
                {
                    if (count < 5) // Show first 5 versions
                    {
                        Console.WriteLine($"  - {version.GetString()}");
                    }
                    count++;
                }
                
                if (count > 5)
                {
                    Console.WriteLine($"  ... and {count - 5} more versions");
                }
                
                Console.WriteLine($"Total versions found: {count}");
                
                // Get the latest version info
                if (versionsElement.GetArrayLength() > 0)
                {
                    var latestVersion = versionsElement[versionsElement.GetArrayLength() - 1].GetString();
                    Console.WriteLine($"Latest version: {latestVersion}");
                    
                    // Get package metadata
                    string metadataUrl = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/{latestVersion}/{packageId.ToLowerInvariant()}.nuspec";
                    Console.WriteLine($"Package metadata URL: {metadataUrl}");
                }
            }
            else
            {
                Console.WriteLine("No versions found for this package.");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        
        Console.WriteLine("Package search completed.");
    }
}