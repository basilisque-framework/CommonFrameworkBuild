# Copyright 2026 Alexander Stärk
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

param(
    [string] $PackagePath,
    [string] $BuildType = $env:BAS_CB_BUILD_TYPE
)

$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $false

function Invoke-Dotnet {
    param([string[]] $Arguments)

    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE."
    }
}

$repositoryRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
$previousPackagesPath = $env:NUGET_PACKAGES
Push-Location $repositoryRoot
try {
    if ([string]::IsNullOrWhiteSpace($BuildType)) {
        $BuildType = 'CI'
    }

    if ([string]::IsNullOrWhiteSpace($PackagePath)) {
        $artifactsPath = $env:BAS_CB_ARTIFACTS_PATH
        if ([string]::IsNullOrWhiteSpace($artifactsPath)) {
            $artifactsPath = 'src/artifacts'
        }
        $packages = @(Get-ChildItem (Join-Path $artifactsPath 'package/release') -Filter 'Basilisque.CommonFrameworkBuild.*.nupkg' -File |
            Where-Object { $_.Name -notlike '*.symbols.nupkg' })
        if ($packages.Count -ne 1) {
            throw 'Expected exactly one freshly built CommonFrameworkBuild package. For local runs with older artifacts, pass -PackagePath explicitly.'
        }
        $PackagePath = $packages[0].FullName
    }

    $archive = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path $PackagePath).Path)
    try {
        $entry = $archive.GetEntry('Basilisque.CommonFrameworkBuild.nuspec')
        if ($null -eq $entry) {
            throw 'The selected package does not contain Basilisque.CommonFrameworkBuild.nuspec.'
        }
        $reader = [System.IO.StreamReader]::new($entry.Open())
        try {
            $manifest = [xml]$reader.ReadToEnd()
        }
        finally {
            $reader.Dispose()
        }
        $packageVersion = [string]$manifest.package.metadata.version
        if ($manifest.package.metadata.id -ne 'Basilisque.CommonFrameworkBuild' -or [string]::IsNullOrWhiteSpace($packageVersion)) {
            throw 'The selected package must be Basilisque.CommonFrameworkBuild and have a version.'
        }
    }
    finally {
        $archive.Dispose()
    }

    $env:NUGET_PACKAGES = Join-Path $repositoryRoot "tests/artifacts/ci-nuget/$([guid]::NewGuid().ToString('N'))"
    $solution = Join-Path $repositoryRoot 'tests/Basilisque.CommonFrameworkBuild.Tests.slnx'
    $properties = @("-p:BAS_CFB_PackageVersionUnderTest=$packageVersion", "-p:BAS_CB_BuildType=$BuildType")
    Write-Host "Testing CommonFrameworkBuild package $packageVersion ($PackagePath)"

    Invoke-Dotnet -Arguments (@('build', $solution, '-c', 'Release', '--disable-build-servers') + $properties)
    Invoke-Dotnet -Arguments (@('pack', $solution, '-c', 'Release', '--no-build') + $properties)
    Invoke-Dotnet -Arguments (@('test', '--solution', $solution, '-c', 'Release', '--no-build') + $properties)
}
finally {
    $env:NUGET_PACKAGES = $previousPackagesPath
    Pop-Location
}