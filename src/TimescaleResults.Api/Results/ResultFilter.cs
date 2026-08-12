namespace TimescaleResults.Api.Results;

public sealed record ResultFilter(
    string? FileName,
    DateTimeOffset? MinDateFrom,
    DateTimeOffset? MinDateTo,
    decimal? AverageValueFrom,
    decimal? AverageValueTo,
    decimal? AverageExecutionTimeFrom,
    decimal? AverageExecutionTimeTo);