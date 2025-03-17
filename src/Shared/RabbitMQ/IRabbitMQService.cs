using System;
using System.Threading.Tasks;

namespace Shared.RabbitMQ;

public interface IRabbitMQService
{
    Task PublishMessage<T>(string queueName, T message);
    Task Subscribe<T>(string queueName, Func<T, Task> handler) where T : class;
} 