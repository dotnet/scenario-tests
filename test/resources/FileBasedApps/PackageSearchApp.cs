using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using System.IO;

// Complex file-based app that uses dotnet package search, JSON/XML conversion, and LINQ queries
class Program
{
    public class PackageInfo
    {
        public string Id { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public long? TotalDownloads { get; set; }
        public string[] Authors { get; set; }
        public string ProjectUrl { get; set; }
        public string[] Tags { get; set; }
    }

    static async Task Main(string[] args)
    {
        Console.WriteLine("File-based app: Enhanced Package Search Demo");
        
        string packageQuery = args.Length > 0 ? args[0] : "Microsoft.Extensions";
        Console.WriteLine($"Searching for packages matching: {packageQuery}");
        
        try
        {
            // Step 1: Use dotnet package search command with JSON output
            var packageSearchResult = await ExecuteDotnetPackageSearch(packageQuery);
            
            if (string.IsNullOrEmpty(packageSearchResult))
            {
                Console.WriteLine("No search results returned.");
                return;
            }

            // Step 2: Parse JSON and verify expected content
            var packages = ParsePackageSearchJson(packageSearchResult);
            
            if (!packages.Any())
            {
                Console.WriteLine("No packages found in search results.");
                return;
            }

            Console.WriteLine($"Found {packages.Count} packages. Validating content...");
            
            // Step 3: Verify JSON contains expected content
            ValidatePackageContent(packages);
            
            // Step 4: Convert JSON data to XML
            var packagesXml = ConvertPackagesToXml(packages);
            
            // Step 5: Use LINQ queries over XML to analyze data
            var analysisResults = AnalyzePackagesWithLinq(packagesXml);
            
            // Step 6: Output formatted table sorted by download counts
            DisplayFormattedResults(analysisResults);
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("Package search and analysis completed.");
    }

    static async Task<string> ExecuteDotnetPackageSearch(string query)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"package search {query} --format json --verbosity detailed --take 10",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start dotnet process");
            }

            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"dotnet package search failed with exit code {process.ExitCode}. Error: {error}");
            }

            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to execute dotnet package search: {ex.Message}");
            throw;
        }
    }

    static List<PackageInfo> ParsePackageSearchJson(string jsonOutput)
    {
        var packages = new List<PackageInfo>();
        
        try
        {
            using var document = JsonDocument.Parse(jsonOutput);
            var root = document.RootElement;
            
            if (root.TryGetProperty("searchResult", out var searchResult))
            {
                foreach (var packageElement in searchResult.EnumerateArray())
                {
                    var package = new PackageInfo
                    {
                        Id = packageElement.TryGetProperty("packageId", out var id) ? id.GetString() : "Unknown",
                        Version = packageElement.TryGetProperty("latestVersion", out var version) ? version.GetString() : "Unknown",
                        Description = packageElement.TryGetProperty("description", out var desc) ? desc.GetString() : "No description",
                        TotalDownloads = packageElement.TryGetProperty("totalDownloads", out var downloads) ? downloads.GetInt64() : 0,
                        ProjectUrl = packageElement.TryGetProperty("projectUrl", out var url) ? url.GetString() : null
                    };
                    
                    // Parse authors array
                    if (packageElement.TryGetProperty("authors", out var authorsElement))
                    {
                        var authorsList = new List<string>();
                        foreach (var author in authorsElement.EnumerateArray())
                        {
                            authorsList.Add(author.GetString() ?? "Unknown");
                        }
                        package.Authors = authorsList.ToArray();
                    }
                    else
                    {
                        package.Authors = new[] { "Unknown" };
                    }
                    
                    // Parse tags array
                    if (packageElement.TryGetProperty("tags", out var tagsElement))
                    {
                        var tagsList = new List<string>();
                        foreach (var tag in tagsElement.EnumerateArray())
                        {
                            tagsList.Add(tag.GetString() ?? "");
                        }
                        package.Tags = tagsList.ToArray();
                    }
                    else
                    {
                        package.Tags = Array.Empty<string>();
                    }
                    
                    packages.Add(package);
                }
            }
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
            throw;
        }
        
        return packages;
    }

    static void ValidatePackageContent(List<PackageInfo> packages)
    {
        Console.WriteLine("Validating package content:");
        
        // Check for expected content
        var hasVersions = packages.All(p => !string.IsNullOrEmpty(p.Version));
        var hasProjectUrls = packages.Any(p => !string.IsNullOrEmpty(p.ProjectUrl));
        var hasDownloadCounts = packages.Any(p => p.TotalDownloads > 0);
        
        Console.WriteLine($"- All packages have versions: {hasVersions}");
        Console.WriteLine($"- Some packages have project URLs: {hasProjectUrls}");
        Console.WriteLine($"- Some packages have download counts: {hasDownloadCounts}");
        
        if (!hasVersions)
        {
            throw new InvalidOperationException("Expected version information not found");
        }
    }

    static XDocument ConvertPackagesToXml(List<PackageInfo> packages)
    {
        Console.WriteLine("Converting JSON data to XML...");
        
        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("Packages",
                packages.Select(p => new XElement("Package",
                    new XAttribute("Id", p.Id ?? "Unknown"),
                    new XElement("Version", p.Version ?? "Unknown"),
                    new XElement("Description", p.Description ?? "No description"),
                    new XElement("TotalDownloads", p.TotalDownloads ?? 0),
                    new XElement("ProjectUrl", p.ProjectUrl ?? ""),
                    new XElement("Authors", 
                        (p.Authors ?? Array.Empty<string>()).Select(a => new XElement("Author", a))),
                    new XElement("Tags",
                        (p.Tags ?? Array.Empty<string>()).Select(t => new XElement("Tag", t)))
                ))
            )
        );
        
        Console.WriteLine($"Created XML document with {packages.Count} package entries");
        return xml;
    }

    static List<(string Id, string Version, long Downloads, string Url, string AuthorList)> AnalyzePackagesWithLinq(XDocument xml)
    {
        Console.WriteLine("Analyzing packages using LINQ queries over XML...");
        
        // Complex LINQ query demonstrating various .NET functionality
        var analysisResults = xml.Descendants("Package")
            .Where(p => p.Element("TotalDownloads") != null)
            .Select(p => new
            {
                Id = p.Attribute("Id")?.Value ?? "Unknown",
                Version = p.Element("Version")?.Value ?? "Unknown",
                Downloads = long.TryParse(p.Element("TotalDownloads")?.Value, out var downloads) ? downloads : 0,
                Url = p.Element("ProjectUrl")?.Value ?? "Not specified",
                Authors = p.Element("Authors")?.Elements("Author").Select(a => a.Value).ToArray() ?? Array.Empty<string>()
            })
            .OrderByDescending(p => p.Downloads)
            .ThenBy(p => p.Id)
            .Take(10)
            .Select(p => (
                Id: p.Id,
                Version: p.Version,
                Downloads: p.Downloads,
                Url: p.Url,
                AuthorList: string.Join(", ", p.Authors.Length > 0 ? p.Authors : new[] { "Unknown" })
            ))
            .ToList();
        
        Console.WriteLine($"Analysis completed. Top {analysisResults.Count} packages by download count:");
        
        return analysisResults;
    }

    static void DisplayFormattedResults(List<(string Id, string Version, long Downloads, string Url, string AuthorList)> results)
    {
        Console.WriteLine("\n" + new string('=', 120));
        Console.WriteLine("PACKAGE ANALYSIS RESULTS - SORTED BY DOWNLOAD COUNT");
        Console.WriteLine(new string('=', 120));
        
        // Header
        Console.WriteLine($"{"Package ID",-35} {"Version",-15} {"Downloads",-15} {"Authors",-25} {"Project URL",-30}");
        Console.WriteLine(new string('-', 120));
        
        // Data rows
        foreach (var package in results)
        {
            var truncatedUrl = package.Url.Length > 28 ? package.Url.Substring(0, 25) + "..." : package.Url;
            var truncatedAuthors = package.AuthorList.Length > 23 ? package.AuthorList.Substring(0, 20) + "..." : package.AuthorList;
            
            Console.WriteLine($"{package.Id,-35} {package.Version,-15} {package.Downloads,-15:N0} {truncatedAuthors,-25} {truncatedUrl,-30}");
        }
        
        Console.WriteLine(new string('-', 120));
        
        // Summary statistics using additional LINQ
        if (results.Any())
        {
            var totalDownloads = results.Sum(r => r.Downloads);
            var avgDownloads = results.Average(r => r.Downloads);
            var maxDownloads = results.Max(r => r.Downloads);
            var minDownloads = results.Min(r => r.Downloads);
            
            Console.WriteLine($"Summary: {results.Count} packages, Total Downloads: {totalDownloads:N0}, Average: {avgDownloads:N0}, Max: {maxDownloads:N0}, Min: {minDownloads:N0}");
        }
        
        Console.WriteLine(new string('=', 120));
    }
}