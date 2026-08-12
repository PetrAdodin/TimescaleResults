using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TimescaleResults.Api.Csv;
using TimescaleResults.Api.Data;
using TimescaleResults.Api.Data.Entities;
using TimescaleResults.Api.Statistics;

namespace TimescaleResults.Api.Services;

public sealed class CsvUploadService(
    AppDbContext dbContext,
    CsvFileParser csvFileParser,
    CsvValueValidator csvValueValidator,
    StatisticsCalculator statisticsCalculator)
{
    public async Task UploadAsync(
        string fileName,
        Stream stream,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(stream);

        var rawRows = await csvFileParser.ParseAsync(
            stream,
            cancellationToken);

        var validatedRows = csvValueValidator.Validate(
            rawRows,
            DateTime.UtcNow);

        var statistics = statisticsCalculator.Calculate(validatedRows);

        var result = CreateResultEntity(
            fileName,
            validatedRows,
            statistics);

        var fileLockKey = CreateFileLockKey(fileName);

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock({fileLockKey});",
            cancellationToken);

        await dbContext.Results
            .Where(existingResult => existingResult.FileName == fileName)
            .ExecuteDeleteAsync(cancellationToken);

        dbContext.Results.Add(result);

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    private static long CreateFileLockKey(string fileName)
    {
        var bytes = Encoding.UTF8.GetBytes(fileName);
        var hash = SHA256.HashData(bytes);

        return BinaryPrimitives.ReadInt64BigEndian(hash);
    }

    private static ResultEntity CreateResultEntity(
        string fileName,
        IReadOnlyList<ValidatedCsvRow> rows,
        CalculatedStatistics statistics)
    {
        var result = new ResultEntity
        {
            FileName = fileName,
            DateRangeSeconds = statistics.DateRangeSeconds,
            MinDate = statistics.MinDate,
            AverageExecutionTime = statistics.AverageExecutionTime,
            AverageValue = statistics.AverageValue,
            MedianValue = statistics.MedianValue,
            MaxValue = statistics.MaxValue,
            MinValue = statistics.MinValue
        };

        foreach (var row in rows)
        {
            result.Values.Add(new ValueEntity
            {
                Date = row.Date,
                ExecutionTime = row.ExecutionTime,
                Value = row.Value,
                Result = result
            });
        }

        return result;
    }
}