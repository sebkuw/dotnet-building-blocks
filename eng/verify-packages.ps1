param(
    [string] $PackageDirectory = "artifacts/packages",
    [string] $ProjectDirectory = "src",
    [string] $ExpectedRepositoryUrl = "https://github.com/sebkuw/dotnet-building-blocks",
    [string] $ExpectedRepositoryCommit
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$expectedVersions = @{}
$projectFiles = Get-ChildItem -Path $ProjectDirectory -Filter "*.csproj" -Recurse
foreach ($projectFile in $projectFiles) {
    [xml] $project = Get-Content -Raw -LiteralPath $projectFile.FullName
    $packageId = [string] $project.Project.PropertyGroup.PackageId
    $version = [string] $project.Project.PropertyGroup.Version

    if ([string]::IsNullOrWhiteSpace($packageId)) {
        continue
    }

    if ($expectedVersions.ContainsKey($packageId)) {
        throw "Package ID '$packageId' is declared by more than one project."
    }

    if ([string]::IsNullOrWhiteSpace($version)) {
        throw "$($projectFile.FullName) does not declare a package version."
    }

    $expectedVersions[$packageId] = $version
}

if ($expectedVersions.Count -eq 0) {
    throw "No production package projects were found under '$ProjectDirectory'."
}

$expectedPackageIds = @($expectedVersions.Keys | Sort-Object)

$packages = Get-ChildItem -Path $PackageDirectory -Filter "sebkuw.*.nupkg" |
    Where-Object { $_.Name -notlike "*.symbols.nupkg" }

if ($packages.Count -ne $expectedPackageIds.Count) {
    throw "Expected $($expectedPackageIds.Count) sebkuw packages, found $($packages.Count)."
}

$actualPackageIds = @()
foreach ($package in $packages) {
    $archive = [IO.Compression.ZipFile]::OpenRead($package.FullName)
    try {
        $entries = $archive.Entries.FullName
        $packageId = $package.BaseName -replace "\.\d+\.\d+\.\d+.*$", ""
        $requiredEntries = @(
            "README.md",
            "CHANGELOG.md",
            "lib/net10.0/$packageId.dll",
            "lib/net10.0/$packageId.xml"
        )

        foreach ($entry in $requiredEntries) {
            if ($entry -notin $entries) {
                throw "$($package.Name) is missing '$entry'."
            }
        }

        $nuspecEntry = $archive.Entries | Where-Object { $_.FullName -eq "$packageId.nuspec" }
        if ($null -eq $nuspecEntry) {
            throw "$($package.Name) is missing '$packageId.nuspec'."
        }

        $stream = $nuspecEntry.Open()
        $reader = [IO.StreamReader]::new($stream)
        try {
            [xml] $nuspec = $reader.ReadToEnd()
            $metadata = $nuspec.package.metadata
            $metadataId = [string] $metadata.id
            $metadataVersion = [string] $metadata.version
            $repository = $metadata.repository

            if ($metadataId -ne $packageId) {
                throw "$($package.Name) declares package ID '$metadataId' instead of '$packageId'."
            }

            $actualPackageIds += $metadataId

            $expectedVersion = $expectedVersions[$metadataId]
            if ($metadataVersion -cne $expectedVersion) {
                throw "$($package.Name) declares version '$metadataVersion' instead of project version '$expectedVersion'."
            }

            if ($null -eq $repository -or
                [string] $repository.type -cne "git" -or
                ([string] $repository.url).TrimEnd("/") -cne $ExpectedRepositoryUrl.TrimEnd("/")) {
                throw "$($package.Name) must link to the source Git repository '$ExpectedRepositoryUrl'."
            }

            if (-not [string]::IsNullOrWhiteSpace($ExpectedRepositoryCommit) -and
                [string] $repository.commit -cne $ExpectedRepositoryCommit) {
                throw "$($package.Name) links to commit '$([string] $repository.commit)' instead of '$ExpectedRepositoryCommit'."
            }

            $license = $nuspec.package.metadata.license
            if ($license.type -ne "expression" -or [string] $license.'#text' -ne "MIT") {
                throw "$($package.Name) must declare the MIT license expression."
            }
        }
        finally {
            $reader.Dispose()
            $stream.Dispose()
        }
    }
    finally {
        $archive.Dispose()
    }
}

$missingPackageIds = $expectedPackageIds | Where-Object { $_ -notin $actualPackageIds }
$unexpectedPackageIds = $actualPackageIds | Where-Object { $_ -notin $expectedPackageIds }
if ($missingPackageIds.Count -gt 0 -or $unexpectedPackageIds.Count -gt 0) {
    throw "Package artifacts do not match the production projects. Missing: $($missingPackageIds -join ', '); unexpected: $($unexpectedPackageIds -join ', ')."
}

Write-Host "Validated $($packages.Count) NuGet packages."
