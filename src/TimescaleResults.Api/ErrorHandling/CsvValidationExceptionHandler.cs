using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TimescaleResults.Api.Csv;

namespace TimescaleResults.Api.ErrorHandling;

public sealed class CsvValidationExceptionHandler
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not CsvValidationException csvException)
        {
            return false;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Invalid CSV file",
            Detail = csvException.Message
        };

        if (csvException.RowNumber is not null)
        {
            problemDetails.Extensions["rowNumber"] =
                csvException.RowNumber;
        }

        httpContext.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(
            problemDetails,
            cancellationToken);

        return true;
    }
}