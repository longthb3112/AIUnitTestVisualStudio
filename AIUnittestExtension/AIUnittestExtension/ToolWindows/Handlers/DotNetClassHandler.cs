using AIUnittestExtension.ToolWindows.AI;
using AIUnittestExtension.ToolWindows.Helpers;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows
{
    public class DotNetClassHandler : IUnitTestHandler
    {
        const string UNITTEST_TOKEN = "UNITTEST_TOKEN";
        private IAI _iAI;
       
        public async Task<string> GenerateUnitTestAsync(string filePath, string outPutFolderPath, string apiKey, string model, string inputPrompt)
        {
            var methodChunks = DotnetClassHelper.ExtractPublicMethods(filePath);
            var className = DotnetClassHelper.ExtractClassName(filePath);
            var templatefileText = DotnetClassHelper.ExtractClassTemplate(filePath , UNITTEST_TOKEN);

            if (string.IsNullOrEmpty(templatefileText))
            {
                return "Cannot proceed this file";
            }

            //merge chunks to bucket to reduce AI requests 
            var buckets = DotnetClassHelper.Bucketize(methodChunks, ( s => s.Split(new[] { "\r\n" }, StringSplitOptions.None).Length - 1), 150);
            var finalUnitTestContent = new StringBuilder();

            foreach (var key in buckets.Keys)
            {
                string unitTest = await GenerateUnitTestForChunkAsync(string.Join("\n", buckets[key]), apiKey, model, className, inputPrompt);
                finalUnitTestContent.AppendLine(unitTest);
            }
          
            // Save final combined unit test file
            var combinedResult = DotnetClassHelper.FormatCSharpCode(RelaceToken(templatefileText, finalUnitTestContent.ToString(), UNITTEST_TOKEN));
            var outPutFilePath = Path.Combine(outPutFolderPath, $"{className}Test.cs");
            File.WriteAllText(outPutFilePath, combinedResult);

            return $"Generated Unit Test File: {outPutFilePath}";
        }
        static string RelaceToken(string templatefileText,string content, string TOKEN)
        {
            return templatefileText.Replace(TOKEN, content)
                            .Replace("```csharp", "")
                            .Replace("```", "");
                            
        }
        public async Task<string> GenerateUnitTestForChunkAsync(string methodCode, string apiKey, string model, string className, string inputPrompt)
        {
            var prompt = "Generate unit test methods for the following C# method in class {className}:" +
            "{methodCode}" +
            $"\nRequirements:" +
            inputPrompt.Replace("{methodCode}", methodCode).Replace("{className}", className);
            try
            {
                var response = await CallAIAsync(prompt, apiKey, model);
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<string> CallAIAsync(string prompt, string apiKey, string model)
        {
            _iAI = new ChatGPT();
            return await _iAI.ExecuteAsync(prompt, apiKey, model);
        }
    }
}
