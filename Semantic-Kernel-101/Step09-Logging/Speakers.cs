using Microsoft.SemanticKernel;

class Speakers
{
    List<Speaker> Items =
    [
        new Speaker("Adrien", "Azure OpenAI"),
        new Speaker("Adrien", "Azure Playwright"),
        new Speaker("Christophe", "Azure Playwright"),
        new Speaker("Christophe", ".NET Blazor - What's new"),
        new Speaker("Denis", ".NET Blazor - What's new"),
        new Speaker("Denis", "Microsoft Fluent UI Blazor"),
        new Speaker("Vincent", "Microsoft Fluent UI Blazor")
    ];

    [KernelFunction("Get_Speakers_By_Name")]
    public IList<Speaker> GetSpeakersByName(string name)
    {
        return Items.Where(s => s.Name == name).ToList();
    }

    [KernelFunction("Get_Sessions_By_Speaker")]
    public IList<Speaker> GetSessionsBySpeaker(string session)
    {
        return Items.Where(s => s.SessionTitle == session).ToList();
    }

    [KernelFunction("Add_A_New_Speaker_And_Session")]
    public void AddANewSpeakerAndSession(string speaker, string session)
    {
        Items.Add(new Speaker(speaker, session));
    }

    public record Speaker(string Name, string SessionTitle);
}