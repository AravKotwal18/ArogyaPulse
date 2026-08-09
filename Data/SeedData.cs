using ArogyaPulse.Api.Models;

namespace ArogyaPulse.Api.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            var existingPatients = context.Patients.ToList();
            if (existingPatients.Any())
            {
                bool modified = false;
                foreach (var p in existingPatients)
                {
                    if (string.IsNullOrWhiteSpace(p.Gender))
                    {
                        p.Gender = (p.Name.Contains("Rajesh") || p.Name.Contains("Vikram") || p.Name.Contains("Ramesh")) ? "Male" : "Female";
                        p.BloodGroup = p.Name.Contains("Rajesh") ? "A+" : p.Name.Contains("Vikram") ? "B-" : p.Name.Contains("Priya") ? "B+" : "O+";
                        if (p.Gender != "Female")
                        {
                            p.IsPregnant = false;
                        }
                        modified = true;
                    }
                }
                if (modified)
                {
                    context.SaveChanges();
                }
                return;
            }

            var patients = new List<Patient>
            {
                new Patient
                {
                    Name = "Priya Sharma",
                    Age = 28,
                    Gender = "Female",
                    BloodGroup = "B+",
                    Village = "Nandpur",
                    Bp = "165/105",
                    SpO2 = 88,
                    Temp = 38.5,
                    Glucose = 210,
                    Symptoms = "Severe headache, facial edema, visual disturbance",
                    IsPregnant = true,
                    RiskScore = 140,
                    RiskLevel = "High",
                    Status = "Referred to CHC",
                    DoctorNotes = "Urgent obstetric triage required. Transferred via ambulance.",
                    Timestamp = DateTime.UtcNow.AddMinutes(-25)
                },
                new Patient
                {
                    Name = "Sunita Verma",
                    Age = 24,
                    Gender = "Female",
                    BloodGroup = "O+",
                    Village = "Devpur",
                    Bp = "142/94",
                    SpO2 = 91,
                    Temp = 37.8,
                    Glucose = 130,
                    Symptoms = "Swelling in ankles, persistent headache during 3rd trimester",
                    IsPregnant = true,
                    RiskScore = 70,
                    RiskLevel = "High",
                    Status = "Pending",
                    DoctorNotes = "High pre-eclampsia suspicion. Scheduled for immediate doctor evaluation.",
                    Timestamp = DateTime.UtcNow.AddMinutes(-40)
                },
                new Patient
                {
                    Name = "Rajesh Kumar",
                    Age = 45,
                    Gender = "Male",
                    BloodGroup = "A+",
                    Village = "Laxmipur",
                    Bp = "145/92",
                    SpO2 = 92,
                    Temp = 37.2,
                    Glucose = 180,
                    Symptoms = "Chest discomfort, shortness of breath on exertion",
                    IsPregnant = false,
                    RiskScore = 35,
                    RiskLevel = "Medium",
                    Status = "Under Observation",
                    DoctorNotes = "ECG recommended. Priority review within 24h.",
                    Timestamp = DateTime.UtcNow.AddHours(-2)
                },
                new Patient
                {
                    Name = "Ramesh Patel",
                    Age = 58,
                    Gender = "Male",
                    BloodGroup = "O-",
                    Village = "Devpur",
                    Bp = "162/102",
                    SpO2 = 94,
                    Temp = 36.9,
                    Glucose = 240,
                    Symptoms = "Dizziness, extreme thirst, blurred vision",
                    IsPregnant = false,
                    RiskScore = 85,
                    RiskLevel = "High",
                    Status = "Pending",
                    DoctorNotes = "Diabetic hypertensive urgency.",
                    Timestamp = DateTime.UtcNow.AddHours(-1)
                },
                new Patient
                {
                    Name = "Anjali Devi",
                    Age = 32,
                    Gender = "Female",
                    BloodGroup = "AB+",
                    Village = "Rampur",
                    Bp = "135/88",
                    SpO2 = 95,
                    Temp = 36.8,
                    Glucose = 120,
                    Symptoms = "Mild fever, general weakness",
                    IsPregnant = false,
                    RiskScore = 0,
                    RiskLevel = "Low",
                    Status = "Discharged",
                    DoctorNotes = "Normal vitals. Prescribed rest and oral rehydration.",
                    Timestamp = DateTime.UtcNow.AddHours(-5)
                },
                new Patient
                {
                    Name = "Vikram Singh",
                    Age = 50,
                    Gender = "Male",
                    BloodGroup = "B-",
                    Village = "Nandpur",
                    Bp = "130/85",
                    SpO2 = 96,
                    Temp = 37.0,
                    Glucose = 110,
                    Symptoms = "Routine community screening checkup",
                    IsPregnant = false,
                    RiskScore = 0,
                    RiskLevel = "Low",
                    Status = "Discharged",
                    DoctorNotes = "Healthy.",
                    Timestamp = DateTime.UtcNow.AddHours(-6)
                }
            };

            context.Patients.AddRange(patients);
            context.SaveChanges();
        }
    }
}