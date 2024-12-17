#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

using Agents;
using Azure;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.OpenAI;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Plugins;

Uri endpoint = new(Environment.GetEnvironmentVariable("AI:AzureSearch:Endpoint")!);
AzureKeyCredential keyCredential = new(Environment.GetEnvironmentVariable("AI:AzureSearch:ApiKey")!);

// Create kernel builder
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();

// Register ITextEmbeddingGenerationService implementation
kernelBuilder.Services.AddSingleton<ITextEmbeddingGenerationService, OpenAITextEmbeddingGenerationService>();

// SearchIndexClient from Azure .NET SDK to perform search operations.
kernelBuilder.Services.AddSingleton<SearchIndexClient>((_) => new SearchIndexClient(endpoint, keyCredential));

// Embedding generation service to convert string query to vector
kernelBuilder.AddAzureOpenAITextEmbeddingGeneration("text-embedding-ada-002", Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint")!, Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key")!);

// Chat completion service to ask questions based on data from Azure AI Search index.
kernelBuilder.AddAzureOpenAIChatCompletion("gpt-4o", Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint")!, Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key")!);

// Register Azure AI Search Plugin
kernelBuilder.Plugins.AddFromType<AzureAISearchPlugin>();

// Create kernel
var kernel = kernelBuilder.Build();

Console.WriteLine("Creating AzureAI agent...");

OpenAIAssistantDefinition definition = new(modelId: "gpt-4o");

OpenAIAssistantAgent agent =
await OpenAIAssistantAgent.CreateAsync(
    kernel,
    clientProvider: OpenAIClientProvider.ForAzureOpenAI(Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key")!, new Uri(Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint")!)),
    new(definition.ModelId)
    {
        Name = "AzureAI",
        Description = Descriptions.AzureAI,
        Instructions = Instructions.AzureAI,
        EnableCodeInterpreter = true,
    });

var thread = await agent.CreateThreadAsync();

Console.WriteLine("[COPILOT] : How can I help you today?");

while (true)
{
    Console.Write("[YOU] : ");
    var input = Console.ReadLine();
    if (input == "exit")
    {
        break;
    }

    var chat = new AgentGroupChat(agent);

    chat.AddChatMessage(new ChatMessageContent(AuthorRole.User, input));

    Console.WriteLine($"# {AuthorRole.User}: '{input}'");

    await foreach (var content in chat.InvokeAsync(agent))
    {
        Console.WriteLine($"# {content.Role} - {content.AuthorName ?? "*"}: '{content.Content}'");
    }
}