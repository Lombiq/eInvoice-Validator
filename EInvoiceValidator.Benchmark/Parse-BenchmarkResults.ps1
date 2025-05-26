# Path to the folder containing your .md benchmark result files.
$benchmarkFolder = "$PSScriptRoot\BenchmarkResults"

# List to collect parsed benchmark results.
$results = @()

# Get all .md files.
Get-ChildItem -Path $benchmarkFolder -Filter *.md | ForEach-Object {
    $file = $_
    $content = Get-Content $file.FullName

    # Extract metadata.
    $timestamp = ($content | Where-Object { $_ -match '\*\*Run Timestamp:\*\*' }) -replace '.*\*\*Run Timestamp:\*\* ', ''

    $batchSizeLine = $content | Where-Object { $_ -match '\*\*Batch Size:\*\*' }
    $batchSize = [int]($batchSizeLine -replace '.*\*\*Batch Size:\*\* ', '')

    $batchCountLine = $content | Where-Object { $_ -match '\*\*Batch Count:\*\*' }
    $batchCount = [int]($batchCountLine -replace '.*\*\*Batch Count:\*\* ', '')

    $minDelay = ($content | Where-Object { $_ -match '\*\*Minimum Delay Between Batches:\*\*' }) -replace '.*\*\*Minimum Delay Between Batches:\*\* ', ''

    # Get only rows that look like actual batch results (start with | <number>).
    $batchLines = $content | Where-Object { $_ -match '^\|\s*\d+\s*\|' }

    foreach ($line in $batchLines) {
        $columns = $line -split '\|\s*' | Where-Object { $_ -ne '' }
        $results += [PSCustomObject]@{
            File                  = $file.Name
            Timestamp             = $timestamp
            BatchSize             = $batchSize
            BatchCount            = $batchCount
            MinDelayBetweenBatches = $minDelay
            Batch                 = [int]$columns[0]
            SchemaInnerMs         = [double]$columns[1]
            SchemaFullMs          = [double]$columns[2]
            SchematronInnerMs     = [double]$columns[3]
            SchematronFullMs      = [double]$columns[4]
            TotalMs               = [double]$columns[5]
        }
    }
}

# Output all results to console.
$results | Format-Table -AutoSize

# Export to CSV.
$results | Export-Csv -Path "$benchmarkFolder\BenchmarkResults.csv" -NoTypeInformation
