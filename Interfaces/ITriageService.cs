using ArogyaPulse.Api.Models;
namespace ArogyaPulse.Api.Interfaces
{
    public interface ITriageService
    {
        int CalculateScore(string bp, int spO2, double temp, int glucose, bool isPregnant);
        string GetRiskLevel(int score);
    }
}