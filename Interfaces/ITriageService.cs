using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Interfaces
{
    public interface ITriageService
    {
        int CalculateScore(string bp, int spO2, double temp, int glucose, bool isPregnant);
        string GetRiskLevel(int score);
        TriageResultDto EvaluateTriage(string bp, int spO2, double temp, int glucose, bool isPregnant);
    }
}