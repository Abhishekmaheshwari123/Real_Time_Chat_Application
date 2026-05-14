using ConnectHub.Media.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConnectHub.Media.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MediaController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;

    public MediaController(IBlobStorageService blobStorageService)
    {
        _blobStorageService = blobStorageService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var stream = file.OpenReadStream();
        var fileUrl = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

        return Ok(new { url = fileUrl });
    }
}
