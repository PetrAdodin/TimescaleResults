namespace TimescaleResults.Api.Data.Entities;

public class ResultEntity
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public decimal DateRangeSeconds { get; set; }

    public DateTime MinDate { get; set; }

    public decimal AverageExecutionTime { get; set; }

    public decimal AverageValue { get; set; }

    public decimal MedianValue { get; set; }

    public decimal MaxValue { get; set; }

    public decimal MinValue { get; set; }

    public ICollection<ValueEntity> Values { get; set; } = new List<ValueEntity>();
}