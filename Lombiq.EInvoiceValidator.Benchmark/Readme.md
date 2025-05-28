# Lombiq eInvoice Validator Benchmark

## About

This a console application to benchmark the Lombiq eInvoice Validator library.

## How to use

Start the benchmark by running the Run-BenchmarkMultipleTimes.ps1 PowerShell script in the root of the project. This will run the benchmark multiple times to get more reliable results, and then it will generate a `CSV` file with the results in the _BenchmarkResults_ folder. You can also see the logs for each run in the _BenchmarkRunLogs_ folder.

You can set how many runs you want to perform and the delay between them by setting -Runs and -DelayBetweenRunsSeconds parameters, e.g. if you want 2 runs and 10 seconds between them:

``` powershell
    ./Run-BenchmarkMultipleTimes.ps1 -Runs 2 -DelayBetweenRunsSeconds 10
```

## Previous results

- **Run Timestamp:** 2025-05-28 10:03:38 UTC
- **Batch Size:** 200
- **Batch Count:** 10
- **Minimum Delay Between Batches:** 1000 ms

| Batch | Schematron Inner (ms) | Total (ms) |
|-------|-----------------------|------------|
| 1 | 168.04 | 548.49 |
| 2 | 172.86 | 743.295 |
| 3 | 163.705 | 559.12 |
| 4 | 162.58 | 951.085 |
| 5 | 152.595 | 820.25 |
| 6 | 160.035 | 984.295 |
| 7 | 158.76 | 604.045 |
| 8 | 151.955 | 443.965 |
| 9 | 150.815 | 816.115 |
| 10 | 160.11 | 758.16 |
| **AVG** | **160.146** | **722.882** |

Schematron Inner means the inner process of the Schematron validation. Total means the total time taken for the whole validation process, including the Schematron validation and Schema validation. The most time consuming part is calling the Schematron validation, because it runs in a separate Node.js process, so it has to serialize the XML, send it to the Node.js process, run the validation there, and then deserialize the result back to the C# code. Schema validation takes maximum 1-3 ms, so it's not included in the results.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
