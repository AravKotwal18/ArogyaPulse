using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Services
{
    public class AiChatService : IAiChatService
    {
        private readonly IPatientRepository _patientRepository;

        public AiChatService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<ChatResponseDto> GetGuidanceAsync(ChatRequestDto request)
        {
            var queryLower = request.Query.ToLower();
            bool isHindi = request.Language.ToLower() == "hi" || ContainsHindi(request.Query);

            var alerts = new List<string>();
            var actions = new List<string>();
            string responseText = "";
            string severity = "Info";
            string? patientContext = null;

            // If a PatientId is provided, fetch context
            if (request.PatientId.HasValue)
            {
                var patient = await _patientRepository.GetByIdAsync(request.PatientId.Value);
                if (patient != null)
                {
                    patientContext = $"Patient: {patient.Name}, Age: {patient.Age}, Gender: {patient.Gender}, " +
                                    $"Village: {patient.Village}, BP: {patient.Bp}, SpO2: {patient.SpO2}%, " +
                                    $"Temp: {patient.Temp:F1}°C, Glucose: {patient.Glucose} mg/dL, " +
                                    $"Risk: {patient.RiskLevel} ({patient.RiskScore}/100)";
                }
            }

            // SpO2 / Oxygen topics
            if (queryLower.Contains("spo2") || queryLower.Contains("oxygen") || queryLower.Contains("hypoxia") ||
                queryLower.Contains("ऑक्सीजन") || queryLower.Contains("सांस"))
            {
                if (queryLower.Contains("< 90") || queryLower.Contains("90") || queryLower.Contains("कम"))
                {
                    severity = "Critical";
                    if (isHindi)
                    {
                        responseText = "⚠️ **गंभीर हाइपोक्सिया (SpO2 < 90%) आपातकालीन मार्गदर्शन:**\n" +
                                       "जब मरीज का SpO2 90% से कम होता है, तो यह गंभीर श्वसन विफलता का संकेत है। तुरंत आपातकालीन कार्रवाई आवश्यक है।";
                        alerts.Add("SpO2 < 90% गंभीर हाइपोक्सिया का संकेत है!");
                        actions.Add("मरीज को 2-4 लीटर/मिनट की दर से पूरक ऑक्सीजन दें।");
                        actions.Add("मरीज को बैठने की स्थिति (Fowler's position) में रखें।");
                        actions.Add("तुरंत एम्बुलेंस/अस्पताल वाहन बुलाएं और जिला अस्पताल को सूचित करें।");
                    }
                    else
                    {
                        responseText = "⚠️ **Severe Hypoxia (SpO2 < 90%) Emergency Guidance:**\n" +
                                       "An SpO2 level below 90% indicates severe respiratory compromise and requires immediate emergency triage.";
                        alerts.Add("SpO2 < 90% signifies severe physiological distress!");
                        actions.Add("Administer supplemental oxygen (2-4 L/min) immediately.");
                        actions.Add("Place patient in an upright sitting (Fowler's) position to aid lung expansion.");
                        actions.Add("Dispatch emergency ambulance transport to District Hospital / CHC.");
                    }
                }
                else
                {
                    if (isHindi)
                    {
                        responseText = "ℹ️ **SpO2 सामान्य सीमाएं:**\n" +
                                       "• 95% - 100%: सामान्य ऑक्सीजन स्तर\n" +
                                       "• 90% - 94%: मध्यम हाइपोक्सिमिया (निगरानी आवश्यक)\n" +
                                       "• < 90%: गंभीर हाइपोक्सिया (तत्काल अस्पताल रेफरल)";
                        actions.Add("पल्स ऑक्सीमीटर की बैटरी और फिंगर प्रोब की स्थिति जांचें।");
                    }
                    else
                    {
                        responseText = "ℹ️ **SpO2 Reference Ranges:**\n" +
                                       "• 95% - 100%: Normal blood oxygen saturation\n" +
                                       "• 90% - 94%: Moderate hypoxemia (Priority review within 24h)\n" +
                                       "• < 90%: Severe hypoxia (Immediate emergency transport)";
                        actions.Add("Verify probe placement and check for cold extremities.");
                    }
                }
            }
            // Blood Pressure / Hypertension / Pre-eclampsia
            else if (queryLower.Contains("bp") || queryLower.Contains("pressure") || queryLower.Contains("hypertension") ||
                     queryLower.Contains("बीपी") || queryLower.Contains("रक्तचाप") ||
                     queryLower.Contains("pre-eclampsia") || queryLower.Contains("eclampsia"))
            {
                if (queryLower.Contains("preg") || queryLower.Contains("गर्भवती") ||
                    queryLower.Contains("pre-eclampsia") || queryLower.Contains("गर्भावस्था"))
                {
                    severity = "Critical";
                    if (isHindi)
                    {
                        responseText = "🤰 **गर्भावस्था में उच्च बीपी (प्री-एकलम्पसिया) दिशानिर्देश:**\n" +
                                       "गर्भवती महिला में सिस्टोलिक बीपी ≥ 140 या डायस्टोलिक ≥ 90 mmHg प्री-एकलम्पसिया का उच्च जोखिम दर्शाता है।";
                        alerts.Add("प्री-एकलम्पसिया माँ और बच्चे दोनों के लिए खतरनाक है!");
                        alerts.Add("लक्षण: तेज सिरदर्द, चेहरे पर सूजन, धुंधला दिखना।");
                        actions.Add("तुरंत जिला अस्पताल के प्रसूति विभाग (Obstetric Unit) में रेफर करें।");
                        actions.Add("मरीज को शांत रखें और बीपी दोबारा मापें।");
                    }
                    else
                    {
                        responseText = "🤰 **Gestational Pre-Eclampsia Clinical Protocol:**\n" +
                                       "Blood Pressure ≥ 140/90 mmHg in a pregnant woman triggers an automatic obstetric high-risk alert (+45 points).";
                        alerts.Add("Pre-eclampsia poses high risk of seizures (eclampsia) and fetal compromise!");
                        alerts.Add("Warning signs: Severe headache, facial edema, visual disturbances, upper abdominal pain.");
                        actions.Add("Immediate referral to District Hospital Obstetric emergency department.");
                        actions.Add("Keep patient calm in left lateral position and transport urgently.");
                    }
                }
                else
                {
                    if (isHindi)
                    {
                        responseText = "🩸 **रक्तचाप (BP) वर्गीकरण:**\n" +
                                       "• सामान्य: < 120/80 mmHg\n" +
                                       "• स्टेज 1 हाइपरटेंशन: 140-159 / 90-99 mmHg (+20 अंक)\n" +
                                       "• हाइपरटेंसिव क्राइसिस: ≥ 160/100 mmHg (+40 अंक)";
                        actions.Add("यदि बीपी 160/100 या उससे अधिक है, तो तुरंत डॉक्टर को सूचित करें।");
                    }
                    else
                    {
                        responseText = "🩸 **Blood Pressure Triage Stratification:**\n" +
                                       "• Normal: < 120/80 mmHg\n" +
                                       "• Stage 1 Hypertension: Systolic 140-159 or Diastolic 90-99 mmHg (+20 pts)\n" +
                                       "• Hypertensive Crisis: Systolic ≥ 160 or Diastolic ≥ 100 mmHg (+40 pts)";
                        actions.Add("If BP ≥ 160/100, trigger doctor alert and re-check after 15 minutes rest.");
                    }
                }
            }
            // Fever / Temperature / Glucose / Sugar
            else if (queryLower.Contains("fever") || queryLower.Contains("temp") ||
                     queryLower.Contains("बुखार") || queryLower.Contains("तापमान") ||
                     queryLower.Contains("glucose") || queryLower.Contains("sugar") || queryLower.Contains("शुगर"))
            {
                if (isHindi)
                {
                    responseText = "🌡️ **तापमान एवं ग्लूकोज मार्गदर्शन:**\n" +
                                   "• उच्च बुखार (≥ 39°C): पैरासिटामोल दें, ठंडी पट्टी करें, मलेरिया/डेंगू की जांच करें।\n" +
                                   "• लो शुगर (< 70 mg/dL): तुरंत मीठा पानी/जूस या ग्लूकोज दें।\n" +
                                   "• हाई शुगर (≥ 200 mg/dL): खूब पानी पिलाएं और डॉक्टर से परामर्श लें।";
                    actions.Add("महत्वपूर्ण संकेत 30 मिनट में दोबारा दर्ज करें।");
                }
                else
                {
                    responseText = "🌡️ **Temperature & Glucose Guidelines:**\n" +
                                   "• High Fever (≥ 39°C): Administer antipyretic, sponge cooling, screen for endemic infections.\n" +
                                   "• Hypoglycemia (< 70 mg/dL): Administer 15-20g fast-acting oral glucose/fruit juice immediately.\n" +
                                   "• Hyperglycemia (≥ 200 mg/dL): Check hydration and refer for glycemic control.";
                    actions.Add("Re-assess vital signs within 30 minutes.");
                }
            }
            // Dehydration / Diarrhea (new topic)
            else if (queryLower.Contains("dehydration") || queryLower.Contains("diarrhea") || queryLower.Contains("diarrhoea") ||
                     queryLower.Contains("ors") || queryLower.Contains("दस्त") || queryLower.Contains("निर्जलीकरण"))
            {
                severity = "Warning";
                if (isHindi)
                {
                    responseText = "💧 **निर्जलीकरण / दस्त प्रबंधन (WHO ORS प्रोटोकॉल):**\n" +
                                   "• हल्का: मुँह सूखा, प्यास बढ़ी → ORS घोल हर 15 मिनट में।\n" +
                                   "• मध्यम: आँखें धँसी, त्वचा में लचीलापन कम → ORS + जिंक सप्लीमेंट।\n" +
                                   "• गंभीर: बेहोशी, पेशाब बंद → तुरंत IV तरल पदार्थ के लिए रेफर करें।";
                    alerts.Add("बच्चों और बुजुर्गों में निर्जलीकरण तेजी से गंभीर हो सकता है।");
                    actions.Add("ORS घोल तैयार करें: 1 लीटर साफ पानी + 1 ORS पैकेट।");
                    actions.Add("यदि 4 घंटे में सुधार न हो तो PHC रेफर करें।");
                }
                else
                {
                    responseText = "💧 **Dehydration / Diarrhea Management (WHO ORS Protocol):**\n" +
                                   "• Mild: Dry mouth, increased thirst → ORS solution every 15 minutes.\n" +
                                   "• Moderate: Sunken eyes, reduced skin turgor → ORS + Zinc supplementation.\n" +
                                   "• Severe: Lethargy, no urine output → Immediate IV fluid referral.";
                    alerts.Add("Children and elderly patients are at high risk of rapid deterioration.");
                    actions.Add("Prepare ORS: 1 liter clean water + 1 ORS packet.");
                    actions.Add("If no improvement in 4 hours, refer to PHC/CHC.");
                }
            }
            // Malaria screening (new topic)
            else if (queryLower.Contains("malaria") || queryLower.Contains("मलेरिया") || queryLower.Contains("mosquito") ||
                     queryLower.Contains("मच्छर"))
            {
                severity = "Warning";
                if (isHindi)
                {
                    responseText = "🦟 **मलेरिया जांच प्रोटोकॉल:**\n" +
                                   "• लक्षण: तेज बुखार (ठंड के साथ), सिरदर्द, शरीर दर्द, पसीना।\n" +
                                   "• RDT (रैपिड डायग्नोस्टिक टेस्ट) करें।\n" +
                                   "• Pf+ (फॉल्सीपेरम) → ACT दवाई तुरंत शुरू करें।\n" +
                                   "• Pv+ (वाइवैक्स) → क्लोरोक्वीन + प्राइमाक्वीन।";
                    actions.Add("रोगी का तापमान हर 4 घंटे में मापें।");
                    actions.Add("यदि RDT पॉजिटिव है, तो PHC को तुरंत सूचित करें।");
                }
                else
                {
                    responseText = "🦟 **Malaria Screening Protocol:**\n" +
                                   "• Symptoms: High fever with chills, headache, body ache, sweating.\n" +
                                   "• Perform RDT (Rapid Diagnostic Test).\n" +
                                   "• Pf+ (Falciparum) → Start ACT (Artemisinin Combination Therapy) immediately.\n" +
                                   "• Pv+ (Vivax) → Chloroquine + Primaquine course.";
                    actions.Add("Monitor temperature every 4 hours.");
                    actions.Add("If RDT positive, notify PHC immediately for case reporting.");
                }
            }
            // Default / Welcome
            else
            {
                if (isHindi)
                {
                    responseText = "🤖 **आरोग्यपल्स एआई आशा सहायक में आपका स्वागत है!**\n" +
                                   "मैं प्राथमिक स्वास्थ्य देखभाल और WHO/NHM ट्राइएज दिशानिर्देशों में सहायता कर सकता हूँ।\n\n" +
                                   "**उपलब्ध विषय:**\n" +
                                   "• बीपी (BP) / रक्तचाप\n" +
                                   "• ऑक्सीजन (SpO2) / सांस की तकलीफ\n" +
                                   "• बुखार / तापमान / शुगर\n" +
                                   "• गर्भावस्था / प्री-एकलम्पसिया\n" +
                                   "• दस्त / निर्जलीकरण / ORS\n" +
                                   "• मलेरिया जांच";
                    actions.Add("उदाहरण: 'जब SpO2 < 90% हो तो क्या करें?'");
                    actions.Add("उदाहरण: 'गर्भावस्था में उच्च बीपी के क्या लक्षण हैं?'");
                    actions.Add("उदाहरण: 'दस्त में ORS कैसे दें?'");
                }
                else
                {
                    responseText = "🤖 **Welcome to ArogyaPulse AI ASHA Assistant!**\n" +
                                   "I provide instant clinical decision support following WHO and NHM India rural triage guidelines.\n\n" +
                                   "**Available Topics:**\n" +
                                   "• Blood Pressure / Hypertension\n" +
                                   "• SpO2 / Oxygen / Hypoxia\n" +
                                   "• Fever / Temperature / Glucose\n" +
                                   "• Pregnancy / Pre-eclampsia\n" +
                                   "• Dehydration / Diarrhea / ORS\n" +
                                   "• Malaria Screening";
                    actions.Add("Try: 'What to do when SpO2 is below 90%?'");
                    actions.Add("Try: 'Pre-eclampsia warning signs and BP thresholds'");
                    actions.Add("Try: 'How to prepare ORS for dehydration?'");
                }
            }

            return new ChatResponseDto
            {
                Response = responseText,
                ClinicalAlerts = alerts,
                ActionSteps = actions,
                Severity = severity,
                PatientContext = patientContext,
                Timestamp = DateTime.UtcNow
            };
        }

        private bool ContainsHindi(string text)
        {
            return text.Any(c => c >= 0x0900 && c <= 0x097F);
        }
    }
}