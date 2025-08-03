# File-Based Apps Scenario Tests

This directory contains scenario tests for file-based apps functionality in .NET SDK.

## Overview

File-based apps allow developers to execute single C# files directly using `dotnet run <file.cs>` without creating a full project structure. This feature enables rapid prototyping and script-like execution of C# code.

## Test Coverage

The `FileBasedAppTests` class provides comprehensive coverage for file-based app scenarios:

### Test Scenarios

1. **VerifySimpleFileBasedApp** - Tests basic execution of a simple HelloWorld.cs file
2. **VerifyFileBasedAppWithPackageSearch** - Tests complex app that searches NuGet packages using HttpClient and System.Text.Json
3. **VerifyFileBasedAppWithPackageDownload** - Tests advanced app that downloads and inspects actual NuGet packages
4. **VerifyFileBasedAppWithArguments** - Tests command line argument processing
5. **VerifyFileBasedAppErrorHandling** - Tests compilation error handling
6. **VerifyPreMadeFileBasedApps** - Tests using pre-made resource files

### Package Search and Download

The tests include complex scenarios that:
- Search for `Microsoft.NETCore.App.Ref` package using NuGet API
- Download packages at specific versions (8.0.0)
- Extract and inspect package contents using ZipArchive
- Parse nuspec files to extract package metadata
- Print detailed package information

## Test Resources

Sample .cs files are provided in `test/resources/FileBasedApps/`:

- **HelloWorld.cs** - Simple console application
- **PackageSearchApp.cs** - Package search functionality using NuGet API
- **PackageDownloadApp.cs** - Package download and inspection demo

## File-Based App Helper

The `FileBasedAppHelper` class provides utilities for:
- Executing .cs files using `dotnet run <file.cs>`
- Creating temporary C# files for testing
- Proper environment configuration for isolated execution
- Error handling and timeout management

## Requirements

These tests require a .NET SDK version that supports file-based apps functionality (typically .NET 6 or later with appropriate SDK features enabled).

## Usage

The tests are designed to be run as part of the overall scenario test suite and will automatically handle cleanup of temporary files and directories.