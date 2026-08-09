using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.DTOs;
namespace ArogyaPulse.Api.Services
{
    public class TriageService : ITriageService
    {
        public int CalculateScore(string bp, int spO2, double temp, int glucose, bool isPregnant)
        {
            var result = EvaluateTriage(bp, spO2, temp, glucose, isPregnant);
            return result.TotalScore;
        }
        public string GetRiskLevel(int score)
        {
            if (score >= 45) return "High";
            if (score >= 20) return "Medium";
            return "Low";
        }
        public TriageResultDto EvaluateTriage(string bp, int spO2, double temp, int glucose, bool isPregnant)
        {
            int score = 0;
            var breakdown = new List<string>();
            var actions = new List<string>();
            // 1. Blood Pressure Evaluation
            if (!string.IsNullOrWhiteSpace(bp))
            {
                var parts = bp.Split('/');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0].Trim(), out int systolic) &&
                    int.TryParse(parts[1].Trim(), out int diastolic))
                {
                    if (systolic >= 160 || diastolic >= 100)
                    {
                        score += 40;
                        breakdown.Add($"Hypertensive Crisis (BP {bp}): +40 pts");
                        actions.Add("Immediate antihypertensive protocol & urgent physician evaluation required.");
                    }
                    else if (systolic >= 140 || diastolic >= 90)
                    {
                        score += 20;
                        breakdown.Add($"Stage 1 Hypertension (BP {bp}): +20 pts");
                        actions.Add("Monitor BP every 4 hours and schedule medical consultation within 24 hours.");
                    }
                    // Pre-eclampsia screening for pregnant patients
                    if (isPregnant && (systolic >= 140 || diastolic >= 90))
                    {
                        score += 35;
                        breakdown.Add($"Gestational Pre-Eclampsia Risk (Pregnant with BP {bp}): +35 pts");
                        actions.Add("CRITICAL: Immediate referral to District Hospital Obstetric Care.");
                    }
                }
            }
            // 2. Blood Oxygen Saturation (SpO2)
            if (spO2 < 90)
            {
                score += 35;
                breakdown.Add($"Severe Hypoxia (SpO2 {spO2}% < 90%): +35 pts");
                actions.Add("CRITICAL: Administer emergency oxygen (2-4 L/min) and arrange urgent transport.");
            }
            else if (spO2 <= 94)
            {
                score += 15;
                breakdown.Add($"Moderate Hypoxemia (SpO2 {spO2}% in 90-94% range): +15 pts");
                actions.Add("Monitor oxygen saturation continuously; keep patient in sitting position.");
            }
            // 3. Blood Glucose
            if (glucose >= 200)
            {
                score += 30;
                breakdown.Add($"Severe Hyperglycemia (Glucose {glucose} mg/dL ≥ 200): +30 pts");
                actions.Add("Check urine ketones & hydration level. Evaluate for diabetic triage.");
            }
            else if (glucose < 70)
            {
                score += 30;
                breakdown.Add($"Hypoglycemia (Glucose {glucose} mg/dL < 70): +30 pts");
                actions.Add("Administer oral oral glucose/sweetened juice immediately; retest in 15 mins.");
            }
            // 4. Body Temperature
            if (temp >= 39.0)
            {
                score += 20;
                breakdown.Add($"High Fever (Temp {temp}°C ≥ 39°C): +20 pts");
                actions.Add("Provide antipyretics (Paracetamol) and cold compress; screen for infection/malaria.");
            }
            else if (temp < 35.0)
            {
                score += 20;
                breakdown.Add($"Hypothermia (Temp {temp}°C < 35°C): +20 pts");
                actions.Add("Provide thermal blanket warming & monitor core temperature.");
            }
            if (breakdown.Count == 0)
            {
                breakdown.Add("All vitals within standard physiological reference ranges: 0 pts");
                actions.Add("Routine primary healthcare checkup & routine follow-up.");
            }
            string riskLevel = GetRiskLevel(score);
            return new TriageResultDto
            {
                TotalScore = score,
                RiskLevel = riskLevel,
                Breakdown = breakdown,
                ActionRecommendations = actions
            };
        }
    }
}