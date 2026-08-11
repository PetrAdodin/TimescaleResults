namespace TimescaleResults.Api.Csv;

public sealed record ValidatedCsvRow(
    DateTime Date,
    decimal ExecutionTime,
    decimal Value);