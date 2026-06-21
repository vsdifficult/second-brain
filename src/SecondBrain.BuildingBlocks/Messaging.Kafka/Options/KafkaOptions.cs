using SecondBrain.BuildingBlocks.Abstractions.Messaging;

namespace SecondBrain.BuildingBlocks.Infrastructure.Messaging.Options;


public class KafkaOptions : IKafkaOptions
{
    public const string SectionName = "Kafka";
    
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = "second-brain-group";
    public string ClientId { get; set; } = "second-brain-client";
    public int MessageTimeoutMs { get; set; } = 30000;
    public int MaxRetries { get; set; } = 3;
    public Dictionary<string, string> TopicMappings { get; set; } = new();
    
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BootstrapServers))
            throw new InvalidOperationException("Kafka BootstrapServers is required");
        
        if (MessageTimeoutMs <= 0)
            throw new InvalidOperationException("MessageTimeoutMs must be positive");
    }
}