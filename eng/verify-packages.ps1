param(
    [string] $PackageDirectory = "artifacts/packages"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$packages = Get-ChildItem -Path $PackageDirectory -Filter "NetDevs.*.nupkg" |
    Where-Object { $_.Name -notlike "*.symbols.nupkg" }

if ($packages.Count -ne 6) {
    throw "Expected 6 NetDevs packages, found $($packages.Count)."
}

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

Write-Host "Validated $($packages.Count) NuGet packages."
