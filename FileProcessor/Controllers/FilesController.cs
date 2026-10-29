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
        public FilesController(
            CsvProcessingService csvProcessingService,
            ILogger<FilesController> logger)
        {
            _csvProcessingService = csvProcessingService;
            _logger = logger;
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

                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "CSV processing failed for {FileName}.", file.FileName);
                return BadRequest(new { error = ex.Message });
            }
        }
  
    }

}
