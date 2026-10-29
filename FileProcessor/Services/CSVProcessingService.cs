using FileProcessor.Models;
using System.Globalization;

namespace FileProcessor.Services
{
    public sealed class CsvProcessingService
    {
        private readonly ILogger<CsvProcessingService> _logger;
        public CsvProcessingService(ILogger<CsvProcessingService> logger)
        {
            _logger = logger;
        }
        public async Task<ProcessFileResponse> ProcessAsync(
            Stream csvStream,
            string fileName,
            CancellationToken cancellationToken)
        {
            var started = System.Diagnostics.Stopwatch.StartNew();
            using var reader = new StreamReader(csvStream);
            var headerLine = await reader.ReadLineAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(headerLine))
            {
                throw new InvalidDataException("The CSV file is empty.");
            }

            var headers = SplitCsvLine(headerLine);
            var columnIndex = Array.FindIndex(
                headers, header => string.Equals(header.Trim(), "FullName", StringComparison.OrdinalIgnoreCase));

            if (columnIndex < 0)
            {
                throw new InvalidDataException($"Invalid File with no column");
            }

            var rowsProcessed = 0;
            var rowsSkipped = 0;

            while (true)
            {
                var line = await reader.ReadLineAsync(cancellationToken);
                if (line is null)
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                var cells = SplitCsvLine(line);
                if (columnIndex >= cells.Length)
                {
                    rowsSkipped++;
                    continue;
                }
                rowsProcessed++;
            }

            if (rowsProcessed == 0)
            {
                throw new InvalidDataException($"No rows was processed");
            }

            started.Stop();

            _logger.LogInformation(
                "Processed {FileName}: rows {RowsProcessed}, skipped {RowsSkipped}, elapsed  { ElapsedMs} ms.",
                fileName,
            rowsProcessed,
            rowsSkipped,
            started.ElapsedMilliseconds);


            return new ProcessFileResponse(
                fileName,
                rowsProcessed,
                rowsSkipped,
                started.ElapsedMilliseconds);
        }
        private static string[] SplitCsvLine(string line) => line.Split(',');
    }
}
