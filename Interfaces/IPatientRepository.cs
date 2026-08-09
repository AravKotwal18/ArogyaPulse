using ArogyaPulse.Api.Models;
namespace ArogyaPulse.Api.Interfaces
{
    public interface IPatientRepository
    {
        Task<List<Patient>> GetAllAsync();
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient> AddAsync(Patient patient);
    }
}