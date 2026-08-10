using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ArogyaPulse.Api.DTOs;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;
using Microsoft.Extensions.Options;

namespace ArogyaPulse.Api.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ITriageService _triageService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AiSettings _settings;
        private readonly ILogger<AiChatService> _logger;

        private const string Disclaimer =
            "This assistant provides screening support only. " +
            "It does not diagnose disease, prescribe treatment, or replace a qualified healthcare professional.";

        public AiChatService(
            IPatientRepository patientRepository,
            ITriageService triageService,
            IHttpClientFactory httpClientFactory,
            IOptions<AiSettings> settings,
            ILogger<AiChatService> logger)
        {
            _patientRepository = patientRepository;
            _triageService = triageService;
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<ChatResponseDto> GetGuidanceAsync(ChatDto request)
        {
            var queryText = !string.IsNullOrWhiteSpace(request.Message)
                ? request.Message
                : request.Query;

            if (string.IsNullOrWhiteSpace(queryText))
            {
                return new ChatResponseDto
                {
                    Response = "Please enter a health-related question or patient observations.",
                    LanguageDetected = "English",
                    Severity = "Warning",
                    Disclaimer = Disclaimer,
                    Timestamp = DateTime.UtcNow
                };
            }

            string? patientContext = null;
            bool isPregnant = false;

            if (request.PatientId.HasValue)
            {
                var patient = await _patientRepository.GetByIdAsync(request.PatientId.Value);
                if (patient != null)
                {
                    isPregnant = patient.IsPregnant;
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

            // Check if query mentions pregnancy keywords
            if (!isPregnant)
            {
                var lower = queryText.ToLowerInvariant();
                if (lower.Contains("pregnant") || lower.Contains("pregnancy") || queryText.Contains("गर्भवती") || queryText.Contains("गर्भ"))
                {
                    isPregnant = true;
                }
            }

            AiExtractionDto? extracted = null;

            // 1. Attempt Gemini Call if API Key is set
            if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                extracted = await CallGeminiAsync(queryText, patientContext);
            }

            // 2. Fallback NLP Extractor if Gemini unavailable or failed
            if (extracted == null)
            {
                extracted = FallbackExtract(queryText);
            }

            // Standardize Language
            string language = string.IsNullOrWhiteSpace(extracted.Language) || extracted.Language.Equals("unknown", StringComparison.OrdinalIgnoreCase)
                ? DetectLanguage(queryText)
                : extracted.Language;

            // Build Extracted Vitals
            bool hasVitalInQuery = !string.IsNullOrWhiteSpace(extracted.Bp) ||
                                   extracted.SpO2.HasValue ||
                                   extracted.Temp.HasValue ||
                                   extracted.Glucose.HasValue;

            ExtractedVitalsDto? vitalsDto = null;
            TriageResultDto? triageEvaluation = null;

            if (hasVitalInQuery)
            {
                vitalsDto = new ExtractedVitalsDto
                {
                    Bp = extracted.Bp,
                    SpO2 = extracted.SpO2 ?? 0,
                    Temp = extracted.Temp ?? 0,
                    Glucose = extracted.Glucose ?? 0
                };

                // Run automated triage evaluation engine
                string bpForTriage = !string.IsNullOrWhiteSpace(extracted.Bp) ? extracted.Bp : "120/80";
                int spO2ForTriage = extracted.SpO2 ?? 98;
                double tempForTriage = extracted.Temp ?? 37.0;
                int glucoseForTriage = extracted.Glucose ?? 100;

                triageEvaluation = _triageService.EvaluateTriage(
                    bpForTriage, spO2ForTriage, tempForTriage, glucoseForTriage, isPregnant);
            }

            // Build alerts and action steps
            var alerts = new List<string>();
            var actionSteps = new List<string>();

            if (extracted.SpO2 is < 90)
            {
                alerts.Add("Critical SpO2 (< 90%) detected. Urgent clinical assessment & oxygen therapy protocol required.");
            }

            if (!string.IsNullOrWhiteSpace(extracted.Bp))
            {
                alerts.Add($"Blood pressure ({extracted.Bp}) extracted. Verify measurement before taking clinical action.");
            }

            if (triageEvaluation != null)
            {
                foreach (var rec in triageEvaluation.ActionRecommendations)
                {
                    if (!actionSteps.Contains(rec)) actionSteps.Add(rec);
                }
            }

            if (extracted.MissingInformation != null && extracted.MissingInformation.Count > 0)
            {
                foreach (var missing in extracted.MissingInformation)
                {
                    actionSteps.Add($"Please collect: {missing}");
                }
            }

            // Severity determination
            string severity = "Info";
            if (extracted.SpO2 is < 90 || (triageEvaluation != null && triageEvaluation.RiskLevel == "High"))
            {
                severity = "Critical";
            }
            else if ((triageEvaluation != null && triageEvaluation.RiskLevel == "Medium") || (extracted.MissingInformation != null && extracted.MissingInformation.Count > 0))
            {
                severity = "Warning";
            }

            string responseText = string.IsNullOrWhiteSpace(extracted.AssistantResponse)
                ? "Please follow standard screening protocols."
                : extracted.AssistantResponse;

            return new ChatResponseDto
            {
                Response = responseText,
                LanguageDetected = language,
                ExtractedVitals = vitalsDto,
                ExtractedSymptoms = extracted.Symptoms ?? new List<string>(),
                TriageEvaluation = triageEvaluation,
                ClinicalAlerts = alerts,
                ActionSteps = actionSteps,
                Severity = severity,
                PatientContext = patientContext,
                Disclaimer = Disclaimer,
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<AiExtractionDto?> CallGeminiAsync(string queryText, string? patientContext)
        {
            var systemPrompt = """
You are the ArogyaPulse ASHA Assistant. Your job is to extract vital signs, symptoms, and language from ASHA worker inputs and provide safe screening guidance.

IMPORTANT SAFETY RULES:
1. Never diagnose a disease.
2. Never invent patient information or vital signs.
3. Never prescribe medication or dosage.
4. Understand English and Hindi.
5. Return valid JSON ONLY matching the required format.

JSON format:
{
  "language": "English" or "Hindi",
  "intent": "triage_support" or "question",
  "bp": "120/80" or null,
  "spO2": number or null,
  "temp": number in Celsius or null,
  "glucose": number in mg/dL or null,
  "symptoms": ["symptom1", "symptom2"],
  "missingInformation": ["missing info if any"],
  "assistantResponse": "Clear, safe guidance for the ASHA worker."
}
""";

            if (!string.IsNullOrWhiteSpace(patientContext))
            {
                systemPrompt += "\nCurrent patient context:\n" + patientContext;
            }

            var requestBody = new
            {
                system_instruction = new
                {
                    parts = new[] { new { text = systemPrompt } }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = queryText } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 800,
                    response_mime_type = "application/json"
                }
            };

            try
            {
                var client = _httpClientFactory.CreateClient("Gemini");
                var url = $"{_settings.BaseUrl}/models/{_settings.Model}:generateContent";

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                httpRequest.Headers.Add("x-goog-api-key", _settings.ApiKey);
                httpRequest.Content = JsonContent.Create(requestBody);

                using var response = await client.SendAsync(httpRequest);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini request failed ({StatusCode}): {ResponseText}", response.StatusCode, responseText);
                    return null;
                }

                var gemini = JsonSerializer.Deserialize<GeminiResponse>(responseText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var generatedText = gemini?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
                if (string.IsNullOrWhiteSpace(generatedText)) return null;

                generatedText = CleanJson(generatedText);

                return JsonSerializer.Deserialize<AiExtractionDto>(generatedText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini API call failed.");
                return null;
            }
        }

        private static AiExtractionDto FallbackExtract(string queryText)
        {
            var language = DetectLanguage(queryText);
            var result = new AiExtractionDto
            {
                Language = language,
                Intent = "screening_support"
            };

            // 1. Blood Pressure Regex: 120/80, 160/100, etc.
            var bpMatch = Regex.Match(queryText, @"\b(?<sys>\d{2,3})\s*[/:\-]\s*(?<dia>\d{2,3})\b");
            if (bpMatch.Success)
            {
                result.Bp = $"{bpMatch.Groups["sys"].Value}/{bpMatch.Groups["dia"].Value}";
            }

            // 2. SpO2 Regex: 88%, spo2 92, spo2 is 88%, oxygen 85
            var spo2Match = Regex.Match(queryText, @"\b(?:sp[oO]2|spo2|oxygen|ऑक्सीजन)\s*(?:is|=|:|-)?\s*(?<val>\d{2,3})%?\b|\b(?<val>\d{2,3})\s*%\s*(?:spo2|oxygen)?\b", RegexOptions.IgnoreCase);
            if (spo2Match.Success && int.TryParse(spo2Match.Groups["val"].Value, out var spo2Val) && spo2Val >= 50 && spo2Val <= 100)
            {
                result.SpO2 = spo2Val;
            }

            // 3. Temperature Regex: 101 F, 38.5 C, temp is 39, बुखार 102
            var tempMatch = Regex.Match(queryText, @"\b(?:temp|temperature|fever|बुखार|तापमान)\s*(?:is|=|:|-)?\s*(?<val>\d{2,3}(?:\.\d)?)\s*(?<unit>c|f|°c|°f)?\b|\b(?<val>\d{2,3}(?:\.\d)?)\s*(?<unit>°c|°f)\b", RegexOptions.IgnoreCase);
            if (tempMatch.Success && double.TryParse(tempMatch.Groups["val"].Value, out var tempVal))
            {
                var unit = tempMatch.Groups["unit"].Value.ToLowerInvariant();
                if (unit.Contains('f') || tempVal > 50)
                {
                    // Convert Fahrenheit to Celsius
                    tempVal = (tempVal - 32.0) * 5.0 / 9.0;
                }
                if (tempVal >= 30.0 && tempVal <= 45.0)
                {
                    result.Temp = Math.Round(tempVal, 1);
                }
            }

            // 4. Glucose Regex: glucose 200, sugar is 180, 210 mg/dl
            var glucoseMatch = Regex.Match(queryText, @"\b(?:glucose|sugar|शुगर|ग्लूकोज)\s*(?:is|=|:|-)?\s*(?<val>\d{2,3})\s*(?:mg/dl)?\b|\b(?<val>\d{2,3})\s*mg/dl\b", RegexOptions.IgnoreCase);
            if (glucoseMatch.Success && int.TryParse(glucoseMatch.Groups["val"].Value, out var glucoseVal) && glucoseVal >= 20 && glucoseVal <= 600)
            {
                result.Glucose = glucoseVal;
            }

            // 5. Symptoms Extraction
            var symptoms = new List<string>();
            var lowerQuery = queryText.ToLowerInvariant();

            var symptomPatterns = new Dictionary<string, string[]>
            {
                { "Shortness of breath / सांस फूलना", new[] { "breath", "breathing", "saans", "saas", "सांस", "फूल रही", "shortness of breath" } },
                { "Chest pain / छाती में दर्द", new[] { "chest pain", "chati me dard", "छाती में दर्द", "सीने में दर्द" } },
                { "Fever / बुखार", new[] { "fever", "bukhar", "बुखार", "ताप" } },
                { "Cough / खांसी", new[] { "cough", "khansi", "खांसी" } },
                { "Headache / सिरदर्द", new[] { "headache", "sirdard", "सिरदर्द", "सर दर्द" } },
                { "Vomiting / उल्टी", new[] { "vomit", "vomiting", "ulti", "उल्टी" } },
                { "Dizziness / चक्कर", new[] { "dizzy", "dizziness", "chakkan", "ch चक्कर" } },
                { "Fatigue / थकान", new[] { "fatigue", "thakan", "थकान", "कमजोरी" } }
            };

            foreach (var kvp in symptomPatterns)
            {
                if (kvp.Value.Any(kw => lowerQuery.Contains(kw)))
                {
                    symptoms.Add(kvp.Key);
                }
            }

            result.Symptoms = symptoms;

            // 6. Generate Clinical Guidance Response
            var guidance = new List<string>();
            if (language == "Hindi")
            {
                guidance.Add("आशा कार्यकर्ता के लिए दिशानिर्देश:");
                if (result.SpO2.HasValue && result.SpO2 < 90)
                {
                    guidance.Add("⚠️ चेतावनी: SpO2 बहुत कम (90% से कम) है! मरीज को तुरंत अस्पताल/स्वास्थ्य केंद्र (CHCs/PHCs) भेजें और ऑक्सीजन सहायता प्रदान करें।");
                }
                if (!string.IsNullOrWhiteSpace(result.Bp))
                {
                    guidance.Add($"🩺 बीपी दर्ज़: {result.Bp}। यदि सिस्टोलिक 140+ या डायस्टोलिक 90+ है, तो दोबारा मापें और डॉक्टर को सूचित करें।");
                }
                if (symptoms.Count > 0)
                {
                    guidance.Add($"📋 पहचाने गए लक्षण: {string.Join(", ", symptoms)}। मरीज के महत्वपूर्ण संकेतों (Vitals) की निगरानी करें।");
                }
                if (guidance.Count == 1)
                {
                    guidance.Add("आरोग्यपल्स प्रोटोकॉल के अनुसार मरीज के सभी महत्वपूर्ण संकेत (BP, SpO2, Temp, Glucose) दर्ज़ करें।");
                }
            }
            else
            {
                guidance.Add("ASHA Clinical Guidance Protocol:");
                if (result.SpO2.HasValue && result.SpO2 < 90)
                {
                    guidance.Add("⚠️ WARNING: Very low SpO2 (< 90%). Immediate referral to a healthcare facility and oxygen administration is required.");
                }
                if (!string.IsNullOrWhiteSpace(result.Bp))
                {
                    guidance.Add($"🩺 Blood Pressure: {result.Bp}. If Systolic >= 140 or Diastolic >= 90, recheck reading and arrange medical evaluation.");
                }
                if (symptoms.Count > 0)
                {
                    guidance.Add($"📋 Symptoms Identified: {string.Join(", ", symptoms)}. Continue vital signs monitoring.");
                }
                if (guidance.Count == 1)
                {
                    guidance.Add("Record full vital signs (BP, SpO2, Temp, Glucose) for complete screening scoring.");
                }
            }

            result.AssistantResponse = string.Join("\n\n", guidance);
            return result;
        }

        private static string DetectLanguage(string text)
        {
            if (Regex.IsMatch(text, @"[\u0900-\u097F]"))
            {
                return "Hindi";
            }

            var hindiWords = new[] { "मरीज", "बीपी", "सांस", "बुखार", "दर्द", "उल्टी", "चक्कर", "है", "का", "की", "में", "को", "bukhar", "khansi", "saans", "hai", "ka", "ki" };
            var lower = text.ToLowerInvariant();
            if (hindiWords.Any(w => lower.Contains(w)))
            {
                return "Hindi";
            }

            return "English";
        }

        private static string CleanJson(string text)
        {
            text = text.Trim();
            if (text.StartsWith("```"))
            {
                var firstNewLine = text.IndexOf('\n');
                if (firstNewLine >= 0)
                {
                    text = text[(firstNewLine + 1)..];
                }
                if (text.EndsWith("```"))
                {
                    text = text[..^3];
                }
            }
            return text.Trim();
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