namespace SecondBrain.BuildingBlocks.Abstractions.Messaging;

public interface IKafkaOptions
{
    string BootstrapServers { get; }
    string GroupId { get; }
    string ClientId { get; }
    int MessageTimeoutMs { get; }
    int MaxRetries { get; }
    Dictionary<string, string> TopicMappings { get; }
}