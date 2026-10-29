namespace FileProcessor.Models;
public sealed record ProcessingReport(
int TotalFilesProcessed,
long TotalProcessingTimeMs,
IReadOnlyList<ProcessingRecord> Files);
