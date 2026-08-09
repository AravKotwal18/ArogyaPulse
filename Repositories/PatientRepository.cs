using Microsoft.EntityFrameworkCore;
using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.Models;
using ArogyaPulse.Api.Interfaces;
namespace ArogyaPulse.Api.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;
        public PatientRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Patient>> GetAllAsync()
        {
            return await _context.Patients
                .OrderByDescending(p => p.Timestamp)
                .ToListAsync();
        }
        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients.FindAsync(id);
        }
        public async Task<Patient> AddAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }
    }
}