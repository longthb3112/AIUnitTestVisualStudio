using Microsoft.Build.Framework.XamlTypes;
using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;

namespace AIUnittestExtension.ToolWindows
{
    public class DotNetClassHandler: IUnitTestHandler
    {
        const string UNITTEST_TOKEN = "UNITTEST_TOKEN";
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";

        public async Task<string> GenerateUnitTestAsync(string filePath, string outPutFolderPath, string apiKey, string model)
        {
            List<string> methodChunks = ExtractPublicMethods(filePath);
            var templatefileText = ExtractClassTemplate(filePath);
            var className = ExtractClassName(filePath);
            if (string.IsNullOrEmpty(templatefileText))
            {
                return "Cannot proceed this file";
            }
            StringBuilder finalUnitTestContent = new StringBuilder();

            foreach (var methodChunk in methodChunks)
            {
                // Send method chunk to AI API
                string unitTest = await GenerateUnitTestForChunkAsync(methodChunk, apiKey, model, className);
                finalUnitTestContent.AppendLine(unitTest);
            }

            // Save final combined unit test file
            var combinedResult = templatefileText.Replace(UNITTEST_TOKEN, FormatSourceCodeWithIndentation(finalUnitTestContent.ToString()));
            var outPutFilePath = Path.Combine(outPutFolderPath, $"{className}Test.cs");
            File.WriteAllText(outPutFilePath, combinedResult);

            return outPutFilePath;
        }
        public async Task<string> GenerateUnitTestForChunkAsync(string methodCode, string apiKey, string model, string className)
        {
           var prompt = $"Generate unit test methods for the following C# method in class {className}:" +
                $"{methodCode}" +
                $"Requirements:" +
                $"- Use xUnit for testing." +
                $"- Use Moq to mock any database interactions or dependencies." +
                $"- Include both positive and negative test cases to verify expected behavior." +
                $"- Ensure proper assertions and exception handling." +
                $"- Do **not** include the namespace or class definition in the output." +
                $"- **Do not** generate test cases involving exceeding MinValue or MaxValue (e.g., MaxValue + 1, MinValue - 1)." +
                $"- Return **only** valid C# test methods, without enclosing them in a class." +
                $"- **Do not include** Markdown formatting (e.g., ` ```csharp` and ` ``` `)." +
                $"- Use `{className}` when calling the method under test.";
            try
            {
                var response = await CallChatGPTAsync(prompt, apiKey, model);
                return response;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }      
        public static async Task<string> CallChatGPTAsync(string prompt, string apiKey, string model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = model, 
                    messages = new[]
                    {
                    new { role = "system", content = "You are an AI expert in unit test that generates unit tests using Xunit and Moq for C# methods." },
                    new { role = "user", content = prompt }
                },
                    max_tokens = 1500
                };

                string json = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var parsedResponse = JsonConvert.DeserializeObject<ChatGPTResponse>(responseBody);
                return parsedResponse.Choices[0].Message.Content;
            }
        }
        static List<string> ExtractPublicMethods(string filePath)
        {
            string[] lines = File.ReadAllLines(filePath);
            List<string> methodChunks = new List<string>();
            StringBuilder currentMethod = new StringBuilder();
            bool insideMethod = false;
            int openBraces = 0;

            //Regex to match multi-line method signatures and ensure it's public
            Regex methodSignatureRegex = new Regex(@"^\s*(public|protected|internal)\s+(static\s+)?(\w+<.*?>|\w+)\s+\w+\s*\(.*");

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                //Detect method signature (allowing multi-line signatures)
                if (!insideMethod && methodSignatureRegex.IsMatch(trimmedLine))
                {
                    insideMethod = true;
                    openBraces = 0;
                    currentMethod.Clear();
                }

                if (insideMethod)
                {
                    currentMethod.AppendLine(line);

                    //Start counting braces **only after the first `{` appears**
                    if (trimmedLine.Contains("{"))
                    {
                        openBraces += CountOccurrences(trimmedLine, '{');
                    }
                    if (trimmedLine.Contains("}"))
                    {
                        openBraces -= CountOccurrences(trimmedLine, '}');
                    }

                    //If all `{` are matched with `}`, method is fully captured
                    if (openBraces == 0 && trimmedLine.Contains("}"))
                    {
                        methodChunks.Add(currentMethod.ToString().Trim()); // Store full method
                        insideMethod = false;
                    }
                }
            }

            return methodChunks;
        }
        static int CountOccurrences(string text, char character)
        {
            int count = 0;
            foreach (char c in text)
            {
                if (c == character) count++;
            }
            return count;
        }
        static string ExtractClassTemplate(string filePath)
        {
            var input = File.ReadAllText(filePath);

            // Extract all using statements
            string usingPattern = @"^\s*using\s+[A-Za-z0-9_.]+;\s*$";
            List<string> usingMatches = Regex.Matches(input, usingPattern, RegexOptions.Multiline)
                                     .Cast<Match>()  // Explicitly cast MatchCollection to IEnumerable<Match>
                                     .Select(m => m.Value.Trim()) // Extract value and trim spaces
                                     .ToList(); // Convert to List<string>

            string usings = string.Join("\n", usingMatches);
            // Extract namespace declaration
            Match namespaceMatch = Regex.Match(input, @"\bnamespace\s+[\w.]+\s*{", RegexOptions.Singleline);
            string namespaceDeclaration = namespaceMatch.Success ? namespaceMatch.Value : "";


            // Regex to match class declaration (with or without inheritance)
            Match match = Regex.Match(input, @"\bclass\s+(\w+)", RegexOptions.Singleline);
            if (!match.Success) return null;

            var className = match.Groups[1].Value;

            var template = usings + "\n";
            template += "using Xunit;\n";
            template += "using Moq;\n\n";
            template += namespaceDeclaration + "\n";
            template += $"    public class {className}Test\n";
            template += "    {\n";  
            template += $"{UNITTEST_TOKEN}\n";
            template += "    }\n";  
            template += "}\n";  

     
            return template;
        }
        static string ExtractClassName(string filePath)
        {
            var input = File.ReadAllText(filePath);
            Match match = Regex.Match(input, @"\bclass\s+(\w+)", RegexOptions.Singleline);
            if (!match.Success) return null;

            return match.Groups[1].Value;
        }
        static string FormatSourceCodeWithIndentation(string code)
        {
            //static string AutoIndent(string input, int spaces)
            //{
            //    string indent = new string(' ', spaces); // Create an indent of 4 spaces
            //    return string.Join("\n", input
            //        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None) // Split into lines
            //        .Select(line => indent + line.TrimEnd())); // Trim right and add indentation
            //}
            code = code.Replace("```csharp", "").Replace("```", "");
            string indent = new string(' ', 6); // Create an indent of 4 spaces
            return string.Join("\n", code
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None) // Split into lines
                .Select(line => indent + line.TrimEnd())); // Trim right and add indentation
            //using (var provider = new CSharpCodeProvider())
            //{
            //    var options = new CodeGeneratorOptions
            //    {
            //        IndentString = "    ", // 4 spaces
            //        BracingStyle = "C"
            //    };

            //    using (StringWriter writer = new StringWriter())
            //    {
            //        provider.GenerateCodeFromCompileUnit(new CodeSnippetCompileUnit(code), writer, options);
            //        return writer.ToString();
            //    }
            //}
        }
    } 
}
