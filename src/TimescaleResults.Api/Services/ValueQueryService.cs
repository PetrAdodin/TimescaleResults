using Microsoft.EntityFrameworkCore;
using TimescaleResults.Api.Data;
using TimescaleResults.Api.Values;

namespace TimescaleResults.Api.Services;

public sealed class ValueQueryService(
    AppDbContext dbContext)
{
    public async Task<IReadOnlyList<ValueDto>> GetLatestAsync(
        string fileName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        return await dbContext.Values
            .AsNoTracking()
            .Where(value => value.Result.FileName == fileName)
            .OrderByDescending(value => value.Date)
            .ThenByDescending(value => value.Id)
            .Take(10)
            .Select(value => new ValueDto(
                value.Date,
                value.ExecutionTime,
                value.Value))
            .ToListAsync(cancellationToken);
    }
}