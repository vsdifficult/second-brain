namespace SecondBrain.BuildingBlocks.EFCore.Outbox;

public class OutboxMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Topic { get; set; }
    public required string Key { get; set; }
    public required string Payload { get; set; }
    public string? Type { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public int RetryCount { get; set; }
    public string? Error { get; set; }
}