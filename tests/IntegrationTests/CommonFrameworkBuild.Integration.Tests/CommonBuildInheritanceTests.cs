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

using System.Text.Json;

namespace CommonFrameworkBuild.Integration.Tests;

public class CommonBuildInheritanceTests
{
    [Test]
    public async Task CommonBuild_Is_Resolved_Through_FrameworkBuild_Not_A_Direct_Reference()
    {
        using var assets = JsonDocument.Parse(await File.ReadAllTextAsync(Fixture.Property("FixtureAssetsPath")));
        var framework = $"net{Environment.Version.Major}.0";
        var directDependencies = assets.RootElement.GetProperty("project").GetProperty("frameworks")
            .GetProperty(framework).GetProperty("dependencies");
        await Assert.That(directDependencies.TryGetProperty("Basilisque.CommonFrameworkBuild", out _)).IsTrue();
        await Assert.That(directDependencies.TryGetProperty("Basilisque.CommonBuild", out _)).IsFalse();

        var libraries = assets.RootElement.GetProperty("targets").GetProperty(framework);
        var frameworkBuild = libraries.EnumerateObject().Single(library => library.Name.StartsWith("Basilisque.CommonFrameworkBuild/", StringComparison.Ordinal));
        await Assert.That(frameworkBuild.Value.GetProperty("dependencies").TryGetProperty("Basilisque.CommonBuild", out _)).IsTrue();
        await Assert.That(libraries.EnumerateObject().Any(library => library.Name.StartsWith("Basilisque.CommonBuild/", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task CommonBuild_Compiler_And_Documentation_Defaults_Are_Applied()
    {
        await Assert.That(Fixture.Property("Nullable")).IsEqualTo("enable");
        await Assert.That(Fixture.Property("ImplicitUsings")).IsEqualTo("disable");
        await Assert.That(Fixture.Property("GenerateDocumentationFile")).IsEqualTo("true");
        await Assert.That(File.Exists(Path.ChangeExtension(Fixture.Assembly.Location, ".xml"))).IsTrue();
    }

    [Test]
    public async Task CommonBuild_Version_Target_Changes_The_Actual_Assembly()
    {
        await Assert.That(Fixture.Assembly.GetName().Version).IsEqualTo(new Version(7, 3, 0, 0));
    }
}