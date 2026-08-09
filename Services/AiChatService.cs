using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArogyaPulse.Api.DTOs;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;
using Microsoft.Extensions.Options;

namespace ArogyaPulse.Api.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AiSettings _settings;
        private readonly ILogger<AiChatService> _logger;

        private const string Disclaimer =
            "This assistant provides screening support only. " +
            "It does not diagnose disease, prescribe treatment, or replace a qualified healthcare professional.";

        public AiChatService(
            IPatientRepository patientRepository,
            IHttpClientFactory httpClientFactory,
            IOptions<AiSettings> settings,
            ILogger<AiChatService> logger)
        {
            _patientRepository = patientRepository;
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ChatResponseDto> GetGuidanceAsync(
            ChatDto request)
        {
            // Support both "message" and "query".
            var queryText =
                !string.IsNullOrWhiteSpace(request.Message)
                    ? request.Message
                    : request.Query;

            if (string.IsNullOrWhiteSpace(queryText))
            {
                return new ChatResponseDto
                {
                    Response = "Please enter a health-related question.",
                    Severity = "Warning",
                    Disclaimer = Disclaimer,
                    Timestamp = DateTime.UtcNow
                };
            }

            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                return new ChatResponseDto
                {
                    Response =
                        "The AI service is not configured. " +
                        "Please use the standard screening workflow.",
                    Severity = "Warning",
                    Disclaimer = Disclaimer,
                    Timestamp = DateTime.UtcNow
                };
            }

            string? patientContext = null;

            if (request.PatientId.HasValue)
            {
                var patient =
                    await _patientRepository.GetByIdAsync(
                        request.PatientId.Value);

                if (patient != null)
                {
                    patientContext =
                        $"Age: {patient.Age}; " +
                        $"Gender: {patient.Gender}; " +
                        $"Village: {patient.Village}; " +
                        $"BP: {patient.Bp}; " +
                        $"SpO2: {patient.SpO2}%; " +
                        $"Temperature: {patient.Temp:F1} C; " +
                        $"Glucose: {patient.Glucose} mg/dL; " +
                        $"Pregnancy explicitly recorded: {patient.IsPregnant}; " +
                        $"Current screening risk: {patient.RiskLevel} " +
                        $"({patient.RiskScore}/100).";
                }
                else
                {
                    _logger.LogWarning(
                        "Patient {PatientId} was requested for AI context but was not found.",
                        request.PatientId.Value);
                }
            }

            var systemPrompt = """
You are the ArogyaPulse ASHA Assistant.

Your job is to help a frontline health worker communicate patient information
clearly and safely.

IMPORTANT SAFETY RULES:
1. Never diagnose a disease.
2. Never invent patient information.
3. Never invent vital signs.
4. Never prescribe medication or dosage.
5. Never override the application's deterministic triage engine.
6. If required information is missing, ask for it.
7. Use simple language suitable for an ASHA worker.
8. Understand English and Hindi.
9. Return valid JSON only.
10. Treat patient context as sensitive information.
11. If the situation appears urgent, advise the ASHA worker to follow the
    application's triage result and contact an appropriate healthcare professional.
12. Do not claim that an AI response is a diagnosis.

Extract only information explicitly present in the user's message.

The JSON must contain:
language
intent
bp
spO2
temp
glucose
symptoms
missingInformation
assistantResponse

For unknown numeric values, use null.
For no symptoms, use [].
For no missing information, use [].

The assistantResponse must contain safe workflow guidance only.
""";

            if (!string.IsNullOrWhiteSpace(patientContext))
            {
                systemPrompt +=
                    "\nCurrent patient context:\n" +
                    patientContext;
            }

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = systemPrompt
                        }
                    }
                },

                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new
                            {
                                text = queryText
                            }
                        }
                    }
                },

                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 800,
                    response_mime_type = "application/json"
                }
            };

            var client =
                _httpClientFactory.CreateClient("Gemini");

            var url =
                $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent";

            using var httpRequest =
                new HttpRequestMessage(
                    HttpMethod.Post,
                    url);

            httpRequest.Headers.Add(
                "x-goog-api-key",
                _settings.ApiKey);

            httpRequest.Content =
                JsonContent.Create(requestBody);

            try
            {
                using var response =
                    await client.SendAsync(httpRequest);

                var responseText =
                    await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Gemini request failed. StatusCode: {StatusCode}, Response: {Response}",
                        response.StatusCode,
                        responseText);

                    return BuildFallback(
                        patientContext,
                        "The AI assistant is temporarily unavailable.");
                }

                var gemini =
                    JsonSerializer.Deserialize<GeminiResponse>(
                        responseText,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                var generatedText =
                    gemini?
                        .Candidates?
                        .FirstOrDefault()?
                        .Content?
                        .Parts?
                        .FirstOrDefault()?
                        .Text;

                if (string.IsNullOrWhiteSpace(generatedText))
                {
                    return BuildFallback(
                        patientContext,
                        "The AI assistant returned no usable response.");
                }

                generatedText =
                    CleanJson(generatedText);

                AiExtractionDto? extracted;

                try
                {
                    extracted =
                        JsonSerializer.Deserialize<AiExtractionDto>(
                            generatedText,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                }
                catch (JsonException ex)
                {
                    _logger.LogError(
                        ex,
                        "Gemini returned invalid JSON: {GeneratedText}",
                        generatedText);

                    return BuildFallback(
                        patientContext,
                        "The AI response could not be safely validated.");
                }

                if (extracted == null)
                {
                    return BuildFallback(
                        patientContext,
                        "The AI response could not be validated.");
                }

                var alerts = new List<string>();

                if (extracted.SpO2 is < 90)
                {
                    alerts.Add(
                        "Very low SpO2 was reported. Follow the application's triage protocol.");
                }

                if (!string.IsNullOrWhiteSpace(extracted.Bp))
                {
                    alerts.Add(
                        "Blood pressure information was extracted. Verify the measurement before clinical action.");
                }

                if (extracted.MissingInformation.Count > 0)
                {
                    alerts.Add(
                        "Additional patient information is required.");
                }

                return new ChatResponseDto
                {
                    Response =
                        string.IsNullOrWhiteSpace(
                            extracted.AssistantResponse)
                            ? "Please follow the standard screening workflow."
                            : extracted.AssistantResponse,

                    ClinicalAlerts = alerts,

                    ActionSteps = extracted.MissingInformation
                        .Select(x => $"Please provide: {x}")
                        .ToList(),

                    Severity =
                        DetermineSeverity(extracted),

                    PatientContext =
                        patientContext,

                    Disclaimer =
                        Disclaimer,

                    Timestamp =
                        DateTime.UtcNow
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(
                    ex,
                    "Network error while contacting Gemini.");

                return BuildFallback(
                    patientContext,
                    "The AI assistant could not connect to the AI service.");
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(
                    ex,
                    "Gemini request timed out.");

                return BuildFallback(
                    patientContext,
                    "The AI assistant request timed out. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "AI assistant request failed.");

                return BuildFallback(
                    patientContext,
                    "The AI assistant is temporarily unavailable.");
            }
        }

        private static string CleanJson(string text)
        {
            text = text.Trim();

            if (text.StartsWith("```"))
            {
                var firstNewLine =
                    text.IndexOf('\n');

                if (firstNewLine >= 0)
                {
                    text =
                        text[(firstNewLine + 1)..];
                }

                if (text.EndsWith("```"))
                {
                    text =
                        text[..^3];
                }
            }

            return text.Trim();
        }

        private static string DetermineSeverity(
            AiExtractionDto extraction)
        {
            if (extraction.SpO2 is < 90)
                return "Critical";

            if (extraction.MissingInformation.Count > 0)
                return "Warning";

            return "Info";
        }

        private static ChatResponseDto BuildFallback(
            string? patientContext,
            string message)
        {
            return new ChatResponseDto
            {
                Response = message,
                Severity = "Warning",
                PatientContext = patientContext,
                Disclaimer = Disclaimer,
                Timestamp = DateTime.UtcNow
            };
        }

        private sealed class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public List<GeminiCandidate>? Candidates { get; set; }
        }

        private sealed class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent? Content { get; set; }
        }

        private sealed class GeminiContent
        {
            [JsonPropertyName("parts")]
            public List<GeminiPart>? Parts { get; set; }
        }

        private sealed class GeminiPart
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}