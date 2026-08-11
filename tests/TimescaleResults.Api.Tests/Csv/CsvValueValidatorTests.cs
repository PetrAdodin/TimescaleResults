using TimescaleResults.Api.Csv;
using Xunit;

namespace TimescaleResults.Api.Tests.Csv;

public sealed class CsvValueValidatorTests
{
    private static readonly DateTime CurrentUtc =
        new(2026, 8, 11, 12, 0, 0, DateTimeKind.Utc);

    private readonly CsvValueValidator _validator = new();

    [Fact]
    public void Validate_ValidRow_ReturnsTypedValuesInUtc()
    {
        var rows = new[]
        {
            new CsvRawRow(
                2,
                "2026-08-10T15:00:00+03:00",
                "1.5",
                "10.25")
        };

        var result = _validator.Validate(rows, CurrentUtc);

        var row = Assert.Single(result);

        Assert.Equal(
            new DateTime(
                2026,
                8,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc),
            row.Date);

        Assert.Equal(1.5m, row.ExecutionTime);
        Assert.Equal(10.25m, row.Value);
    }

    [Fact]
    public void Validate_DateAtMinimumBoundary_AcceptsRow()
    {
        var rows = new[]
        {
            new CsvRawRow(
                2,
                "2000-01-01T00:00:00Z",
                "0",
                "0")
        };

        var result = _validator.Validate(rows, CurrentUtc);

        Assert.Single(result);
    }

    [Fact]
    public void Validate_DateBeforeMinimum_ThrowsValidationException()
    {
        var rows = new[]
        {
            new CsvRawRow(
                2,
                "1999-12-31T23:59:59Z",
                "1",
                "1")
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(2, exception.RowNumber);
        Assert.Contains("earlier than 2000-01-01", exception.Message);
    }

    [Fact]
    public void Validate_FutureDate_ThrowsValidationException()
    {
        var rows = new[]
        {
            new CsvRawRow(
                5,
                "2026-08-11T12:00:01Z",
                "1",
                "1")
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(5, exception.RowNumber);
        Assert.Contains("current time", exception.Message);
    }

    [Fact]
    public void Validate_InvalidDate_ThrowsValidationException()
    {
        var rows = new[]
        {
            new CsvRawRow(
                3,
                "not-a-date",
                "1",
                "1")
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(3, exception.RowNumber);
        Assert.Contains("invalid format", exception.Message);
    }

    [Fact]
    public void Validate_InvalidExecutionTime_ThrowsValidationException()
    {
        var rows = new[]
        {
            new CsvRawRow(
                2,
                "2026-08-10T12:00:00Z",
                "not-a-number",
                "1")
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(2, exception.RowNumber);
        Assert.Contains("ExecutionTime", exception.Message);
    }

    [Fact]
    public void Validate_NegativeExecutionTime_ThrowsValidationException()
    {
        var rows = new[]
        {
            new CsvRawRow(
                2,
                "2026-08-10T12:00:00Z",
                "-0.1",
                "1")
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(2, exception.RowNumber);
        Assert.Contains("ExecutionTime", exception.Message);
    }

    [Fact]
    public void Validate_NegativeValue_ThrowsValidationException()
    {
        var rows = new[]
        {
            new CsvRawRow(
                2,
                "2026-08-10T12:00:00Z",
                "1",
                "-0.1")
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(2, exception.RowNumber);
        Assert.Contains("Value", exception.Message);
    }

    [Fact]
    public void Validate_CommaAsDecimalSeparator_ThrowsValidationException()
    {
        var rows = new[]
        {
        new CsvRawRow(
            2,
            "2026-08-10T12:00:00Z",
            "1,5",
            "10")
    };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(2, exception.RowNumber);
        Assert.Contains("ExecutionTime", exception.Message);
    }

    [Theory]
    [InlineData("", "1", "1", "Date")]
    [InlineData("2026-08-10T12:00:00Z", "", "1", "ExecutionTime")]
    [InlineData("2026-08-10T12:00:00Z", "1", "", "Value")]
    public void Validate_MissingRequiredField_ThrowsValidationException(
        string date,
        string executionTime,
        string value,
        string expectedField)
    {
        var rows = new[]
        {
            new CsvRawRow(
                7,
                date,
                executionTime,
                value)
        };

        var exception = Assert.Throws<CsvValidationException>(
            () => _validator.Validate(rows, CurrentUtc));

        Assert.Equal(7, exception.RowNumber);
        Assert.Contains(expectedField, exception.Message);
    }
}