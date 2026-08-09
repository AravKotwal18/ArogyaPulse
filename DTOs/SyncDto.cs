using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class SyncRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        public List<OfflinePatientDto> Records { get; set; } = new();
    }

    public class OfflinePatientDto
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        [Required]
        public string Gender { get; set; } = "Unknown";
        public string BloodGroup { get; set; } = "Unknown";
        [Required]
        public string Village { get; set; } = string.Empty;
        [Required]
        public VitalsDto Vitals { get; set; } = new();
        public string Symptoms { get; set; } = string.Empty;
        public bool IsPregnant { get; set; } = false;

        /// <summary>
        /// Timestamp from the offline device when the record was captured.
        /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }

    public class SyncResponseDto
    {
        public int TotalReceived { get; set; }
        public int Synced { get; set; }
        public int Conflicts { get; set; }
        public List<SyncRecordResultDto> Results { get; set; } = new();
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }

    public class SyncRecordResultDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // Synced, Conflict, Error
        public int? PatientId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
