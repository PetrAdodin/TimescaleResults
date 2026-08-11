using Microsoft.AspNetCore.Mvc;
using TimescaleResults.Api.Services;

namespace TimescaleResults.Api.Controllers;

[ApiController]
[Route("api/files")]
public sealed class FilesController(
    CsvUploadService csvUploadService) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(
            file.FileName.Replace('\\', '/'));

        if (string.IsNullOrWhiteSpace(fileName))
        {
            ModelState.AddModelError(
                nameof(file),
                "File name is required.");

            return ValidationProblem(ModelState);
        }

        await using var stream = file.OpenReadStream();

        await csvUploadService.UploadAsync(
            fileName,
            stream,
            cancellationToken);

        return NoContent();
    }
}