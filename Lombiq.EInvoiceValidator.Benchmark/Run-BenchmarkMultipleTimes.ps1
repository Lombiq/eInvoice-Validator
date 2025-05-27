param(
    [int]$Runs = 5,
    [int]$DelayBetweenRunsSeconds = 2,
    [string]$LogFolder = '.\BenchmarkRunLogs'
)

# Ensure output folder exists.
$timestampFolder = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$runOutputPath = Join-Path -Path $LogFolder -ChildPath $timestampFolder
New-Item -ItemType Directory -Path $runOutputPath -Force | Out-Null

Write-Output "`n Starting $Runs benchmark runs..."

for ($i = 1; $i -le $Runs; $i++) {
    $runTime = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
    $logFile = Join-Path -Path $runOutputPath -ChildPath "run_$i.log"

    Write-Output "Run $i at $runTime"

    # Run the benchmark and capture output.
    dotnet run | Tee-Object -FilePath $logFile

    # Optional delay between runs.
    if ($i -lt $Runs -and $DelayBetweenRunsSeconds -gt 0) {
        Write-Output "⏳ Waiting $DelayBetweenRunsSeconds seconds before next run..."
        Start-Sleep -Seconds $DelayBetweenRunsSeconds
    }
}

Write-Output "`n All $Runs runs completed. Logs saved to: $runOutputPath"

# Run Parse-BenchmarkResults.ps1 to parse the results.
& "$PSScriptRoot\Parse-BenchmarkResults.ps1"
