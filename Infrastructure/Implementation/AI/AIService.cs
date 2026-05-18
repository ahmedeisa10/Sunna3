using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Tamkeen.Application.DTOs.AI;
using Tamkeen.Application.Interfaces.AI;

namespace Tamkeen.Infrastructure.Implementation.AI
{
    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;

        public AIService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _apiKey = config["Anthropic:ApiKey"]!;
        }

        public async Task<MaintenanceAdviceResponseDto> GetMaintenanceAdviceAsync(string problem)
        {
            var systemPrompt = """
                أنت مساعد صيانة منزلية خبير. مهمتك هي مساعدة المستأجرين بتقديم نصائح أولية بسيطة وآمنة قبل طلب فني.
                
                القواعد:
                - اكتب بالعربية فقط
                - قدّم 3 إلى 5 خطوات عملية بسيطة ومرقّمة
                - ابدأ دائماً بالأمان (افصل الكهرباء / أغلق المياه إن لزم)
                - إذا كانت المشكلة خطيرة، وضّح ذلك واطلب من المستأجر الاتصال بفني فوراً
                - لا تقترح حلولاً تتطلب مهارة متخصصة
                - اختم دائماً بجملة: "إذا استمرت المشكلة، يُنصح بإنشاء طلب صيانة."
                - لا تستخدم markdown (لا نجوم، لا هاشتاق)
                """;

            var body = new
            {
                model = "claude-opus-4-20250514",
                max_tokens = 1024,
                system = systemPrompt,
                messages = new[]
                {
                    new { role = "user", content = $"مشكلتي: {problem}" }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
            request.Headers.Add("x-api-key", _apiKey);
            request.Headers.Add("anthropic-version", "2023-06-01");
            request.Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? "لم أتمكن من الحصول على نصيحة.";

            return new MaintenanceAdviceResponseDto { Advice = text };
        }
    }
}