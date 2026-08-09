using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Patients.Any())
            {
                return; // already seeded
            }

            var patients = new List<Patient>
            {
                new Patient
                {
                    Name = "Priya Sharma",
                    Age = 28,
                    Village = "Nandpur",
                    Bp = "160/105",
                    SpO2 = 88,
                    Temp = 38.5,
                    Glucose = 210,
                    Symptoms = "Severe headache, swelling in face",
                    IsPregnant = true,
                    RiskScore = 75,
                    RiskLevel = "High"
                },
                new Patient
                {
                    Name = "Rajesh Kumar",
                    Age = 45,
                    Village = "Laxmipur",
                    Bp = "145/92",
                    SpO2 = 92,
                    Temp = 37.2,
                    Glucose = 180,
                    Symptoms = "Chest discomfort, shortness of breath",
                    IsPregnant = false,
                    RiskScore = 55,
                    RiskLevel = "High"
                },
                new Patient
                {
                    Name = "Anjali Devi",
                    Age = 32,
                    Village = "Rampur",
                    Bp = "135/88",
                    SpO2 = 95,
                    Temp = 36.8,
                    Glucose = 120,
                    Symptoms = "Mild fever, general weakness",
                    IsPregnant = false,
                    RiskScore = 28,
                    RiskLevel = "Medium"
                },
                new Patient
                {
                    Name = "Vikram Singh",
                    Age = 50,
                    Village = "Nandpur",
                    Bp = "130/85",
                    SpO2 = 96,
                    Temp = 37.0,
                    Glucose = 110,
                    Symptoms = "Routine checkup",
                    IsPregnant = false,
                    RiskScore = 12,
                    RiskLevel = "Low"
                }
            };

            context.Patients.AddRange(patients);
            context.SaveChanges();
        }
    }
}