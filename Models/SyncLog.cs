using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.Models
{
    public class SyncLog
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LocalRecordId { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        public string Payload { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public int? PatientId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SyncedAt { get; set; }
    }
}