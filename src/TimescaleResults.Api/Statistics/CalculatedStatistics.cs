namespace TimescaleResults.Api.Statistics;

public sealed record CalculatedStatistics(
    decimal DateRangeSeconds,
    DateTime MinDate,
    decimal AverageExecutionTime,
    decimal AverageValue,
    decimal MedianValue,
    decimal MaxValue,
    decimal MinValue);