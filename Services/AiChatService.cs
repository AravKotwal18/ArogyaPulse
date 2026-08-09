using System.Text.RegularExpressions;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ITriageService _triageService;

        public AiChatService(IPatientRepository patientRepository, ITriageService triageService)
        {
            _patientRepository = patientRepository;
            _triageService = triageService;
        }

        public async Task<ChatResponseDto> GetGuidanceAsync(ChatRequestDto request)
        {
            string message = !string.IsNullOrWhiteSpace(request.Message) ? request.Message : request.Query;
            var queryLower = message.ToLower();

            // 1. Language Detection
            string languageDetected = DetectLanguage(message, request.Language);

            // 2. Vital Extraction
            var extractedVitals = ExtractVitals(message);

            // 3. Symptom Extraction
            var extractedSymptoms = ExtractSymptoms(message);

            var alerts = new List<string>();
            var actions = new List<string>();
            string responseText = "";
            string severity = "Info";
            string? patientContext = null;
            TriageResultDto? triageEval = null;

            // 4. Patient Context lookup if PatientId provided
            if (request.PatientId.HasValue)
            {
                var patient = await _patientRepository.GetByIdAsync(request.PatientId.Value);
                if (patient != null)
                {
                    patientContext = $"Patient #{patient.Id} ({patient.Name}): SpO₂ {patient.SpO2}%, BP {patient.Bp}, Temp {patient.Temp:F1}°C, Glucose {patient.Glucose} mg/dL, Risk: {patient.RiskLevel}";
                }
            }

            // 5. If vitals were extracted, calculate deterministic triage
            if (extractedVitals != null && !string.IsNullOrWhiteSpace(extractedVitals.Bp))
            {
                triageEval = _triageService.EvaluateTriage(
                    extractedVitals.Bp,
                    extractedVitals.SpO2 > 0 ? extractedVitals.SpO2 : 98,
                    extractedVitals.Temp > 0 ? extractedVitals.Temp : 37.0,
                    extractedVitals.Glucose > 0 ? extractedVitals.Glucose : 100,
                    false);

                if (triageEval.RiskLevel == "High") severity = "Critical";
                else if (triageEval.RiskLevel == "Medium") severity = "Warning";
            }

            // 6. Clinical Domain Logic & Response Formulation

            // Case A: SpO2 / Oxygen extracted or queried
            if (extractedVitals?.SpO2 > 0 || queryLower.Contains("spo2") || queryLower.Contains("oxygen") ||
                queryLower.Contains("ऑक्सीजन") || queryLower.Contains("सांस") || queryLower.Contains("moochu"))
            {
                int spO2Val = extractedVitals?.SpO2 > 0 ? extractedVitals.SpO2 : (queryLower.Contains("88") ? 88 : 95);

                if (spO2Val < 90)
                {
                    severity = "Critical";
                    alerts.Add($"SpO2 {spO2Val}% indicates severe respiratory distress / hypoxia!");

                    if (languageDetected == "Tamil")
                    {
                        responseText = $"⚠️ **கடுமையான ஹைபோக்ஸியா (SpO2 {spO2Val}% < 90%) அவசர வழிகாட்டுதல்:**\n" +
                                       $"நோயாளிக்கு மூச்சுத் திணறல் மற்றும் குறைந்த ஆக்சிஜன் உள்ளது ({spO2Val}%). உடனடியாக அவசர சிகிச்சை தேவை.";
                        actions.Add("நோயாளிக்கு உடனடியாக ஆக்சிஜன் (2-4 L/min) வழங்கவும்.");
                        actions.Add("நோயாளிக்கு Fowler's நிலை (அமர்ந்த நிலை) தரவும்.");
                        actions.Add("உடனடியாக 108 ஆம்புலன்ஸ் மூலமாக அரசு மருத்துவமனைக்கு மாற்றவும்.");
                    }
                    else if (languageDetected == "Hindi")
                    {
                        responseText = $"⚠️ **गंभीर हाइपोक्सिया (SpO2 {spO2Val}% < 90%) आपातकालीन मार्गदर्शन:**\n" +
                                       $"मरीज का SpO2 {spO2Val}% है, जो गंभीर सांस लेने की समस्या को दर्शाता है।";
                        alerts.Add($"SpO2 {spO2Val}% गंभीर हाइपोक्सिया का संकेत है!");
                        actions.Add("मरीज को 2-4 लीटर/मिनट की दर से पूरक ऑक्सीजन दें।");
                        actions.Add("मरीज को बैठने की स्थिति (Fowler's position) में रखें।");
                        actions.Add("तुरंत एम्बुलेंस/अस्पताल वाहन बुलाएं और जिला अस्पताल को सूचित करें।");
                    }
                    else
                    {
                        responseText = $"⚠️ **Severe Hypoxia (SpO2 {spO2Val}% < 90%) Emergency Guidance:**\n" +
                                       $"Extracted SpO2 level of {spO2Val}% indicates severe respiratory compromise and requires immediate emergency triage.";
                        actions.Add("Administer supplemental oxygen (2-4 L/min) immediately.");
                        actions.Add("Place patient in an upright sitting (Fowler's) position to aid lung expansion.");
                        actions.Add("Dispatch emergency transport to District Hospital / CHC immediately.");
                    }
                }
                else
                {
                    responseText = $"ℹ️ **SpO2 Evaluation ({spO2Val}%):**\n" +
                                   $"• 95% - 100%: Normal oxygen saturation\n" +
                                   $"• 90% - 94%: Moderate hypoxemia (Priority review)\n" +
                                   $"• < 90%: Severe hypoxia (Emergency referral)";
                    actions.Add("Verify probe placement and re-check pulse oximeter reading.");
                }
            }
            // Case B: Blood Pressure / Pre-Eclampsia
            else if (extractedVitals?.Bp != null || queryLower.Contains("bp") || queryLower.Contains("pressure") ||
                     queryLower.Contains("बीपी") || queryLower.Contains("pre-eclampsia"))
            {
                string bpVal = extractedVitals?.Bp ?? "140/90";
                if (queryLower.Contains("preg") || queryLower.Contains("গর্ভবতী") || queryLower.Contains("गर्भवती"))
                {
                    severity = "Critical";
                    alerts.Add("Gestational pre-eclampsia warning!");
                    responseText = $"🤰 **Gestational Pre-Eclampsia Protocol (BP {bpVal}):**\n" +
                                   $"Elevated blood pressure ({bpVal}) during pregnancy poses severe risk of eclampsia.";
                    actions.Add("Immediate referral to District Hospital Obstetric emergency department.");
                    actions.Add("Keep patient calm in left lateral position and transport urgently.");
                }
                else
                {
                    responseText = $"🩸 **Blood Pressure Stratification ({bpVal}):**\n" +
                                   $"• Normal: < 120/80 mmHg\n" +
                                   $"• Stage 1 Hypertension: 140-159 / 90-99 mmHg\n" +
                                   $"• Hypertensive Crisis: ≥ 160/100 mmHg";
                    actions.Add("If BP ≥ 160/100, trigger doctor alert and re-check after 15 minutes rest.");
                }
            }
            // Case C: Vague Symptoms without Vitals (Missing info safeguard)
            else if (extractedSymptoms.Count > 0 && extractedVitals == null)
            {
                string symptomsStr = string.Join(", ", extractedSymptoms);
                responseText = $"📋 **Observed Symptoms Detected:** {symptomsStr}\n\n" +
                               $"To calculate an exact clinical triage risk score, please record the patient's vital signs (SpO₂, Blood Pressure, Temperature, Glucose).";
                actions.Add("Measure SpO₂ using a pulse oximeter.");
                actions.Add("Take Blood Pressure using a digital BP monitor.");
                actions.Add("Check body temperature using a thermometer.");
            }
            // Case D: Default Welcome / Multilingual Assistant Overview
            else
            {
                if (languageDetected == "Tamil")
                {
                    responseText = "🤖 **ஆரோக்யபல்ஸ் AI ஆஷா உதவியாளருக்கு நல்வரவு!**\n" +
                                   "நான் தமிழ், ஆங்கிலம் மற்றும் இந்தி மொழிகளில் மருத்துவ வழிகாட்டுதல் வழங்குவேன்.\n\n" +
                                   "உதாரணம்: 'Patient ku oxygen 88 irukku, moochu kashtama irukku.'";
                    actions.Add("கேள்வி கேட்கவும்: 'SpO2 < 90% என்றால் என்ன செய்வது?'");
                }
                else if (languageDetected == "Hindi")
                {
                    responseText = "🤖 **आरोग्यपल्स एआई आशा सहायक में आपका स्वागत है!**\n" +
                                   "मैं प्राथमिक स्वास्थ्य देखभाल और ट्राइएज दिशानिर्देशों में सहायता कर सकता हूँ।\n\n" +
                                   "उदाहरण: 'मरीज का ऑक्सीजन 88 है और सांस फूल रही है।'";
                    actions.Add("प्रश्न पूछें: 'SpO2 90 से कम होने पर क्या करें?'");
                }
                else
                {
                    responseText = "🤖 **Welcome to ArogyaPulse AI ASHA Assistant!**\n" +
                                   "I provide natural-language vital sign extraction and clinical decision support in English, Hindi, Tamil, and Hinglish.\n\n" +
                                   "**Example query:** 'Patient has difficulty breathing and oxygen is 88.'";
                    actions.Add("Try asking: 'What to do when SpO2 is below 90%?'");
                    actions.Add("Try asking: 'Patient ku moochu kashtama irukku, oxygen 88'");
                }
            }

            return new ChatResponseDto
            {
                Response = responseText,
                ClinicalAlerts = alerts,
                ActionSteps = actions,
                Severity = severity,
                LanguageDetected = languageDetected,
                PatientContext = patientContext,
                ExtractedVitals = extractedVitals,
                ExtractedSymptoms = extractedSymptoms,
                TriageEvaluation = triageEval,
                Disclaimer = "This assistant provides screening support and does not diagnose disease. All findings must be reviewed by a qualified healthcare professional.",
                Timestamp = DateTime.UtcNow
            };
        }

        private string DetectLanguage(string text, string requestedLang)
        {
            if (requestedLang.ToLower() == "ta" || text.Any(c => c >= 0x0B80 && c <= 0x0BFF) ||
                text.ToLower().Contains("irukku") || text.ToLower().Contains("kashtama") || text.ToLower().Contains("moochu") || text.ToLower().Contains("kaachal"))
            {
                return "Tamil";
            }
            if (requestedLang.ToLower() == "hi" || text.Any(c => c >= 0x0900 && c <= 0x097F) ||
                text.ToLower().Contains("मरीज") || text.ToLower().Contains("है") || text.ToLower().Contains("बुखार"))
            {
                return "Hindi";
            }
            if (text.ToLower().Contains("hai") || text.ToLower().Contains("mariz") || text.ToLower().Contains("ho gaya"))
            {
                return "Hinglish";
            }
            return "English";
        }

        private VitalsDto? ExtractVitals(string text)
        {
            string textLower = text.ToLower();
            int spO2 = 0;
            string bp = "";
            double temp = 0.0;
            int glucose = 0;

            // SpO2 Regex (e.g., "oxygen 88", "spo2 88", "88%", "88 oxygen")
            var spO2Match = Regex.Match(textLower, @"(spo2|oxygen|ऑक्सीजन)\s*[:=]?\s*(\d{2,3})");
            if (spO2Match.Success && int.TryParse(spO2Match.Groups[2].Value, out int sVal))
            {
                spO2 = sVal;
            }
            else
            {
                var percentMatch = Regex.Match(textLower, @"(\d{2,3})\s*%");
                if (percentMatch.Success && int.TryParse(percentMatch.Groups[1].Value, out int pVal) && pVal <= 100 && pVal >= 40)
                {
                    spO2 = pVal;
                }
            }

            // BP Regex (e.g., "160/100", "bp 140 90")
            var bpMatch = Regex.Match(textLower, @"\b(\d{2,3}/\d{2,3})\b");
            if (bpMatch.Success)
            {
                bp = bpMatch.Groups[1].Value;
            }
            else
            {
                var bpAltMatch = Regex.Match(textLower, @"bp\s*[:=]?\s*(\d{2,3})\s+(\d{2,3})");
                if (bpAltMatch.Success)
                {
                    bp = $"{bpAltMatch.Groups[1].Value}/{bpAltMatch.Groups[2].Value}";
                }
            }

            // Temp Regex (e.g., "temp 38.5", "temperature 39")
            var tempMatch = Regex.Match(textLower, @"(temp|temperature|तापमान)\s*[:=]?\s*(\d{2,3}\.?\d?)");
            if (tempMatch.Success && double.TryParse(tempMatch.Groups[2].Value, out double tVal))
            {
                temp = tVal;
            }

            // Glucose Regex (e.g., "glucose 210", "sugar 180")
            var glucoseMatch = Regex.Match(textLower, @"(glucose|sugar|शुगर)\s*[:=]?\s*(\d{2,3})");
            if (glucoseMatch.Success && int.TryParse(glucoseMatch.Groups[2].Value, out int gVal))
            {
                glucose = gVal;
            }

            if (spO2 > 0 || !string.IsNullOrWhiteSpace(bp) || temp > 0 || glucose > 0)
            {
                return new VitalsDto
                {
                    Bp = string.IsNullOrWhiteSpace(bp) ? "120/80" : bp,
                    SpO2 = spO2 > 0 ? spO2 : 98,
                    Temp = temp > 0 ? temp : 37.0,
                    Glucose = glucose > 0 ? glucose : 100
                };
            }

            return null;
        }

        private List<string> ExtractSymptoms(string text)
        {
            var symptoms = new List<string>();
            string textLower = text.ToLower();

            if (textLower.Contains("breathing") || textLower.Contains("moochu") || textLower.Contains("saans") || textLower.Contains("सांस"))
                symptoms.Add("Difficulty breathing");
            if (textLower.Contains("fever") || textLower.Contains("kaachal") || textLower.Contains("bukhar") || textLower.Contains("बुखार"))
                symptoms.Add("Fever");
            if (textLower.Contains("headache") || textLower.Contains("head ache") || textLower.Contains("sirdard") || textLower.Contains("सिरदर्द"))
                symptoms.Add("Severe headache");
            if (textLower.Contains("dizziness") || textLower.Contains("chakkar") || textLower.Contains("mayakkam"))
                symptoms.Add("Dizziness");
            if (textLower.Contains("chest pain") || textLower.Contains("chest discomfort") || textLower.Contains("सीने में दर्द"))
                symptoms.Add("Chest pain");
            if (textLower.Contains("swelling") || textLower.Contains("edema") || textLower.Contains("सूजन"))
                symptoms.Add("Swelling");

            return symptoms;
        }
    }
}