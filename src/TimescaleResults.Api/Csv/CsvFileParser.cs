using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace TimescaleResults.Api.Csv;

public sealed class CsvFileParser
{
    private const int MaxRowCount = 10_000;

    private static readonly string[] ExpectedHeader =
    [
        "Date",
        "ExecutionTime",
        "Value"
    ];

    public async Task<IReadOnlyList<CsvRawRow>> ParseAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException("The stream must be readable.", nameof(stream));
        }

        using var reader = new StreamReader(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true,
            bufferSize: 1024,
            leaveOpen: true);

        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false,
            IgnoreBlankLines = false,
            TrimOptions = TrimOptions.Trim
        };

        using var parser = new CsvParser(reader, configuration);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!await parser.ReadAsync())
            {
                throw new CsvValidationException("CSV file is empty.");
            }

            ValidateHeader(parser.Record, parser.RawRow);

            var rows = new List<CsvRawRow>();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!await parser.ReadAsync())
                {
                    break;
                }

                var record = parser.Record ?? [];

                ValidateRowStructure(record, parser.RawRow);

                rows.Add(new CsvRawRow(
                    parser.RawRow,
                    record[0],
                    record[1],
                    record[2]));

                if (rows.Count > MaxRowCount)
                {
                    throw new CsvValidationException(
                        $"CSV file cannot contain more than {MaxRowCount} data rows.");
                }
            }

            if (rows.Count == 0)
            {
                throw new CsvValidationException(
                    "CSV file must contain at least one data row.");
            }

            return rows;
        }
        catch (CsvHelperException exception)
        {
            throw new CsvValidationException(
                $"Invalid CSV format near row {parser.RawRow}.",
                parser.RawRow,
                exception);
        }
    }

    private static void ValidateHeader(string[]? header, int rowNumber)
    {
        if (header is null ||
            header.Length != ExpectedHeader.Length ||
            !header.SequenceEqual(ExpectedHeader, StringComparer.Ordinal))
        {
            throw new CsvValidationException(
                "CSV header must be exactly: Date;ExecutionTime;Value.",
                rowNumber);
        }
    }

    private static void ValidateRowStructure(string[] record, int rowNumber)
    {
        if (record.Length == 0 || record.All(string.IsNullOrWhiteSpace))
        {
            throw new CsvValidationException(
                "CSV data row cannot be empty.",
                rowNumber);
        }

        if (record.Length != ExpectedHeader.Length)
        {
            throw new CsvValidationException(
                $"CSV data row must contain exactly {ExpectedHeader.Length} fields.",
                rowNumber);
        }
    }
}