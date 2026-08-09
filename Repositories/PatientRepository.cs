using ArogyaPulse.Api.Data;
using ArogyaPulse.Api.DTOs;
using ArogyaPulse.Api.Interfaces;
using ArogyaPulse.Api.Models;
using Microsoft.EntityFrameworkCore;
namespace ArogyaPulse.Api.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _context;
        public PatientRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<Patient>> GetAllAsync(
            string? village = null,
            string? riskLevel = null,
            string? search = null)
        {
            var query = _context.Patients
                .AsNoTracking()
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(village))
            {
                var villageFilter = village.Trim().ToLower();

                query = query.Where(p =>
                    p.Village.ToLower() == villageFilter);
            }
            if (!string.IsNullOrWhiteSpace(riskLevel))
            {
                var riskFilter = riskLevel.Trim().ToLower();

                query = query.Where(p =>
                    p.RiskLevel.ToLower() == riskFilter);
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchFilter = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchFilter) ||
                    p.Village.ToLower().Contains(searchFilter) ||
                    p.Symptoms.ToLower().Contains(searchFilter));
            }
            return await query
                .OrderByDescending(p => p.Timestamp)
                .ToListAsync();
        }
        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Patient> AddAsync(Patient patient)
        {
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return patient;
        }
        public async Task<Patient?> UpdateAsync(Patient patient)
        {
            var existing = await _context.Patients.FindAsync(patient.Id);
            if (existing == null)
                return null;
            _context.Entry(existing)
                .CurrentValues
                .SetValues(patient);
            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return false;
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<VillageStatDto>> GetVillageStatsAsync()
        {
            var stats = await _context.Patients
                .AsNoTracking()
                .GroupBy(p => p.Village)
                .Select(g => new VillageStatDto
                {
                    Village = g.Key,
                    TotalScreened = g.Count(),
                    HighRiskCount = g.Count(
                        p => p.RiskLevel == "High"),
                    MediumRiskCount = g.Count(
                        p => p.RiskLevel == "Medium"),
                    LowRiskCount = g.Count(
                        p => p.RiskLevel == "Low"),
                    LastScreening = g.Max(
                        p => p.Timestamp)
                })
                .OrderByDescending(v => v.HighRiskCount)
                .ThenByDescending(v => v.TotalScreened)
                .ToListAsync();
            return stats;
        }
    }
}