using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Events;

namespace RankCalculator.Producers;

public class EventProducerService : IEventProducerService
{
    private readonly string _eventsExchange;
    private readonly string _rankRoutingKey;
    private readonly string _rabbitHost;
    private readonly int _rabbitPort;
    private readonly string _rabbitUser;
    private readonly string _rabbitPass;
    
    private IConnection? _connection;
    private IChannel? _channel;
    private bool _isConnected;

    public EventProducerService()
    {
        _eventsExchange = Environment.GetEnvironmentVariable("RABBITMQ_EVENTS_EXCHANGE")!;
        _rankRoutingKey = Environment.GetEnvironmentVariable("RABBITMQ_RANK_ROUTING_KEY")!;
        _rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST")!;
        _rabbitPort = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT")!);
        _rabbitUser = Environment.GetEnvironmentVariable("RABBITMQ_USER")!;
        _rabbitPass = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")!;
        
    }

    public async Task ConnectRabbitMq(CancellationToken cancellationToken = default)
    {
        if (_isConnected) return;

        Console.WriteLine($"Подключение к RabbitMQ: {_rabbitHost}:{_rabbitPort}");

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = _rabbitHost,
            Port = _rabbitPort,
            UserName = _rabbitUser,
            Password = _rabbitPass,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        _channel = await _connection.CreateChannelAsync(null, cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _eventsExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        Console.WriteLine($"Подключено к RabbitMQ, exchange: {_eventsExchange}");
    }

    public async Task PublishRankEventAsync(string textId, double rankValue, CancellationToken cancellationToken = default)
    {
        RankCalculatedEvent rankEvent = new RankCalculatedEvent(textId, rankValue);
        string message = JsonSerializer.Serialize(rankEvent);
        
        await PublishMessageAsync(message, cancellationToken);
    }

    public async Task PublishMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        if (!_isConnected)
        {
            await ConnectRabbitMq(cancellationToken);
        }

        byte[] messageData = Encoding.UTF8.GetBytes(message);

        await _channel!.BasicPublishAsync(
            exchange: _eventsExchange,
            routingKey: _rankRoutingKey,
            mandatory: false,
            body: messageData,
            cancellationToken: cancellationToken
        );
    }

    public async Task ClearConnections(CancellationToken cancellationToken = default)
    {
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
            await _channel.DisposeAsync();
        }

        if (_connection != null && _connection.IsOpen)
        {
            await _connection.CloseAsync(cancellationToken);
            await _connection.DisposeAsync();
        }
        
        _isConnected = false;
    }
}