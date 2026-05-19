using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Tamkeen.Application.DTOs.AI;
using Tamkeen.Application.Interfaces.AI;

namespace Tamkeen.Infrastructure.Implementation.AI
{
    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly string? _apiKey;
        private readonly ILogger<AIService> _logger;

        public AIService(HttpClient http, IConfiguration config, ILogger<AIService> logger)
        {
            _http = http;
            _logger = logger;
            _apiKey = config["Groq:ApiKey"];

            if (string.IsNullOrWhiteSpace(_apiKey))
                _logger.LogError("Groq API Key is MISSING!");
            else
                _logger.LogInformation("Groq API Key loaded OK");
        }

        public async Task<MaintenanceAdviceResponseDto> GetMaintenanceAdviceAsync(string problem)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_apiKey))
                    return new MaintenanceAdviceResponseDto { Advice = "API Key غير موجود" };

                var systemPrompt = """
                    أنت مساعد صيانة منزلية خبير. مهمتك تقديم نصائح أولية بسيطة وآمنة للمستأجرين قبل طلب فني.
                    القواعد:
                    - اكتب بالعربية فقط
                    - قدّم 3 إلى 5 خطوات عملية مرقّمة
                    - ابدأ بالأمان (افصل الكهرباء / أغلق المياه إن لزم)
                    - لو المشكلة خطيرة قول ذلك واطلب الاتصال بفني فوراً
                    - لا تقترح حلولاً تتطلب مهارة متخصصة
                    - اختم بجملة: "إذا استمرت المشكلة، يُنصح بإنشاء طلب صيانة."
                    - لا تستخدم markdown أو نجوم
                    """;

                var body = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user",   content = $"مشكلتي: {problem}" }
                    },
                    max_tokens = 1024,
                    temperature = 0.7
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
                request.Headers.Add("Authorization", $"Bearer {_apiKey}");
                request.Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _http.SendAsync(request);
                var json = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Groq status: {status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                    return new MaintenanceAdviceResponseDto { Advice = $"خطأ: {json}" };

                using var doc = JsonDocument.Parse(json);
                var text = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "لم أتمكن من الحصول على نصيحة.";

                return new MaintenanceAdviceResponseDto { Advice = text };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in GetMaintenanceAdviceAsync");
                return new MaintenanceAdviceResponseDto { Advice = $"حصل خطأ: {ex.Message}" };
            }
        }
        public async Task<MaintenanceScheduleResponseDto> GetMaintenanceScheduleAsync(MaintenanceScheduleRequestDto dto)
        {
            var devicesList = string.Join("\n", dto.Devices.Select(d => $"- {d.DeviceName}: عمره {d.AgeYears} سنوات"));

            var prompt = $$"""
        بناءً على أجهزة المنزل التالية:
        {{devicesList}}
        - آخر صيانة عامة: {{dto.LastMaintenanceDate}}

        اعمل جدول صيانة وقائية شامل لكل جهاز مذكور.
        لكل جهاز اعمل مهمة صيانة واحدة أو أكتر حسب احتياجه.
        رد بـ JSON فقط بالشكل ده:
        {
          "schedule": [
            {
              "device": "اسم الجهاز",
              "task": "المهمة المطلوبة بالتفصيل",
              "frequency": "كل كام شهر",
              "nextDue": "الشهر القادم مثلاً",
              "priority": "High/Medium/Low"
            }
          ]
        }
        لا تكتب أي كلام غير الـ JSON.
        """;

            var body = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]
                {
            new { role = "system", content = "أنت خبير صيانة منزلية. ردودك JSON فقط بدون أي نص إضافي." },
            new { role = "user", content = prompt }
        },
                max_tokens = 1024,
                temperature = 0.5
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            try
            {
                var cleanText = text.Replace("```json", "").Replace("```", "").Trim();
                var result = JsonSerializer.Deserialize<MaintenanceScheduleResponseDto>(cleanText,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result ?? new MaintenanceScheduleResponseDto { Schedule = new() };
            }
            catch
            {
                return new MaintenanceScheduleResponseDto { Schedule = new() };
            }
        }
    }
}