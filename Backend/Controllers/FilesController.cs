using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public FilesController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Please select a file to upload.");
        }

        var allowedExtensions = new[]
        {
            ".txt",
            ".log",
            ".csv",
            ".jpg",
            ".jpeg",
            ".png"
        };

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(
                "Unsupported file type. Upload a log, CSV, image or text file.");
        }

        var uploadsPath = Path.Combine(
            _environment.ContentRootPath,
            "Uploads");

        Directory.CreateDirectory(uploadsPath);

        var safeFileName =
            $"{Guid.NewGuid()}{extension}";

        var filePath = Path.Combine(
            uploadsPath,
            safeFileName);

        await using var stream = new FileStream(
            filePath,
            FileMode.Create);

        await file.CopyToAsync(stream);

        return Ok(new
        {
            message = "File uploaded successfully.",
            originalFileName = file.FileName,
            storedFileName = safeFileName,
            fileSize = file.Length
        });
    }
}