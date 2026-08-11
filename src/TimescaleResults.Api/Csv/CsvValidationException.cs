namespace TimescaleResults.Api.Csv;

public sealed class CsvValidationException : Exception
{
    public CsvValidationException(
        string message,
        int? rowNumber = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        RowNumber = rowNumber;
    }

    public int? RowNumber { get; }
}