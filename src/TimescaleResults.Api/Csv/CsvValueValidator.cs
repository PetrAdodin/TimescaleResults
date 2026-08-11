using System.Globalization;

namespace TimescaleResults.Api.Csv;

public sealed class CsvValueValidator
{
    private static readonly DateTime MinAllowedDate =
        new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public IReadOnlyList<ValidatedCsvRow> Validate(
        IReadOnlyList<CsvRawRow> rows,
        DateTime currentUtc)
    {
        ArgumentNullException.ThrowIfNull(rows);

        if (currentUtc.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                "Current time must be specified in UTC.",
                nameof(currentUtc));
        }

        var validatedRows = new List<ValidatedCsvRow>(rows.Count);

        foreach (var row in rows)
        {
            validatedRows.Add(ValidateRow(row, currentUtc));
        }

        return validatedRows;
    }

    private static ValidatedCsvRow ValidateRow(
        CsvRawRow row,
        DateTime currentUtc)
    {
        ValidateRequiredFields(row);

        var date = ParseDate(row.Date, row.RowNumber);
        var executionTime = ParseDecimal(
            row.ExecutionTime,
            nameof(row.ExecutionTime),
            row.RowNumber);

        var value = ParseDecimal(
            row.Value,
            nameof(row.Value),
            row.RowNumber);

        if (date < MinAllowedDate)
        {
            throw new CsvValidationException(
                "Date cannot be earlier than 2000-01-01.",
                row.RowNumber);
        }

        if (date > currentUtc)
        {
            throw new CsvValidationException(
                "Date cannot be later than the current time.",
                row.RowNumber);
        }

        if (executionTime < 0)
        {
            throw new CsvValidationException(
                "ExecutionTime cannot be negative.",
                row.RowNumber);
        }

        if (value < 0)
        {
            throw new CsvValidationException(
                "Value cannot be negative.",
                row.RowNumber);
        }

        return new ValidatedCsvRow(
            date,
            executionTime,
            value);
    }

    private static void ValidateRequiredFields(CsvRawRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Date))
        {
            throw new CsvValidationException(
                "Date is required.",
                row.RowNumber);
        }

        if (string.IsNullOrWhiteSpace(row.ExecutionTime))
        {
            throw new CsvValidationException(
                "ExecutionTime is required.",
                row.RowNumber);
        }

        if (string.IsNullOrWhiteSpace(row.Value))
        {
            throw new CsvValidationException(
                "Value is required.",
                row.RowNumber);
        }
    }

    private static DateTime ParseDate(string value, int rowNumber)
    {
        if (!DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces |
                DateTimeStyles.AssumeUniversal |
                DateTimeStyles.AdjustToUniversal,
                out var parsedDate))
        {
            throw new CsvValidationException(
                "Date has an invalid format.",
                rowNumber);
        }

        return parsedDate.UtcDateTime;
    }

    private static decimal ParseDecimal(
        string value,
        string fieldName,
        int rowNumber)
    {
        const NumberStyles numberStyles =
            NumberStyles.AllowLeadingSign |
            NumberStyles.AllowDecimalPoint;

        if (!decimal.TryParse(
                value,
                numberStyles,
                CultureInfo.InvariantCulture,
                out var parsedValue))
        {
            throw new CsvValidationException(
                $"{fieldName} must be a valid number.",
                rowNumber);
        }

        return parsedValue;
    }
}