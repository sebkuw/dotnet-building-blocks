param(
    [string] $RepositoryRoot = "."
)

$ErrorActionPreference = "Stop"

$allowedLicenses = @(
    "MIT",
    "Apache-2.0",
    "BSD-2-Clause",
    "BSD-3-Clause",
    "ISC"
)

# This pinned legacy test-only package predates SPDX metadata. Keep the package,
# version, and upstream license URL explicit so a dependency update is re-reviewed.
$legacyLicenseUrls = @{
    "xunit.abstractions/2.0.3" = "https://raw.githubusercontent.com/xunit/xunit/master/license.txt"
}

$assetsFiles = Get-ChildItem -Path $RepositoryRoot -Filter "project.assets.json" -Recurse |
    Where-Object { $_.FullName -match "[\\/]obj[\\/]project\.assets\.json$" }

if ($assetsFiles.Count -eq 0) {
    throw "No project.assets.json files were found. Run dotnet restore first."
}

$packages = @{}
foreach ($assetsFile in $assetsFiles) {
    $assets = Get-Content -Raw -LiteralPath $assetsFile.FullName | ConvertFrom-Json
    $packageRoot = $assets.packageFolders.PSObject.Properties.Name | Select-Object -First 1

    foreach ($library in $assets.libraries.PSObject.Properties) {
        if ($library.Value.type -ne "package") {
            continue
        }

        $parts = $library.Name -split "/", 2
        $id = $parts[0]
        $version = $parts[1]
        $key = "$id/$version"
        $packages[$key] = Join-Path $packageRoot (Join-Path $id.ToLowerInvariant() $version)
    }
}

$violations = @()
foreach ($package in $packages.GetEnumerator() | Sort-Object Name) {
    $nuspec = Get-ChildItem -Path $package.Value -Filter "*.nuspec" | Select-Object -First 1
    if ($null -eq $nuspec) {
        $violations += "$($package.Name): missing nuspec"
        continue
    }

    [xml] $metadata = Get-Content -Raw -LiteralPath $nuspec.FullName
    $licenseNode = $metadata.package.metadata.license
    if ($null -eq $licenseNode -or $licenseNode.type -ne "expression") {
        $licenseUrl = [string] $metadata.package.metadata.licenseUrl
        if ($legacyLicenseUrls.ContainsKey($package.Name) -and $legacyLicenseUrls[$package.Name] -eq $licenseUrl) {
            Write-Host "$($package.Name): approved legacy Apache-2.0 license URL"
            continue
        }

        $violations += "$($package.Name): license is not a verifiable SPDX expression"
        continue
    }

    $expression = [string] $licenseNode.'#text'
    if ([string]::IsNullOrWhiteSpace($expression)) {
        $expression = [string] $licenseNode
    }

    $identifiers = [regex]::Matches($expression, "[A-Za-z0-9.+-]+") |
        ForEach-Object Value |
        Where-Object { $_ -notin @("AND", "OR", "WITH") }

    $unsupported = $identifiers | Where-Object { $_ -notin $allowedLicenses }
    if ($unsupported.Count -gt 0) {
        $violations += "$($package.Name): unsupported license expression '$expression'"
    }
}

if ($violations.Count -gt 0) {
    throw "Package license policy failed: $($violations -join '; ')."
}

Write-Host "Validated $($packages.Count) package licenses."
