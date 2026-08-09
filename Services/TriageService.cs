using ArogyaPulse.Api.Interfaces;
namespace ArogyaPulse.Api.Services
{
    public class TriageService : ITriageService
    {
        public int CalculateScore(string bp, int spO2, double temp, int glucose, bool isPregnant)
        {
            int score = 0;
            if (!string.IsNullOrEmpty(bp))
            {
                var parts = bp.Split('/');
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], out int systolic) &&
                    int.TryParse(parts[1], out int diastolic))
                {
                    if (systolic >= 160 || diastolic >= 100)
                    {
                        score += 40;
                    }
                    else if (systolic >= 140 || diastolic >= 90)
                    {
                        score += 20;
                    }
                    if (isPregnant && systolic >= 140 && diastolic >= 90)
                    {
                        score += 35;
                    }
                }
            }
            if (spO2 < 90)
            {
                score += 35;
            }
            else if (spO2 < 95)
            {
                score += 15;
            }
            if (glucose >= 200 || glucose < 70)
            {
                score += 30;
            }
            if (temp >= 39 || temp < 35)
            {
                score += 20;
            }
            return score;
        }
        public string GetRiskLevel(int score)
        {
            if (score >= 45) return "High";
            if (score >= 20) return "Medium";
            return "Low";
        }
    }
}