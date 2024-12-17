using Microsoft.SemanticKernel;

class Speakers
{
    IList<Speaker> Items =
    [
        new Speaker("Adrien", "Azure OpenAI"),
        new Speaker("Adrien", "Azure Playwright"),
        new Speaker("Christophe", "Azure Playwright"),
        new Speaker("Christophe", ".NET Blazor - What's new"),
        new Speaker("Denis", ".NET Blazor - What's new"),
        new Speaker("Denis", "Microsoft Fluent UI Blazor"),
        new Speaker("Vincent", "Microsoft Fluent UI Blazor")
    ];

    [KernelFunction("GetSpeakersByName")]
    public IList<Speaker> GetSpeakersByName(string name)
    {
        return Items.Where(s => s.Name == name).ToList();
    }

    [KernelFunction("GetSessionsBySpeaker")]
    public IList<Speaker> GetSessionsBySpeaker(string session)
    {
        return Items.Where(s => s.SessionTitle == session).ToList();
    }

    public record Speaker(string Name, string SessionTitle);
}