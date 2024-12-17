using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Agents;
using Agents;
using Microsoft.SemanticKernel.Agents.History;
using Microsoft.SemanticKernel.Agents.Chat;
using System.Text.Json;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.Extensions.Logging;

// Initialization of the service collection
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.Services.AddLogging();
kernelBuilder.AddAzureOpenAIChatCompletion(
    "gpt-4o",
    Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint"),
    Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key"),
    serviceId: "service1");

kernelBuilder.AddAzureOpenAIChatCompletion(
    "gpt-4o-mini",
    Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint"),
    Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key"),
    serviceId: "service2");


Kernel kernel = kernelBuilder.Build();

var tooledKernel = kernel.Clone();
tooledKernel.Plugins.AddFromType<SKAgent.Plugins.PublisherPlugin>();

ChatCompletionAgent agentWriter = new()
{
    Name = "Writer",
    Description = Descriptions.Writer,
    Instructions = Instructions.Writer,
    Kernel = kernel,
    Arguments = new KernelArguments
    {
        { "serviceId", "service1" }
    }
};

ChatCompletionAgent agentReviewer = new()
{
    Name = "Reviewer",
    Description = Descriptions.Reviewer,
    Instructions = Instructions.Reviewer,
    Kernel = tooledKernel,
    Arguments = new KernelArguments(new AzureOpenAIPromptExecutionSettings()
    {
        ServiceId = "service2",
        FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
    })
};


KernelFunction selectionFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        $$$"""
        Examine la RESPONSE fournie et choisi le participant suivant.
        Indiquer uniquement le nom du participant choisi sans explication.
        Ne jamais choisir le participant nommé dans la RESPONSE.

        Choisissez uniquement parmi les participants suivants:
        - {{{agentWriter.Name}}}
        - {{{agentReviewer.Name}}}

        Suivez toujours les règles suivantes lorsque vous choisissez le participant suivant:
        - Si RESPONSE est une entrée de l'utilisateur, c'est le tour de {{{agentWriter.Name}}}.
        - Si la RÉPONSE est par {{{agentReviewer.Name}}}, c'est le tour de {{{agentWriter.Name}}}.
        - Si la RÉPONSE est par {{{agentWriter.Name}}}, c'est le tour de {{{agentReviewer.Name}}}.

        RESPONSE:
        {{$lastmessage}}
        """,
        safeParameterNames: "lastmessage");


const string TerminationToken = "yes";

KernelFunction terminationFunction =
    AgentGroupChat.CreatePromptFunctionForStrategy(
        $$$"""
        Examinez la RESPONSE et déterminez si le contenu a été jugé satisfaisant.
        Si le contenu est satisfaisant, publier le contenu et répondre par un seul mot sans explication: {{{TerminationToken}}}.
        Si des suggestions spécifiques sont formulées, le résultat n'est pas satisfaisant.

        RESPONSE:
        {{$lastmessage}}
        """,
        safeParameterNames: "lastmessage");


ChatHistoryTruncationReducer historyReducer = new(1);


AgentGroupChat chat = new(agentWriter, agentReviewer)
{
    ExecutionSettings = new AgentGroupChatSettings
    {
        SelectionStrategy =
            new KernelFunctionSelectionStrategy(selectionFunction, kernel)
            {
                // Always start with the editor agent.
                InitialAgent = agentWriter,
                // Save tokens by only including the final response
                HistoryReducer = historyReducer,
                // The prompt variable name for the history argument.
                HistoryVariableName = "lastmessage",
                // Returns the entire result value as a string.
                ResultParser = (result) => result.GetValue<string>() ?? agentReviewer.Name
            },
        TerminationStrategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
        {
            // Only evaluate for editor's response
            Agents = [agentReviewer],
            // Save tokens by only including the final response
            HistoryReducer = historyReducer,
            // The prompt variable name for the history argument.
            HistoryVariableName = "lastmessage",
            // Limit total number of turns
            MaximumIterations = 3,
            // Customer result parser to determine if the response is "yes"
            ResultParser = (result) => result.GetValue<string>()?.Contains(TerminationToken, StringComparison.OrdinalIgnoreCase) ?? false
        }
    }
};

Console.WriteLine("Ready!");


bool isComplete = false;
do
{

    Console.WriteLine("Entrez votre thématique:");
    Console.Write("> ");
    string input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }
    input = input.Trim();
    if (input.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        isComplete = true;
        break;
    }

    if (input.Equals("RESET", StringComparison.OrdinalIgnoreCase))
    {
        await chat.ResetAsync();
        Console.WriteLine("[Converation has been reset]");
        continue;
    }

    chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

    chat.IsComplete = false;

    try
    {
        await foreach (ChatMessageContent response in chat.InvokeAsync())
        {
            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine();
            Console.WriteLine($"{response.AuthorName.ToUpperInvariant()}:{Environment.NewLine}{response.Content}");
        }
    }
    catch (HttpOperationException exception)
    {
        Console.WriteLine(exception.Message);
        if (exception.InnerException != null)
        {
            Console.WriteLine(exception.InnerException.Message);
            if (exception.InnerException.Data.Count > 0)
            {
                Console.WriteLine(JsonSerializer.Serialize(exception.InnerException.Data, new JsonSerializerOptions() { WriteIndented = true }));
            }
        }

    }
} while (!isComplete);