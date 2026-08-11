using TimescaleResults.Api.Csv;
using TimescaleResults.Api.Statistics;
using Xunit;

namespace TimescaleResults.Api.Tests.Statistics;

public sealed class StatisticsCalculatorTests
{
    private readonly StatisticsCalculator _calculator = new();

    [Fact]
    public void Calculate_MultipleRows_ReturnsExpectedStatistics()
    {
        var rows = new[]
        {
            CreateRow(
                new DateTime(2026, 8, 10, 12, 0, 10, DateTimeKind.Utc),
                3m,
                10m),
            CreateRow(
                new DateTime(2026, 8, 10, 12, 0, 0, DateTimeKind.Utc),
                1m,
                2m),
            CreateRow(
                new DateTime(2026, 8, 10, 12, 0, 5, DateTimeKind.Utc),
                2m,
                6m)
        };

        var result = _calculator.Calculate(rows);

        Assert.Equal(10m, result.DateRangeSeconds);
        Assert.Equal(
            new DateTime(
                2026,
                8,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc),
            result.MinDate);

        Assert.Equal(2m, result.AverageExecutionTime);
        Assert.Equal(6m, result.AverageValue);
        Assert.Equal(6m, result.MedianValue);
        Assert.Equal(10m, result.MaxValue);
        Assert.Equal(2m, result.MinValue);
    }

    [Fact]
    public void Calculate_EvenValueCount_CalculatesMedianFromTwoMiddleValues()
    {
        var rows = new[]
        {
            CreateRow(value: 20m),
            CreateRow(value: 3m),
            CreateRow(value: 10m),
            CreateRow(value: 1m)
        };

        var result = _calculator.Calculate(rows);

        Assert.Equal(6.5m, result.MedianValue);
    }

    [Fact]
    public void Calculate_OddValueCount_ReturnsMiddleValueAsMedian()
    {
        var rows = new[]
        {
            CreateRow(value: 100m),
            CreateRow(value: 1m),
            CreateRow(value: 5m)
        };

        var result = _calculator.Calculate(rows);

        Assert.Equal(5m, result.MedianValue);
    }

    [Fact]
    public void Calculate_SingleRow_ReturnsSameValuesAndZeroDateRange()
    {
        var date = new DateTime(
            2026,
            8,
            10,
            12,
            0,
            0,
            DateTimeKind.Utc);

        var rows = new[]
        {
            CreateRow(
                date,
                executionTime: 2.5m,
                value: 7.5m)
        };

        var result = _calculator.Calculate(rows);

        Assert.Equal(0m, result.DateRangeSeconds);
        Assert.Equal(date, result.MinDate);
        Assert.Equal(2.5m, result.AverageExecutionTime);
        Assert.Equal(7.5m, result.AverageValue);
        Assert.Equal(7.5m, result.MedianValue);
        Assert.Equal(7.5m, result.MaxValue);
        Assert.Equal(7.5m, result.MinValue);
    }

    [Fact]
    public void Calculate_LargeValues_DoesNotOverflowAverageOrMedian()
    {
        var rows = new[]
        {
            CreateRow(
                executionTime: decimal.MaxValue,
                value: decimal.MaxValue),
            CreateRow(
                executionTime: decimal.MaxValue,
                value: decimal.MaxValue)
        };

        var result = _calculator.Calculate(rows);

        Assert.Equal(
            decimal.MaxValue,
            result.AverageExecutionTime);

        Assert.Equal(
            decimal.MaxValue,
            result.AverageValue);

        Assert.Equal(
            decimal.MaxValue,
            result.MedianValue);
    }

    [Fact]
    public void Calculate_EmptyRows_ThrowsArgumentException()
    {
        var rows = Array.Empty<ValidatedCsvRow>();

        Assert.Throws<ArgumentException>(
            () => _calculator.Calculate(rows));
    }

    private static ValidatedCsvRow CreateRow(
        DateTime? date = null,
        decimal executionTime = 1m,
        decimal value = 1m)
    {
        return new ValidatedCsvRow(
            date ?? new DateTime(
                2026,
                8,
                10,
                12,
                0,
                0,
                DateTimeKind.Utc),
            executionTime,
            value);
    }
}