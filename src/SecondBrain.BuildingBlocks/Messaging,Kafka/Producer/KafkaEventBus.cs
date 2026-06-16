using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecondBrain.BuildingBlocks.Infrastructure.Messaging.Options;
using SecondBrain.BuildingBlocks.Messaging.Kafka.Abstractions;

namespace SecondBrain.BuildingBlocks.Messaging.Kafka.Producer;

public class KafkaEventBus : IEventBus, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public KafkaEventBus(
        IOptions<KafkaOptions> options,
        ILogger<KafkaEventBus> logger)
    {
        _options = options.Value;
        _options.Validate();
        _logger = logger;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        var config = new ProducerConfig
        {
            BootstrapServers = _options.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MessageTimeoutMs = _options.MessageTimeoutMs > 0 ? _options.MessageTimeoutMs : 30000,
            EnableDeliveryReports = false,
            MessageMaxBytes = 5_000_000,
            MaxInFlight = 5,
            QueueBufferingMaxMessages = 100000,
            QueueBufferingMaxKbytes = 1048576,
            CompressionType = CompressionType.Snappy
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, error) => 
                _logger.LogError("Kafka error: {Reason}", error.Reason))
            .SetLogHandler((_, log) => 
                _logger.LogDebug("Kafka log: {Message}", log.Message))
            .Build();
    }

    public async Task PublishAsync<T>(
        string topic, 
        string key, 
        T @event, 
        CancellationToken cancellationToken = default) 
        where T : Event
    {
        try
        {
            var jsonValue = JsonSerializer.Serialize(@event, _jsonOptions);

            var kafkaMessage = new Message<string, string>
            {
                Key = key ?? @event.Id.ToString(),
                Value = jsonValue,
                Headers = new Headers()
            };

            var currentActivity = Activity.Current;
            if (currentActivity != null)
            {
                var traceId = currentActivity.TraceId.ToString();
                var spanId = currentActivity.SpanId.ToString();
                
                kafkaMessage.Headers.Add("traceparent", 
                    Encoding.UTF8.GetBytes($"00-{traceId}-{spanId}-01"));
                
                foreach (var baggage in currentActivity.Baggage)
                {
                    kafkaMessage.Headers.Add($"baggage-{baggage.Key}", 
                        Encoding.UTF8.GetBytes(baggage.Value ?? string.Empty));
                }
            }

            kafkaMessage.Headers.Add("message-type", 
                Encoding.UTF8.GetBytes(typeof(T).Name));
            kafkaMessage.Headers.Add("message-version", 
                Encoding.UTF8.GetBytes("1.0"));
            kafkaMessage.Headers.Add("sent-at", 
                Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")));

            var stopwatch = Stopwatch.StartNew();
            
            var deliveryResult = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            
            stopwatch.Stop();
            
            _logger.LogInformation(
                "Event {EventType} published to {Topic} with offset {Offset} in {ElapsedMs}ms",
                typeof(T).Name,
                topic,
                deliveryResult.Offset,
                stopwatch.ElapsedMilliseconds);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, 
                "Failed to publish event {EventType} to {Topic}. Error: {Reason}", 
                typeof(T).Name, 
                topic, 
                ex.Error.Reason);
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning("Publishing event {EventType} was cancelled", typeof(T).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unexpected error while publishing event {EventType} to {Topic}", 
                typeof(T).Name, 
                topic);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;
        
        if (disposing)
        {
            try
            {
                _logger.LogInformation("Flushing Kafka producer...");
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _logger.LogInformation("Kafka producer disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Kafka producer disposal");
            }
        }
        
        _disposed = true;
    }
}