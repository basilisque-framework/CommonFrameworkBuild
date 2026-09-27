// Copyright 2026 Alexander Stärk
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Consumer.Framework;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

namespace CommonFrameworkBuild.Integration.Tests;

internal static class Fixture
{
    public static Assembly Assembly => typeof(FixtureInfo).Assembly;

    public static string Property(string name) => Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
        .Single(attribute => attribute.Key == name).Value ?? throw new InvalidDataException($"Missing fixture property {name}.");

    public static async Task<Dictionary<string, string>> Evaluate(string? projectPath = null, bool outerBuild = false, params string[] properties)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        var arguments = new[]
        {
            "msbuild", projectPath ?? Property("FixtureProjectPath"), "-nologo",
            "-getProperty:RootNamespace,AssemblyName,PackageId,Authors,PackageReleaseNotes,PackageLicenseExpression,PackageRequireLicenseAcceptance,IsCrossTargetingBuild,Nullable,ImplicitUsings,GenerateDocumentationFile",
            $"-p:Configuration={Property("Configuration")}",
            $"-p:TargetFramework={(outerBuild ? string.Empty : $"net{Environment.Version.Major}.0")}"
        };
        foreach (var argument in arguments.Concat(properties))
            startInfo.ArgumentList.Add(argument);
        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Could not start dotnet.");
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"MSBuild evaluation timed out.\n{await outputTask}\n{await errorTask}");
        }
        var output = await outputTask;
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"MSBuild evaluation failed ({process.ExitCode}).\n{output}\n{await errorTask}");
        using var document = JsonDocument.Parse(output);
        return document.RootElement.GetProperty("Properties").EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.GetString()!);
    }
}