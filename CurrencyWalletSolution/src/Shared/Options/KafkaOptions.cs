namespace Shared.Options;

public class KafkaOptions
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; }
    public string WalletTopic { get; set; }
}