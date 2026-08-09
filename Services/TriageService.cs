using ArogyaPulse.Api.DTOs;
using ArogyaPulse.Api.Interfaces;
namespace ArogyaPulse.Api.Services
{
    public class TriageService : ITriageService
    {
        private const int HighRiskScore = 50;
        private const int MediumRiskScore = 25;
        private const int SevereBpPoints = 40;
        private const int ElevatedBpPoints = 20;
        private const int PregnancyBpPoints = 45;
        private const int SevereSpO2Points = 35;
        private const int ModerateSpO2Points = 15;
        private const int HighGlucosePoints = 25;
        private const int LowGlucosePoints = 30;
        private const int HighTemperaturePoints = 15;
        private const int LowTemperaturePoints = 20;
        private const string Protocol = "ArogyaPulse Screening Protocol v1";
        private const string Disclaimer = "This is a screening aid, not a clinical diagnosis. " + "All findings must be reviewed by a qualified healthcare professional.";

        public int CalculateScore(string bp, int spO2, double temp, int glucose, bool isPregnant)
        {
            return EvaluateTriage(bp, spO2, temp, glucose, isPregnant).TotalScore;
        }
        public string GetRiskLevel(int score)
        {
            if (score >= HighRiskScore)
                return "High";
            if (score >= MediumRiskScore)
                return "Medium";
            return "Low";
        }
        public TriageResultDto EvaluateTriage(string bp, int spO2, double temp, int glucose, bool isPregnant)
        {
            int score = 0;
            var breakdown = new List<string>();
            var actions = new List<string>();
            EvaluateBloodPressure(bp, isPregnant, ref score, breakdown, actions);
            EvaluateSpO2(spO2, ref score, breakdown, actions);
            EvaluateGlucose(glucose, ref score, breakdown, actions);
            EvaluateTemperature(temp, ref score, breakdown, actions);
            if (breakdown.Count == 0)
            {
                breakdown.Add("All recorded vital signs are within the configured screening ranges.");
                actions.Add("Continue routine monitoring and follow-up.");
            }
            score = Math.Clamp(score, 0, 100);
            return new TriageResultDto
            {
                TotalScore = score,
                RiskLevel = GetRiskLevel(score),
                Breakdown = breakdown,
                ActionRecommendations = actions,
                TriageProtocol = Protocol,
                Disclaimer = Disclaimer,
                EvaluatedAt = DateTime.UtcNow
            };
        }
        private static void EvaluateBloodPressure(string bp, bool isPregnant, ref int score, List<string> breakdown, List<string> actions)
        {
            if (string.IsNullOrWhiteSpace(bp))
            {
                breakdown.Add("Blood pressure was not provided.");
                actions.Add("Record a valid blood pressure reading.");
                return;
            }
            var parts = bp.Split('/');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0].Trim(), out var systolic) ||
                !int.TryParse(parts[1].Trim(), out var diastolic))
            {
                breakdown.Add($"Invalid blood pressure format ({bp}).");
                actions.Add("Enter blood pressure in systolic/diastolic format, for example 120/80.");
                return;
            }
            if (systolic <= 0 || diastolic <= 0)
            {
                breakdown.Add($"Invalid blood pressure reading ({bp}).");
                actions.Add("Verify the blood pressure measurement.");
                return;
            }
            if (isPregnant && (systolic >= 140 || diastolic >= 90))
            {
                score += PregnancyBpPoints;
                breakdown.Add($"Pregnancy with elevated BP ({bp}): +{PregnancyBpPoints} pts");
                actions.Add("Prompt clinical assessment is recommended for a pregnant patient with elevated blood pressure.");
                return;
            }

            if (systolic >= 160 || diastolic >= 100)
            {
                score += SevereBpPoints;
                breakdown.Add($"Severely elevated BP ({bp}): +{SevereBpPoints} pts");
                actions.Add("Urgent clinical assessment is recommended.");
                return;
            }

            if (systolic >= 140 || diastolic >= 90)
            {
                score += ElevatedBpPoints;
                breakdown.Add($"Elevated BP ({bp}): +{ElevatedBpPoints} pts");
                actions.Add("Repeat the BP measurement and arrange clinical review.");
            }
        }
        private static void EvaluateSpO2(
            int spO2,
            ref int score,
            List<string> breakdown,
            List<string> actions)
        {
            if (spO2 < 50 || spO2 > 100)
            {
                breakdown.Add($"Invalid SpO2 reading ({spO2}%).");
                actions.Add("Verify the SpO2 measurement.");
                return;
            }
            if (spO2 < 90)
            {
                score += SevereSpO2Points;
                breakdown.Add($"Very low SpO2 ({spO2}%): +{SevereSpO2Points} pts");
                actions.Add("Urgent clinical assessment is recommended.");
                return;
            }

            if (spO2 <= 94)
            {
                score += ModerateSpO2Points;
                breakdown.Add($"Low SpO2 ({spO2}%): +{ModerateSpO2Points} pts");
                actions.Add("Repeat the measurement and arrange clinical review.");
            }
        }
        private static void EvaluateGlucose(
            int glucose,
            ref int score,
            List<string> breakdown,
            List<string> actions)
        {
            if (glucose < 20 || glucose > 600)
            {
                breakdown.Add($"Glucose reading is outside the accepted range ({glucose} mg/dL).");
                actions.Add("Verify the glucose measurement.");
                return;
            }
            if (glucose >= 200)
            {
                score += HighGlucosePoints;
                breakdown.Add($"High glucose ({glucose} mg/dL): +{HighGlucosePoints} pts");
                actions.Add("Repeat or confirm the reading and arrange clinical assessment.");
                return;
            }
            if (glucose < 70)
            {
                score += LowGlucosePoints;
                breakdown.Add($"Low glucose ({glucose} mg/dL): +{LowGlucosePoints} pts");
                actions.Add("Prompt clinical assessment is recommended.");
            }
        }

        private static void EvaluateTemperature(
            double temp,
            ref int score,
            List<string> breakdown,
            List<string> actions)
        {
            if (temp < 30 || temp > 45)
            {
                breakdown.Add($"Invalid temperature reading ({temp:F1}°C).");
                actions.Add("Verify the temperature measurement.");
                return;
            }

            if (temp >= 39)
            {
                score += HighTemperaturePoints;
                breakdown.Add($"High temperature ({temp:F1}°C): +{HighTemperaturePoints} pts");
                actions.Add("Assess for possible infection and arrange clinical review.");

                return;
            }

            if (temp < 35)
            {
                score += LowTemperaturePoints;
                breakdown.Add($"Low temperature ({temp:F1}°C): +{LowTemperaturePoints} pts");
                actions.Add("Urgent clinical assessment is recommended.");
            }
        }
    }
}