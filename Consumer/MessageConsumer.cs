using Contracts;
using MassTransit;

namespace Consumer;

internal class MessageConsumer : IConsumer<Message>
{
    public async Task Consume(ConsumeContext<Message> context)
    {
        await Task.Delay(1000);

        await Console.Out.WriteLineAsync(context.Message.Body);
    }
}
