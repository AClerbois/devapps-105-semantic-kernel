using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Agents;

// Initialization of the service collection
ServiceCollection c = new();

c.AddAzureOpenAIChatCompletion(
    "gpt-4o",
    Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint"),
    Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key"));
c.AddKernel();
IServiceProvider services = c.BuildServiceProvider();

Kernel kernel = services.GetRequiredService<Kernel>();

Console.WriteLine("Creating AzureAI agent...");

ChatCompletionAgent agent = new()
{
    Name = "SummarizationAgent",
    Description = Descriptions.SummarizationAgent,
    Instructions = Instructions.SummarizationAgent,
    Kernel = kernel,
};

Console.WriteLine("Agent created.");


ChatHistory history = [];
bool isComplete = false;
do
{
    Console.WriteLine();
    Console.Write("> ");
    string input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }

    history.AddUserMessage(input);

    Console.WriteLine();

    DateTime now = DateTime.Now;
    KernelArguments arguments = new()
    {
        { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
    };

    await foreach (ChatMessageContent response in agent.InvokeAsync(history, arguments))
    {
        Console.Write("[AzureAI] : ");
        Console.WriteLine($"{response.Content}");
    }


} while (!isComplete);