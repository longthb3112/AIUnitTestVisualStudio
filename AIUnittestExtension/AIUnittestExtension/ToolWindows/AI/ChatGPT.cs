using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AIUnittestExtension.ToolWindows.AI
{
    public interface IAI
    {
         Task<string> ExecuteAsync(string prompt, string apiKey, string model);
    }
    public class ChatGPT: IAI
    {
        private static readonly string apiUrl = "https://api.openai.com/v1/chat/completions";
        public  async Task<string> ExecuteAsync(string prompt, string apiKey, string model)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var requestBody = new
                {
                    model = model,
                    messages = new[]
                    {
                    new { role = "system", content = "You are an AI expert in unit test that generates unit tests for C# methods." },
                    new { role = "user", content = prompt }
                },
                    max_tokens = 1500,
                    temperature = 0.2, // Lower value for more precise and deterministic responses
                    top_p = 0.9,       // Keep diversity but maintain focus
                    frequency_penalty = 0, // Avoid discouraging common patterns
                    presence_penalty = 0  // Avoid unnecessary variations
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
    }
}
