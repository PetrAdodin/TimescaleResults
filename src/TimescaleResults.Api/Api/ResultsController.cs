using Microsoft.AspNetCore.Mvc;
using TimescaleResults.Api.Results;
using TimescaleResults.Api.Services;

namespace TimescaleResults.Api.Api;

[ApiController]
[Route("api/results")]
public sealed class ResultsController(
    ResultQueryService resultQueryService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ResultDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<ResultDto>>> Get(
        [FromQuery] ResultFilter filter,
        CancellationToken cancellationToken)
    {
        var results = await resultQueryService.GetAsync(
            filter,
            cancellationToken);

        return Ok(results);
    }
}