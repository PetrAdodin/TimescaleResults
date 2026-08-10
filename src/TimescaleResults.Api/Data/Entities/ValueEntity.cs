namespace TimescaleResults.Api.Data.Entities;

public class ValueEntity
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public decimal ExecutionTime { get; set; }

    public decimal Value { get; set; }

    public int ResultId { get; set; }

    public ResultEntity Result { get; set; } = null!;
}