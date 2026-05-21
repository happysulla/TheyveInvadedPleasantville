param(
    [ValidateSet("all", "model", "view", "networking", "tests", "config", "interfaces")]
    [string]$Area = "all",

    [string]$Root = "."
)

$projectRoot = Join-Path $Root "PleasantvilleGame"

if (-not (Test-Path -LiteralPath $projectRoot)) {
    Write-Error "Could not find PleasantvilleGame under $Root"
    exit 1
}

$paths = switch ($Area) {
    "model" { @("Model") }
    "view" { @("View", "MainWindow.xaml", "MainWindow.xaml.cs") }
    "networking" { @("Networking", "Protos") }
    "tests" { @("UnitTests") }
    "config" { @("Config", "PleasantvilleGame.csproj") }
    "interfaces" { @("Interfaces") }
    default { @("Model", "View", "Networking", "Interfaces", "UnitTests", "Config") }
}

foreach ($path in $paths) {
    $fullPath = Join-Path $projectRoot $path

    if (Test-Path -LiteralPath $fullPath -PathType Container) {
        Get-ChildItem -LiteralPath $fullPath -Recurse -File |
            ForEach-Object { $_.FullName }
        continue
    }

    if (Test-Path -LiteralPath $fullPath -PathType Leaf) {
        $fullPath
    }
}

