using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace WalletApi.Services.Kafka;

public class KafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaOptions _settings;
    
    public KafkaProducerService(IOptions<KafkaOptions> options)
    {
        _settings = options.Value;
        var config = new ProducerConfig { BootstrapServers = _settings.BootstrapServers };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }
    
    public async Task PublishAsync<T>(string key, T message)
    {
        var jsonMessage = JsonSerializer.Serialize(message);
        await _producer.ProduceAsync(_settings.WalletTopic, new Message<string, string>
        {
            Key = key,
            Value = jsonMessage
        });
    }
}