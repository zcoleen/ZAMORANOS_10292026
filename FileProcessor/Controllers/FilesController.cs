using FileProcessor.Models;
using FileProcessor.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileProcessor.Controllers
{
    [ApiController]
    [Route("api/files")]
    public sealed class FilesController : ControllerBase
    {
        private readonly CsvProcessingService _csvProcessingService;
        private readonly ILogger<FilesController> _logger;
        private readonly FileTrackingService _fileTrackingService;
        public FilesController(
            CsvProcessingService csvProcessingService,
            ILogger<FilesController> logger,
            FileTrackingService fileTrackingService)
        {
            _csvProcessingService = csvProcessingService;
            _logger = logger;
            _fileTrackingService = fileTrackingService;
        }
        [HttpPost]
        [RequestSizeLimit(1_048_576)]
        public async Task<IActionResult> Upload(
            IFormFile? file,
            CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length == 0)
            {
                return BadRequest(new { error = "A non-empty CSV file is required." });
            }
            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".csv", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Only .csv files are accepted." });
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var result = await _csvProcessingService.ProcessAsync(
                    stream,
                    Path.GetFileName(file.FileName),
                    cancellationToken);

                _fileTrackingService.Add(new ProcessingRecord(
                result.FileName,
                result.RowsProcessed,
                result.RowsSkipped,
                result.ProcessingTimeMs,
                DateTimeOffset.UtcNow));

                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "CSV processing failed for {FileName}.", file.FileName);
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("report")]
        public ActionResult<ProcessingReport> GetReport()
        {
            return Ok(_fileTrackingService.GetReport());
        }
    }

}
