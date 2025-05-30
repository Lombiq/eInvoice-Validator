# Path to the folder containing your .md benchmark result files.
$benchmarkFolder = "$PSScriptRoot\BenchmarkResults"

# Ensure the benchmark folder exists.
if (-not (Test-Path $benchmarkFolder))
{
    Write-Error "Benchmark results folder '$benchmarkFolder' does not exist."
    exit 1
}

# List to collect parsed benchmark results.
$results = @()

# Get all .md files.
Get-ChildItem -Path $benchmarkFolder -Filter *.md | ForEach-Object {
    $file = $PSItem
    $content = Get-Content $file.FullName

    # Extract metadata.
    $timestamp = ($content | Where-Object { $PSItem -match '\*\*Run Timestamp:\*\*' }) -replace '.*\*\*Run Timestamp:\*\* ', ''

    $batchSizeLine = $content | Where-Object { $PSItem -match '\*\*Batch Size:\*\*' }
    $batchSize = [int]($batchSizeLine -replace '.*\*\*Batch Size:\*\* ', '')

    $batchCountLine = $content | Where-Object { $PSItem -match '\*\*Batch Count:\*\*' }
    $batchCount = [int]($batchCountLine -replace '.*\*\*Batch Count:\*\* ', '')

    $minDelay = ($content | Where-Object { $PSItem -match '\*\*Minimum Delay Between Batches:\*\*' }) -replace '.*\*\*Minimum Delay Between Batches:\*\* ', ''

    # Get only rows that look like actual batch results (start with | <number>).
    $batchLines = $content | Where-Object { $PSItem -match '^\|\s*\d+\s*\|' }

    foreach ($line in $batchLines)
    {
        $columns = $line -split '\|\s*' | Where-Object { $PSItem -ne '' }
        $results += [PSCustomObject]@{
            File = $file.Name
            Timestamp = $timestamp
            BatchSize = $batchSize
            BatchCount = $batchCount
            MinDelayBetweenBatches = $minDelay
            Batch = [int]$columns[0]
            SchematronInnerMs = [double]$columns[1]
            TotalMs = [double]$columns[2]
        }
    }
}

# Output all results to console.
$results | Format-Table -AutoSize

# Export to CSV.
$results | Export-Csv -Path "$benchmarkFolder\BenchmarkResults.csv" -NoTypeInformation
