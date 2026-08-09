using ArogyaPulse.Api.Models;
using ArogyaPulse.Api.DTOs;

namespace ArogyaPulse.Api.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllAsync(string? village = null, string? riskLevel = null, string? search = null);
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient> AddAsync(Patient patient);
        Task<Patient?> UpdateAsync(Patient patient);
        Task<bool> DeleteAsync(int id);
        Task<List<VillageStatDto>> GetVillageStatsAsync();
    }
}