
using Contracts;
using MetroBus;

string rabbitMqUri = "rabbitmq://20.241.242.61:5672";
string rabbitMqUserName = "user";
string rabbitMqPassword = "Pass123";

var bus = MetroBusInitializer.Instance
               .UseRabbitMq(rabbitMqUri, rabbitMqUserName, rabbitMqPassword)
               .InitializeEventProducer();

bool stopped = false;

while (!stopped)
{
    Console.WriteLine("Enter number of messages OR c to stop the application");
    string input = Console.ReadLine();
    int messageCount;
    if (input == "c")
    {
        stopped = true;
        continue;
    }
    else
    {
        messageCount = int.Parse(input);
    }

    List<Task> tasks = new ();

    for (int i = 0; i < messageCount; i++)
    {
        tasks.Add(
            bus.Publish(new Message
            {
                Body = "Hello!"
            })
        );

        Console.WriteLine($"{i}-sent");
    }

    await Task.WhenAll(tasks);
    Console.WriteLine("-----------------------------------------------------");
}