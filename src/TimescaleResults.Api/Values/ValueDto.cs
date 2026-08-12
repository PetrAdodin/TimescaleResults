namespace TimescaleResults.Api.Values;

public sealed record ValueDto(
    DateTime Date,
    decimal ExecutionTime,
    decimal Value);