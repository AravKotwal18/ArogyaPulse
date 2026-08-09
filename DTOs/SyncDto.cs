using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.DTOs
{
    public class SyncRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string WorkerId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string AppVersion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public List<OfflinePatientDto> Records { get; set; } = new();
    }

    public class OfflinePatientDto
    {
        [Required]
        [MaxLength(100)]
        public string LocalRecordId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 120)]
        public int Age { get; set; }

        [Required]
        [MaxLength(20)]
        public string Gender { get; set; } = "Unknown";

        [MaxLength(10)]
        public string BloodGroup { get; set; } = "Unknown";

        [Required]
        [MaxLength(100)]
        public string Village { get; set; } = string.Empty;

        [Required]
        public VitalsDto Vitals { get; set; } = new();

        [MaxLength(500)]
        public string Symptoms { get; set; } = string.Empty;

        public bool IsPregnant { get; set; }

        public DateTime CapturedAt { get; set; } = DateTime.UtcNow;
    }

    public class SyncResponseDto
    {
        public int TotalReceived { get; set; }

        public int Synced { get; set; }

        public int AlreadySynced { get; set; }

        public int Conflicts { get; set; }

        public int Errors { get; set; }

        public List<SyncRecordResultDto> Results { get; set; } = new();

        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }

    public class SyncRecordResultDto
    {
        public string LocalRecordId { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? PatientId { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}