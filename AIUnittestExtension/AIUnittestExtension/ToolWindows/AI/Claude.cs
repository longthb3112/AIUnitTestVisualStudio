using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows.AI
{
    public class Claude : IAI
    {
        public async Task<string> ExecuteAsync(string prompt, string apiKey, string model)
        {
            string apiUrl = "https://api.anthropic.com/v1/messages";
            using (HttpClient client = new HttpClient())
            {
                // Set up Claude API headers
                client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

                var requestData = new
                {
                    model = model,
                    max_tokens = 1500,
                    temperature = 0.2,
                    top_p = 0.9,
                    top_k = 40,
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
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

                // Extract the response content
                string responseText = root["content"]?[0]?["text"]?.ToString();
                return responseText;
            }
        }
    }
}
