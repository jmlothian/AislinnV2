using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Aislinn.Core.Services;

public class AnthropicResponse
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Role { get; set; }
    public string Model { get; set; }
    public AnthropicContent[] Content { get; set; }

    [JsonPropertyName("stop_reason")]
    public string Stop_Reason { get; set; }

    [JsonPropertyName("stop_sequence")]
    public string Stop_Sequence { get; set; }

    public AnthropicUsage Usage { get; set; }
}

public class AnthropicContent
{
    public string Type { get; set; }
    public string Text { get; set; }
}

public class AnthropicUsage
{
    [JsonPropertyName("input_tokens")]
    public int Input_Tokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int Output_Tokens { get; set; }
}

public class LLMApiService
{

    public string LLMService { get; set; } = "openai";
    private readonly HttpClient _httpClient;
    private readonly RainaConfiguration _config;
    public LLMApiService(RainaConfiguration config)
    {
        _config = config;
        _httpClient = new HttpClient();
    }
    public async Task<string> CallLLM(string systemprompt, string prompt, string agentName, bool json, List<Utterance> history = null, double temperature = 0.7, int maxTokens = 6000)
    {
        //remove any api keys that dont belong
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Remove("x-api-key");

        if (LLMService == "openai")
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.OpenAIApiKey}");
            return await this.CallOpenAIAsync(systemprompt, prompt, agentName, json, history, temperature, maxTokens);
        }
        else if (LLMService == "anthropic")
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", $"{_config.AnthropicKey}");
            return await this.CallAnthropicAsync(systemprompt, prompt, agentName, json, history, temperature, maxTokens);
        }
        else if (LLMService == "local")
        {
            return "";
        }
        else
        {
            return await this.CallOpenAIAsync(systemprompt, prompt, agentName, json, history, temperature, maxTokens);
        }

    }
    private async Task<string> CallAnthropicAsync(string systemprompt, string prompt, string agentName, bool json, List<Utterance> history = null, double temperature = 0.7, int maxTokens = 6000)
    {
        StringContent content;

        // Build messages array
        List<PromptMessage> messages = new List<PromptMessage>();

        if (history != null)
        {
            foreach (var mesg in history)
            {
                if (mesg.Speaker == agentName)
                {
                    messages.Add(new PromptMessage() { role = "assistant", content = mesg.Text });
                }
                else
                {
                    messages.Add(new PromptMessage() { role = "user", content = mesg.Text });
                }
            }
        }
        messages.Add(new PromptMessage() { role = "user", content = prompt });

        var requestBody = new
        {
            model = "claude-sonnet-4-20250514",
            max_tokens = maxTokens,
            temperature = temperature,
            system = systemprompt,
            messages = messages
        };

        content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync("https://api.anthropic.com/v1/messages", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<AnthropicResponse>(responseString);
        return responseObject.Content[0].Text;
    }
    private async Task<string> CallLocalAsync(string systemprompt, string prompt, string agentName, bool json, List<Utterance> history = null, double temperature = 0.7, int maxTokens = 6000)
    {
        return "";
    }
    private async Task<string> CallOpenAIAsync(string systemprompt, string prompt, string agentName, bool json, List<Utterance> history = null, double temperature = 0.7, int maxTokens = 6000)
    {
        StringContent content;
        if (json)
        {
            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = systemprompt },
                    new { role = "user", content = prompt }
                },
                temperature = temperature,
                max_tokens = maxTokens,
                response_format = new
                {
                    type = "json_object"
                }
            };

            content = new StringContent(
               JsonSerializer.Serialize(requestBody),
               Encoding.UTF8,
               "application/json");
        }
        else
        {
            List<PromptMessage> messages = new List<PromptMessage>() { new PromptMessage() { role = "system", content = systemprompt } };
            if (history != null)
            {
                foreach (var mesg in history)
                {
                    if (mesg.Speaker == agentName)
                    {
                        messages.Add(new PromptMessage() { role = "assistant", content = mesg.Text });
                    }
                    else
                    {
                        messages.Add(new PromptMessage() { role = "user", content = mesg.Text });
                    }
                }
            }
            messages.Add(new PromptMessage() { role = "user", content = prompt });
            var requestBody = new
            {
                model = "gpt-4o",
                messages = messages,
                temperature = temperature,
                max_tokens = 6000
            };

            content = new StringContent(
               JsonSerializer.Serialize(requestBody),
               Encoding.UTF8,
               "application/json");
        }
        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var responseObject = JsonSerializer.Deserialize<OpenAIResponse>(responseString);
        return responseObject.Choices[0].Message.Content;
    }
}