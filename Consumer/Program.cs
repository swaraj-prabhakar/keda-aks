using Consumer;
using MetroBus;

string rabbitMqUri = "rabbitmq://rabbitmq.keda-test.svc.cluster.local:5672";
string rabbitMqUserName = "user";
string rabbitMqPassword = "Pass123";
string queue = "message.queue";

var bus = MetroBusInitializer.Instance
                .UseRabbitMq(rabbitMqUri, rabbitMqUserName, rabbitMqPassword)
                    .SetPrefetchCount(1)
                    .RegisterConsumer<MessageConsumer>(queue)
                .Build();

await bus.StartAsync();
while (true) { }