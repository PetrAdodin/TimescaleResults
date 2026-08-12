namespace TimescaleResults.Api.Results;

public sealed record ResultDto(
    string FileName,
    decimal DateRangeSeconds,
    DateTime MinDate,
    decimal AverageExecutionTime,
    decimal AverageValue,
    decimal MedianValue,
    decimal MaxValue,
    decimal MinValue);