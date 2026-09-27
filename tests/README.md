<!--
   Copyright 2026 Alexander Stärk

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->
# CommonFrameworkBuild Integration Tests

The tests consume the locally packed `Basilisque.CommonFrameworkBuild` NuGet package.
Fixtures reference only that package, not CommonBuild directly and not the producer
project. CommonBuild is restored from nuget.org using the producer's dependency range.
No sibling repository or locally built CommonBuild package is required.

## Coverage

- Framework defaults: assembly name, root namespace, package ID, authors, release
  notes, license expression, and license acceptance.
- All seven `BAS_CFB_Set_*` opt-outs, with preexisting values defined before package
  imports. A companion test confirms those values are overwritten without opt-outs.
- The coupling between assembly and package naming; the benchmark naming exception
  and its explicit opt-in; spaces in the project name; non-GitHub and empty URLs.
- Real inner builds on .NET 8 and .NET 10, plus shared properties in the outer build.
- Producer package imports and dependency declaration; actual consumer package
  metadata and both framework-specific assemblies.
- CommonBuild smoke tests: transitive dependency resolution without a direct
  reference, compiler/documentation settings, generated XML documentation, and the
  actual assembly version. The broader CommonBuild test suite is not duplicated.

The standard fixture uses fixed version inputs (`7.3.1`, CI by default) and disables
GitVersion. It tests the inherited version target, not GitVersion's own calculation.
The benchmark fixture intentionally has a space in its project name.

## Local Workflow

Run from the repository root with .NET 8 and .NET 10 runtimes and a .NET 10+ SDK:

```powershell
dotnet pack src/Basilisque.CommonFrameworkBuild.slnx -c Release
dotnet build tests/Basilisque.CommonFrameworkBuild.Tests.slnx -c Release
dotnet pack tests/Basilisque.CommonFrameworkBuild.Tests.slnx -c Release --no-build
dotnet test --solution tests/Basilisque.CommonFrameworkBuild.Tests.slnx -c Release --no-build
```

Build the full test solution and pack the fixtures before running individual tests
in the editor, because package tests inspect the real artifacts. Consumer package
selection uses the version captured in the fixture assembly, never the alphabetically
newest file. Temporary build output is under `tests/artifacts` and is ignored by Git.

For an exact producer version, use the same `-p:BAS_CFB_PackageVersionUnderTest=...`
on all test-solution commands, or use the CI verification script after packing:

```powershell
pwsh -File .github/scripts/Test-Integration.ps1 -PackagePath "<path-to-current-nupkg>" -BuildType CI
```

The selected package must be in `src/artifacts/package/release`, the local feed
configured by `tests/NuGet.config`. Paths in that file use forward slashes so restore
also works on Linux. The script uses a fresh NuGet cache for each invocation to avoid
reusing stale contents of an identically versioned package.

## GitHub Actions

CI and manual builds invoke the existing shared workflow's `verificationScript`
hook after packing the producer. Both .NET 8 and .NET 10 are explicitly installed.
The script reads the exact producer version from its nuspec, builds and packs
the fixtures, and runs the tests. A failure in any command stops the job before
tagging, release creation, or package pushes.

`runDotnetTest: false` disables only the earlier shared test step for `src`, not the
verification hook. No shared workflow changes are needed. The hook runs after Sonar
analysis and does not currently import test coverage into Sonar.

## Known Producer Warning

The producer currently reports `NU5128`: it declares a `netstandard2.0` dependency
group but ships no matching `lib`/`ref` assets. This predates the tests and is not
suppressed by this infrastructure.