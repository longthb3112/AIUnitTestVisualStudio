using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows.AI
{
    public class Gemini : IAI
    {
        public async Task<string> ExecuteAsync(string prompt, string apiKey, string model)
        {
            string apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

            using (HttpClient client = new HttpClient())
            {
                var requestData = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        maxOutputTokens = 1500,
                        temperature = 0.2,
                        topP = 0.9,
                        topK = 40,
                        candidateCount = 1,
                        frequencyPenalty = 0.0,
                        presencePenalty = 0.0
                    }
                };

                string jsonRequest = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JsonNode root = JsonNode.Parse(jsonResponse);

                // Extract the first "parts" text
                string codeBlock = root["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                return codeBlock;
            }
        }
    }
}
