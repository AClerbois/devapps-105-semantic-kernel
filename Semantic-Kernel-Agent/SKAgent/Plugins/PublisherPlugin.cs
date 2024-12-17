using System.ComponentModel;
using System.Diagnostics;
using Microsoft.SemanticKernel;

namespace SKAgent.Plugins;

public sealed class PublisherPlugin
{
    [KernelFunction("Publish_Content")]
    [Description("Publish content to the server")]
    public static void SaveContent(string title, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var newFileName = title.ToLower().Replace(" ", "-");

        Console.WriteLine("===================================================");
        Console.WriteLine("Publisher Plugin");
        Console.WriteLine("===================================================");
        Console.WriteLine("Title: " + newFileName);
        Console.WriteLine("===================================================");

        string html = Markdig.Markdown.ToHtml(content);

        html = $@"
<!DOCTYPE html>
<html lang=""fr"">
<head>
    <meta charset=""UTF-8"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f0f0f0;
        }}
        .container {{
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            background-color: #fff;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <h1>{title}</h1>
        {html}
    </div>
</body>
</html>";

        string fileName = $"{newFileName}.html";
        if (!Directory.Exists("sites"))
            Directory.CreateDirectory("sites");
        File.WriteAllText($"sites/{fileName}", html);
        Console.WriteLine($"Content saved to {fileName}");

        Console.WriteLine("===================================================");
        Console.WriteLine("Lancement du fichier : " + newFileName);
        Console.WriteLine("===================================================");

        Process.Start(fileName);
    }
}