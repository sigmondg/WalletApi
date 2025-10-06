using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;
using Shared.Options;

namespace WalletApi.Services.Kafka;

public class KafkaConsumerService : BackgroundService
{
    private readonly KafkaOptions _settings;

    public KafkaConsumerService(IOptions<KafkaOptions> options)
    {
        _settings = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        
        await EnsureTopicExists(_settings.BootstrapServers, _settings.WalletTopic);
        
        var config = new ConsumerConfig()
        { BootstrapServers = _settings.BootstrapServers,
          GroupId = "wallet-consumer-group",
          AutoOffsetReset = AutoOffsetReset.Earliest,
        AllowAutoCreateTopics = true
        };
        
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.WalletTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            var result = consumer.Consume(stoppingToken);
            Console.WriteLine($"Received event: {result.Message.Value}");
        }
    }
    
    public static async Task EnsureTopicExists(string bootstrapServers, string topicName)
    {
        using var adminClient = new AdminClientBuilder(new AdminClientConfig 
        { 
            BootstrapServers = bootstrapServers 
        }).Build();
    
        try
        {
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
            var topicExists = metadata.Topics.Any(t => t.Topic == topicName);
        
            if (!topicExists)
            {
                await adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification
                    {
                        Name = topicName,
                        NumPartitions = 1,
                        ReplicationFactor = 1
                    }
                });
            
                Console.WriteLine($"Topic '{topicName}' created.");
                await Task.Delay(2000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Topic creation check failed: {ex.Message}");
        }
    }
}