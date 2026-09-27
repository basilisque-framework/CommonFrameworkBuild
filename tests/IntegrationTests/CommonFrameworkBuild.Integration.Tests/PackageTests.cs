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

using System.IO.Compression;
using System.Xml.Linq;

namespace CommonFrameworkBuild.Integration.Tests;

public class PackageTests
{
    private static string ProducerPackage => Directory.EnumerateFiles(Fixture.Property("FrameworkBuildPackagePath"), "*.nupkg").Single();

    private static string ConsumerPackage => Path.Combine(Fixture.Property("ArtifactsPath"), "package",
        Fixture.Property("Configuration").ToLowerInvariant(), $"{Fixture.Property("PackageId")}.{Fixture.Property("PackageVersion")}.nupkg");

    private static XElement Metadata(ZipArchive archive, string packageId)
    {
        using var stream = (archive.GetEntry($"{packageId}.nuspec") ?? throw new InvalidDataException("Package manifest missing.")).Open();
        var document = XDocument.Load(stream);
        return document.Root!.Element(document.Root.Name.Namespace + "metadata")!;
    }

    [Test]
    public async Task Producer_Contains_Build_Imports_But_No_Runtime_Assembly()
    {
        using var archive = ZipFile.OpenRead(ProducerPackage);
        var entries = archive.Entries.Select(entry => entry.FullName).ToHashSet(StringComparer.Ordinal);
        foreach (var directory in new[] { "build", "buildMultiTargeting" })
        {
            await Assert.That(entries.Contains($"{directory}/Basilisque.CommonFrameworkBuild.props")).IsTrue();
            await Assert.That(entries.Contains($"{directory}/Basilisque.CommonFrameworkBuild.targets")).IsTrue();
        }
        foreach (var file in new[] { "Before.Shared.props", "Before.Shared.targets", "After.Shared.props", "After.Shared.targets" })
            await Assert.That(entries.Contains($"buildMultiTargeting/Basilisque.CommonFrameworkBuild.{file}")).IsTrue();
        await Assert.That(entries.Any(entry => entry.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))).IsFalse();
    }

    [Test]
    public async Task Producer_Declares_CommonBuild_As_A_Package_Dependency()
    {
        using var archive = ZipFile.OpenRead(ProducerPackage);
        var metadata = Metadata(archive, "Basilisque.CommonFrameworkBuild");
        var dependency = metadata.Descendants(metadata.Name.Namespace + "dependency")
            .Single(element => element.Attribute("id")?.Value == "Basilisque.CommonBuild");
        await Assert.That(string.IsNullOrWhiteSpace(dependency.Attribute("version")?.Value)).IsFalse();
    }

    [Test]
    public async Task Consumer_Nuspec_Contains_Framework_Metadata()
    {
        using var archive = ZipFile.OpenRead(ConsumerPackage);
        var metadata = Metadata(archive, Fixture.Property("PackageId"));
        var xmlNamespace = metadata.Name.Namespace;
        await Assert.That(metadata.Element(xmlNamespace + "id")!.Value).IsEqualTo("Basilisque.Consumer.Framework");
        await Assert.That(metadata.Element(xmlNamespace + "version")!.Value).IsEqualTo(Fixture.Property("PackageVersion"));
        await Assert.That(metadata.Element(xmlNamespace + "authors")!.Value).IsEqualTo("Alexander St\u00e4rk");
        await Assert.That(metadata.Element(xmlNamespace + "releaseNotes")!.Value).IsEqualTo("https://github.com/basilisque-framework/Consumer.Framework/releases");
        await Assert.That(metadata.Element(xmlNamespace + "license")!.Value).IsEqualTo("Apache-2.0");
        await Assert.That(metadata.Element(xmlNamespace + "license")!.Attribute("type")!.Value).IsEqualTo("expression");
        await Assert.That(metadata.Element(xmlNamespace + "requireLicenseAcceptance")!.Value).IsEqualTo("true");
    }

    [Test]
    public async Task Consumer_Package_Contains_Both_Prefixed_Assemblies()
    {
        using var archive = ZipFile.OpenRead(ConsumerPackage);
        foreach (var framework in new[] { "net8.0", "net10.0" })
            await Assert.That(archive.GetEntry($"lib/{framework}/Basilisque.Consumer.Framework.dll")).IsNotNull();
    }
}