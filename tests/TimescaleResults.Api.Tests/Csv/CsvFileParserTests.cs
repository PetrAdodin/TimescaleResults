using System.Text;
using TimescaleResults.Api.Csv;
using Xunit;

namespace TimescaleResults.Api.Tests.Csv;

public sealed class CsvFileParserTests
{
    private readonly CsvFileParser _parser = new();

    [Fact]
    public async Task ParseAsync_ValidFile_ReturnsParsedRows()
    {
        const string csv = """
            Date;ExecutionTime;Value
            2026-08-10T12:00:00Z;1.5;10.25
            2026-08-10T12:00:01Z;2;20
            """;

        await using var stream = CreateStream(csv);

        var rows = await _parser.ParseAsync(
            stream,
            CancellationToken.None);

        Assert.Equal(2, rows.Count);

        Assert.Equal(2, rows[0].RowNumber);
        Assert.Equal("2026-08-10T12:00:00Z", rows[0].Date);
        Assert.Equal("1.5", rows[0].ExecutionTime);
        Assert.Equal("10.25", rows[0].Value);

        Assert.Equal(3, rows[1].RowNumber);
    }

    [Fact]
    public async Task ParseAsync_FileWithoutData_ThrowsValidationException()
    {
        const string csv = "Date;ExecutionTime;Value";

        await using var stream = CreateStream(csv);

        var exception = await Assert.ThrowsAsync<CsvValidationException>(
            () => _parser.ParseAsync(
                stream,
                CancellationToken.None));

        Assert.Contains("at least one data row", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_InvalidHeader_ThrowsValidationException()
    {
        const string csv = """
            Date;Value;ExecutionTime
            2026-08-10T12:00:00Z;10;1
            """;

        await using var stream = CreateStream(csv);

        var exception = await Assert.ThrowsAsync<CsvValidationException>(
            () => _parser.ParseAsync(
                stream,
                CancellationToken.None));

        Assert.Equal(1, exception.RowNumber);
        Assert.Contains("header", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_EmptyDataRow_ThrowsValidationException()
    {
        const string csv = "Date;ExecutionTime;Value\n\n";

        await using var stream = CreateStream(csv);

        var exception = await Assert.ThrowsAsync<CsvValidationException>(
            () => _parser.ParseAsync(
                stream,
                CancellationToken.None));

        Assert.Equal(2, exception.RowNumber);
    }

    [Fact]
    public async Task ParseAsync_RowWithExtraColumn_ThrowsValidationException()
    {
        const string csv = """
            Date;ExecutionTime;Value
            2026-08-10T12:00:00Z;1;10;unexpected
            """;

        await using var stream = CreateStream(csv);

        var exception = await Assert.ThrowsAsync<CsvValidationException>(
            () => _parser.ParseAsync(
                stream,
                CancellationToken.None));

        Assert.Equal(2, exception.RowNumber);
        Assert.Contains("exactly 3 fields", exception.Message);
    }

    [Fact]
    public async Task ParseAsync_ExactlyTenThousandRows_AcceptsFile()
    {
        await using var stream = CreateStream(CreateCsv(10_000));

        var rows = await _parser.ParseAsync(
            stream,
            CancellationToken.None);

        Assert.Equal(10_000, rows.Count);
    }

    [Fact]
    public async Task ParseAsync_MoreThanTenThousandRows_ThrowsValidationException()
    {
        await using var stream = CreateStream(CreateCsv(10_001));

        var exception = await Assert.ThrowsAsync<CsvValidationException>(
            () => _parser.ParseAsync(
                stream,
                CancellationToken.None));

        Assert.Contains("more than 10000", exception.Message);
    }

    private static MemoryStream CreateStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    private static string CreateCsv(int rowCount)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Date;ExecutionTime;Value");

        for (var index = 0; index < rowCount; index++)
        {
            builder.AppendLine("2026-08-10T12:00:00Z;1;10");
        }

        return builder.ToString();
    }
}