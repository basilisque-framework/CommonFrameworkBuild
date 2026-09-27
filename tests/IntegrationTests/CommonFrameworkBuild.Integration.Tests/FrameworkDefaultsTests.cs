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

namespace CommonFrameworkBuild.Integration.Tests;

public class FrameworkDefaultsTests
{
    [Test]
    public async Task Assembly_Name_Has_Framework_Prefix()
    {
        await Assert.That(typeof(FixtureInfo).Assembly.GetName().Name).IsEqualTo("Basilisque.Consumer.Framework");
    }

    [Test]
    [Arguments("RootNamespace", "Basilisque.Consumer.Framework")]
    [Arguments("PackageId", "Basilisque.Consumer.Framework")]
    [Arguments("Authors", "Alexander St\u00e4rk")]
    [Arguments("PackageReleaseNotes", "https://github.com/basilisque-framework/Consumer.Framework/releases")]
    [Arguments("PackageLicenseExpression", "Apache-2.0")]
    [Arguments("PackageRequireLicenseAcceptance", "true")]
    public async Task Built_Assembly_Captures_Framework_Default(string property, string expected)
    {
        await Assert.That(Fixture.Property(property)).IsEqualTo(expected);
    }

    [Test]
    public async Task Outer_Build_Receives_Shared_Framework_Defaults()
    {
        var properties = await Fixture.Evaluate(outerBuild: true);
        await Assert.That(properties["IsCrossTargetingBuild"]).IsEqualTo("true");
        await Assert.That(properties["AssemblyName"]).IsEqualTo("Basilisque.Consumer.Framework");
        await Assert.That(properties["PackageId"]).IsEqualTo("Basilisque.Consumer.Framework");
        await Assert.That(properties["Authors"]).IsEqualTo("Alexander St\u00e4rk");
        await Assert.That(properties["PackageLicenseExpression"]).IsEqualTo("Apache-2.0");
        await Assert.That(properties["PackageRequireLicenseAcceptance"]).IsEqualTo("true");
    }

    [Test]
    [Arguments("RootNamespace", "Custom.Namespace")]
    [Arguments("AssemblyName", "Custom.Assembly")]
    [Arguments("PackageId", "Custom.Package")]
    [Arguments("Authors", "Custom.Author")]
    [Arguments("PackageReleaseNotes", "Custom release notes")]
    [Arguments("PackageLicenseExpression", "MIT")]
    [Arguments("PackageRequireLicenseAcceptance", "false")]
    public async Task Opt_Out_Preserves_Preexisting_Value(string property, string expected)
    {
        var properties = await Fixture.Evaluate(properties: new[] { "-p:FixtureCustomValues=true", $"-p:BAS_CFB_Set_{property}=false" });
        await Assert.That(properties[property]).IsEqualTo(expected);
    }

    [Test]
    public async Task Defaults_Override_Custom_Values_Without_Opt_Out()
    {
        var properties = await Fixture.Evaluate(properties: new[] { "-p:FixtureCustomValues=true" });
        foreach (var property in new[] { "RootNamespace", "Authors", "PackageId", "PackageReleaseNotes", "PackageLicenseExpression", "PackageRequireLicenseAcceptance" })
            await Assert.That(properties[property]).IsEqualTo(Fixture.Property(property));
        await Assert.That(properties["AssemblyName"]).IsEqualTo("Basilisque.Consumer.Framework");
    }

    [Test]
    public async Task Disabling_Assembly_Renaming_Also_Disables_Default_PackageId_Renaming()
    {
        var properties = await Fixture.Evaluate(properties: new[] { "-p:FixtureCustomValues=true", "-p:BAS_CFB_Set_AssemblyName=false" });
        await Assert.That(properties["AssemblyName"]).IsEqualTo("Custom.Assembly");
        await Assert.That(properties["PackageId"]).IsEqualTo("Custom.Package");
    }

    [Test]
    [Arguments("https://example.org/project")]
    [Arguments("")]
    public async Task Non_GitHub_Url_Does_Not_Replace_Release_Notes(string projectUrl)
    {
        var properties = await Fixture.Evaluate(properties: new[] { "-p:FixtureCustomValues=true", $"-p:PackageProjectUrl={projectUrl}" });
        await Assert.That(properties["PackageReleaseNotes"]).IsEqualTo("Custom release notes");
    }
}
