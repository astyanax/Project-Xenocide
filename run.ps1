$project = Join-Path $PSScriptRoot "src\Xenocide.MonoGame\Xenocide.MonoGame.csproj"

Write-Host "Building..." -ForegroundColor Cyan
$build = dotnet build $project --nologo -v q 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed:" -ForegroundColor Red
    Write-Host $build
    exit $LASTEXITCODE
}
Write-Host "Build succeeded." -ForegroundColor Green

Write-Host "Running..." -ForegroundColor Cyan
Push-Location (Split-Path $project -Parent)
try {
    dotnet run --nologo
}
finally {
    Pop-Location
}
