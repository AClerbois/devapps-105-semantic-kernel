using System.ClientModel;
using OpenAI;
using OpenAI.Chat;

Console.WriteLine("Hello AI World");
Console.WriteLine("===============");

OpenAIClient client = new OpenAIClient(new ApiKeyCredential(Environment.GetEnvironmentVariable("AI:OpenAI:Key")));
ChatClient chatService = client.GetChatClient("gpt-4o-mini");

Console.WriteLine("Q: Quel est la couleur du ciel ?");
await foreach (var update in chatService.CompleteChatStreamingAsync("Quel est la couleur du ciel ? en 5000 mots"))
{
    foreach (var item in update.ContentUpdate)
    {
        Console.Write(item.Text);
    }
}