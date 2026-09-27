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

namespace CommonFrameworkBuild.Integration.Tests;

public class FrameworkConventionsTests
{
    private static string BenchmarkProject => Path.GetFullPath(Path.Combine(
        Path.GetDirectoryName(Fixture.Property("FixtureProjectPath"))!,
        "../Consumer Framework.Benchmarks/Consumer Framework.Benchmarks.csproj"));

    [Test]
    public async Task Benchmark_Keeps_Unprefixed_Assembly_And_Package_Name()
    {
        var properties = await Fixture.Evaluate(BenchmarkProject);
        await Assert.That(properties["AssemblyName"]).IsEqualTo("Consumer Framework.Benchmarks");
        await Assert.That(properties["PackageId"]).IsEqualTo("Consumer Framework.Benchmarks");
    }

    [Test]
    public async Task RootNamespace_Replaces_Project_Name_Spaces()
    {
        var properties = await Fixture.Evaluate(BenchmarkProject);
        await Assert.That(properties["RootNamespace"]).IsEqualTo("Basilisque.Consumer_Framework.Benchmarks");
    }

    [Test]
    public async Task Benchmark_Can_Explicitly_Enable_Assembly_Prefix()
    {
        var properties = await Fixture.Evaluate(BenchmarkProject, properties: new[] { "-p:BAS_CFB_Set_AssemblyName=true" });
        await Assert.That(properties["AssemblyName"]).IsEqualTo("Basilisque.Consumer Framework.Benchmarks");
        await Assert.That(properties["PackageId"]).IsEqualTo("Basilisque.Consumer Framework.Benchmarks");
    }
}