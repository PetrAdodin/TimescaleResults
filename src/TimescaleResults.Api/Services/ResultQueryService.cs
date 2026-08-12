using Microsoft.EntityFrameworkCore;
using TimescaleResults.Api.Data;
using TimescaleResults.Api.Results;

namespace TimescaleResults.Api.Services;

public sealed class ResultQueryService(
    AppDbContext dbContext)
{
    public async Task<IReadOnlyList<ResultDto>> GetAsync(
        ResultFilter filter,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        ValidateFilter(filter);

        var query = dbContext.Results
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.FileName))
        {
            query = query.Where(
                result => result.FileName == filter.FileName);
        }

        if (filter.MinDateFrom is not null)
        {
            var minDateFrom = filter.MinDateFrom.Value.UtcDateTime;

            query = query.Where(
                result => result.MinDate >= minDateFrom);
        }

        if (filter.MinDateTo is not null)
        {
            var minDateTo = filter.MinDateTo.Value.UtcDateTime;

            query = query.Where(
                result => result.MinDate <= minDateTo);
        }

        if (filter.AverageValueFrom is not null)
        {
            query = query.Where(
                result =>
                    result.AverageValue >= filter.AverageValueFrom.Value);
        }

        if (filter.AverageValueTo is not null)
        {
            query = query.Where(
                result =>
                    result.AverageValue <= filter.AverageValueTo.Value);
        }

        if (filter.AverageExecutionTimeFrom is not null)
        {
            query = query.Where(
                result =>
                    result.AverageExecutionTime >=
                    filter.AverageExecutionTimeFrom.Value);
        }

        if (filter.AverageExecutionTimeTo is not null)
        {
            query = query.Where(
                result =>
                    result.AverageExecutionTime <=
                    filter.AverageExecutionTimeTo.Value);
        }

        return await query
            .Select(result => new ResultDto(
                result.FileName,
                result.DateRangeSeconds,
                result.MinDate,
                result.AverageExecutionTime,
                result.AverageValue,
                result.MedianValue,
                result.MaxValue,
                result.MinValue))
            .ToListAsync(cancellationToken);
    }

    private static void ValidateFilter(ResultFilter filter)
    {
        if (filter.MinDateFrom > filter.MinDateTo)
        {
            throw new ResultFilterValidationException(
                "MinDateFrom cannot be later than MinDateTo.");
        }

        if (filter.AverageValueFrom > filter.AverageValueTo)
        {
            throw new ResultFilterValidationException(
                "AverageValueFrom cannot be greater than AverageValueTo.");
        }

        if (filter.AverageExecutionTimeFrom >
            filter.AverageExecutionTimeTo)
        {
            throw new ResultFilterValidationException(
                "AverageExecutionTimeFrom cannot be greater than AverageExecutionTimeTo.");
        }
    }
}