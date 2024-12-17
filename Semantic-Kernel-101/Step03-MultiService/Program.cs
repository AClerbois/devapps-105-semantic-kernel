using System.ClientModel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.MistralAI;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Connectors.Ollama;
using OpenAI;
using OpenAI.Chat;

Console.WriteLine("Hello AI World");
Console.WriteLine("===============");

//IChatCompletionService chatService = new OpenAIChatCompletionService("gpt-4o-mini", Environment.GetEnvironmentVariable("AI:OpenAI:Key")); Console.WriteLine("Configuration : OpenAI - gpt-4o-mini");
//IChatCompletionService chatService = new GoogleAIGeminiChatCompletionService("gemini-1.5-flash-latest", Environment.GetEnvironmentVariable("AI:GoogleGemini:Key")); Console.WriteLine("Configuration : Google - gemini-1.5-flash-latest");
//IChatCompletionService chatService = new MistralAIChatCompletionService("mistral-small-mini", Environment.GetEnvironmentVariable("AI:Mistral:Key")); Console.WriteLine("Configuration : Mistral - mistral-small-mini");
// IChatCompletionService chatService = new AzureOpenAIChatCompletionService("gpt-4o", Environment.GetEnvironmentVariable("AI:AzureOpenAI:Endpoint"), Environment.GetEnvironmentVariable("AI:AzureOpenAI:Key")); Console.WriteLine("Configuration : Azure OpenAI - gpt-4o");
IChatCompletionService chatService = new OllamaChatCompletionService("phi3.5", new Uri("http://127.0.0.1:11434/")); Console.WriteLine("Configuration : Ollama - llama2-uncensored:7b");


Console.WriteLine("Q: Quel est la couleur du ciel ?");
Console.WriteLine( await chatService.GetChatMessageContentAsync("Quel est la couleur du ciel ?"));