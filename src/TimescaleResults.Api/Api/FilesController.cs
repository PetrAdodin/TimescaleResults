using Microsoft.AspNetCore.Mvc;
using TimescaleResults.Api.Services;
using TimescaleResults.Api.Values;

namespace TimescaleResults.Api.Api;

[ApiController]
[Route("api/files")]
public sealed class FilesController(
    CsvUploadService csvUploadService,
    ValueQueryService valueQueryService) : ControllerBase
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
            ModelState.AddModelError(nameof(file), "File name is required.");
            return ValidationProblem(ModelState);
        }

        await using var stream = file.OpenReadStream();

        await csvUploadService.UploadAsync(
            fileName,
            stream,
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{fileName}/values")]
    [ProducesResponseType<IReadOnlyList<ValueDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ValueDto>>> GetLatestValues(
        string fileName,
        CancellationToken cancellationToken)
    {
        var values = await valueQueryService.GetLatestAsync(
            fileName,
            cancellationToken);

        return Ok(values);
    }
}