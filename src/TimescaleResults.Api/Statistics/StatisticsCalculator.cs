using TimescaleResults.Api.Csv;

namespace TimescaleResults.Api.Statistics;

public sealed class StatisticsCalculator
{
    public CalculatedStatistics Calculate(
        IReadOnlyList<ValidatedCsvRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        if (rows.Count == 0)
        {
            throw new ArgumentException(
                "At least one row is required to calculate statistics.",
                nameof(rows));
        }

        var firstRow = rows[0];

        var minDate = firstRow.Date;
        var maxDate = firstRow.Date;

        var minValue = firstRow.Value;
        var maxValue = firstRow.Value;

        var averageExecutionTime = firstRow.ExecutionTime;
        var averageValue = firstRow.Value;

        var values = new decimal[rows.Count];
        values[0] = firstRow.Value;

        for (var index = 1; index < rows.Count; index++)
        {
            var row = rows[index];

            if (row.Date < minDate)
            {
                minDate = row.Date;
            }

            if (row.Date > maxDate)
            {
                maxDate = row.Date;
            }

            if (row.Value < minValue)
            {
                minValue = row.Value;
            }

            if (row.Value > maxValue)
            {
                maxValue = row.Value;
            }

            var count = index + 1;

            averageExecutionTime +=
                (row.ExecutionTime - averageExecutionTime) / count;

            averageValue +=
                (row.Value - averageValue) / count;

            values[index] = row.Value;
        }

        Array.Sort(values);

        var medianValue = CalculateMedian(values);

        var dateRangeSeconds =
            (decimal)(maxDate.Ticks - minDate.Ticks) /
            TimeSpan.TicksPerSecond;

        return new CalculatedStatistics(
            dateRangeSeconds,
            minDate,
            averageExecutionTime,
            averageValue,
            medianValue,
            maxValue,
            minValue);
    }

    private static decimal CalculateMedian(decimal[] sortedValues)
    {
        var middleIndex = sortedValues.Length / 2;

        if (sortedValues.Length % 2 != 0)
        {
            return sortedValues[middleIndex];
        }

        var lowerValue = sortedValues[middleIndex - 1];
        var upperValue = sortedValues[middleIndex];

        return lowerValue + (upperValue - lowerValue) / 2m;
    }
}