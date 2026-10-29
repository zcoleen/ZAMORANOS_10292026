namespace FileProcessor.Models;

public sealed record ProcessFileResponse(
    string FileName,
    int RowsProcessed,
    int RowsSkipped,
    long ProcessingTimeMs);
