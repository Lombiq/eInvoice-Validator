param(
    [string]$ExecutablePath = "dotnet run", # Or "$PSScriptRoot\bin\Release\net8.0\EInvoiceValidator.Benchmark.exe"
    [int]$Runs = 5,
    [int]$DelayBetweenRunsSeconds = 2,
    [string]$LogFolder = ".\BenchmarkRunLogs"
)

# Ensure output folder exists.
$timestampFolder = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$runOutputPath = Join-Path -Path $LogFolder -ChildPath $timestampFolder
New-Item -ItemType Directory -Path $runOutputPath -Force | Out-Null

Write-Host "`n📊 Starting $Runs benchmark runs..."

for ($i = 1; $i -le $Runs; $i++) {
    $runTime = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logFile = Join-Path -Path $runOutputPath -ChildPath "run_$i.log"

    Write-Host "▶️ Run $i at $runTime"

    # Run the benchmark and capture output.
    & $ExecutablePath | Tee-Object -FilePath $logFile

    # Optional delay between runs.
    if ($i -lt $Runs -and $DelayBetweenRunsSeconds -gt 0) {
        Write-Host "⏳ Waiting $DelayBetweenRunsSeconds seconds before next run..."
        Start-Sleep -Seconds $DelayBetweenRunsSeconds
    }
}

Write-Host "`n✅ All $Runs runs completed. Logs saved to: $runOutputPath"

# Run Parse-BenchmarkResults.ps1 to parse the results.
& "$PSScriptRoot\Parse-BenchmarkResults.ps1"
