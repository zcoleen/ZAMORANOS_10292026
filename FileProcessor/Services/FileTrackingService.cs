using System.Collections.Concurrent;
using FileProcessor.Models;
namespace FileProcessor.Services;

public sealed class FileTrackingService
{
    private readonly ConcurrentQueue<ProcessingRecord> _records = new();
    private readonly ILogger<FileTrackingService> _logger;
    public FileTrackingService(ILogger<FileTrackingService> logger)
    {
        _logger = logger;
    }
    public void Add(ProcessingRecord record)
    {
        _records.Enqueue(record);
        _logger.LogInformation(
        "Tracked {FileName} processed at {ProcessedAtUtc}. Total tracked files: {Count}.",
        record.FileName,
        record.ProcessedAtUtc,
        _records.Count);
    }

    public ProcessingReport GetReport()
    {
        var files = _records.ToArray();
        var totalTime = files.Sum(file => file.ProcessingTimeMs);
        return new ProcessingReport(files.Length, totalTime, files);
    }
}
