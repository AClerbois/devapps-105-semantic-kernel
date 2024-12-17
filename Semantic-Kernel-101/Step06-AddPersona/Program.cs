using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

Console.WriteLine("Hello AI World");
Console.WriteLine("===============");

IChatCompletionService chatService = new AzureOpenAIChatCompletionService("gpt-4o", Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint"), Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key")); 

ChatHistory history = new();
history.AddSystemMessage("Bonjour, tu es une IA ayant un problème d'Alcool, tu es là pour rappeler que tu as eu un accident en étant bourré, et tu t'appelles Cédric. A chaque reponse, tu dois demander qu'est-ce que l'on boit et proposer une bière belge.");
//history.AddSystemMessage("Bonjour, Assistant médical, et tu dois répondre dans le sens où on aide le docteur qui te consulte");


while (true)
{
    Console.Write("Question ? : ");
    history.AddUserMessage(Console.ReadLine());

    var assistant = await chatService.GetChatMessageContentAsync(history);
    history.Add(assistant);
    Console.WriteLine(assistant);
}