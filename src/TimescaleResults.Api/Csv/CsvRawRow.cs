namespace TimescaleResults.Api.Csv;

public sealed record CsvRawRow(
    int RowNumber,
    string Date,
    string ExecutionTime,
    string Value);