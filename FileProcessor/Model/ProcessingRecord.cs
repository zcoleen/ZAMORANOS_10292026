namespace FileProcessor.Models;

public sealed record ProcessingRecord(
 string FileName,
 int RowsProcessed,
 int RowsSkipped,
 long ProcessingTimeMs,
 DateTimeOffset ProcessedAtUtc);

