// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

if (args != null && args.Length > 0)
{
    // Retrieve arguments
    string apikey = "sk-proj-eIRIzMcgB992Du0wMdmfT3BlbkFJpV9JA5RSyjKxGrc8BDQv";
    string chatgptModel = "gpt-3.5-turbo";
    string outputFolder = "C:\\Users\\lhtran\\OneDrive - Advanced Infusion Solutions\\AI_Learning\\UnittestGenerations";
    string selectedFile = "C:\\lynx\\src\\Shared Libraries\\AIS.Domain\\Data\\Profile\\AddressBLL.cs";

    if (string.IsNullOrEmpty(apikey) || string.IsNullOrEmpty(chatgptModel) || string.IsNullOrEmpty(outputFolder) || string.IsNullOrEmpty(selectedFile))
    {
        Console.WriteLine("Missing required information");
        return;
    }

    var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Ensures correct directory
            .Build();

    // ✅ Register services using ServiceCollection
    var services = new ServiceCollection()
        .AddSingleton<IConfiguration>(config)  // Register configuration
        .AddSingleton<OpenAIService>()         // Register OpenAIService
        .BuildServiceProvider();

    var aiService = services.GetRequiredService<OpenAIService>();

    string classCode = await File.ReadAllTextAsync(selectedFile);
    string unitTestCode = await aiService.GenerateUnitTestAsync(classCode, apikey, chatgptModel);

    // Regex to extract the class name
    Match match = Regex.Match(unitTestCode, @"public\s+class\s+(\w+)");
    if (match.Success)
    {
        string className = match.Groups[1].Value; // Extracted class name
        string outputFilePath = Path.Combine(outputFolder, $"{className}.cs");
        await File.WriteAllTextAsync(outputFilePath, unitTestCode);

        Console.WriteLine( $"GENERATED_FILEPATH:{outputFilePath}"); // Output file path for VSIX to read
    }
}

