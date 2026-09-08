param(
    [string] $ResultsDirectory = "artifacts/TestResults",
    [double] $MinimumLineRate = 0.90,
    [double] $MinimumBranchRate = 0.80
)

$ErrorActionPreference = "Stop"

$requiredPackages = @(
    "sebkuw.Cqrs.Abstractions",
    "sebkuw.Cqrs",
    "sebkuw.Domain.Abstractions",
    "sebkuw.EntityFrameworkCore.Auditing",
    "sebkuw.ExceptionProcessor",
    "sebkuw.QueryableProcessor"
)

$reports = Get-ChildItem -Path $ResultsDirectory -Filter "coverage.cobertura.xml" -Recurse
if ($reports.Count -eq 0) {
    throw "No Cobertura coverage reports were found under '$ResultsDirectory'."
}

$bestRates = @{}
foreach ($report in $reports) {
    [xml] $coverage = Get-Content -Raw -LiteralPath $report.FullName
    foreach ($package in $coverage.coverage.packages.package) {
        $name = [string] $package.name
        if ($name -notin $requiredPackages) {
            continue
        }

        $lineRate = [double]::Parse([string] $package.'line-rate', [Globalization.CultureInfo]::InvariantCulture)
        $branchRate = [double]::Parse([string] $package.'branch-rate', [Globalization.CultureInfo]::InvariantCulture)

        if (-not $bestRates.ContainsKey($name) -or $lineRate -gt $bestRates[$name].LineRate) {
            $bestRates[$name] = [pscustomobject]@{
                LineRate = $lineRate
                BranchRate = $branchRate
            }
        }
    }
}

$failures = @()
foreach ($name in $requiredPackages) {
    if (-not $bestRates.ContainsKey($name)) {
        $failures += "${name}: no coverage data"
        continue
    }

    $rates = $bestRates[$name]
    Write-Host ("{0}: lines {1:P1}, branches {2:P1}" -f $name, $rates.LineRate, $rates.BranchRate)

    if ($rates.LineRate -lt $MinimumLineRate -or $rates.BranchRate -lt $MinimumBranchRate) {
        $failures += "${name}: lines $($rates.LineRate.ToString('P1')), branches $($rates.BranchRate.ToString('P1'))"
    }
}

if ($failures.Count -gt 0) {
    throw "Coverage requirements were not met: $($failures -join '; ')."
}
