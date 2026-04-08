using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafetyCompliance.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SafetyCompliance.Services
{
    public class GroqApiClient : IAiClient
    {
        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        private const string API_URL = "https://api.groq.com/openai/v1/chat/completions";

        private readonly string _apiKey;
        private readonly string _model;

        public GroqApiClient()
        {
            _apiKey = ConfigurationManager.AppSettings["GroqApiKey"];
            _model = ConfigurationManager.AppSettings["GroqModel"] ?? "llama3-70b-8192";
        }

        public async Task<AuditResult> AnalyzeComplianceAsync(ComplianceData data)
        {
            try
            {
                var prompt = PromptBuilder.Build(data);

                var requestBody = new
                {
                    model = _model,
                    messages = new[] {
                    new { role = "system", content = "You are a professional Safety Auditor. Return ONLY valid JSON." },
                    new { role = "user", content = prompt }
                },
                    temperature = 0.1,
                    response_format = new { type = "json_object" }
                };

                var request = new HttpRequestMessage(HttpMethod.Post, API_URL);
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

                var response = await _http.SendAsync(request);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Groq API Error: {response.StatusCode} - {responseJson}");

                var root = JObject.Parse(responseJson);
                var textContent = root["choices"]?[0]?["message"]?["content"]?.ToString();

                if (string.IsNullOrEmpty(textContent))
                    return CreateEmptyResult("AI returned no content.");

                return JsonConvert.DeserializeObject<AuditResult>(textContent)
                       ?? CreateEmptyResult("Failed to parse JSON.");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to parse AI response. Ensure the AI returned the 'Gaps' list. Error: " + ex.Message);
            }
        }

        private AuditResult CreateEmptyResult(string message)
        {
            return new AuditResult
            {
                Summary = message,
                ReadinessScore = 0,
                Gaps = new List<ComplianceGap>()
            };
        }
    }
}