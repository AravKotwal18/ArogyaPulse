using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.Models
{
    /// <summary>
    /// Tracks offline-created records synced from ASHA worker devices.
    /// Supports conflict detection and resolution for rural low-connectivity environments.
    /// </summary>
    public class SyncLog
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string DeviceId { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LocalRecordId { get; set; } = string.Empty;

        /// <summary>
        /// JSON-serialized patient record payload from offline device.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Synced, Conflict

        public int? PatientId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SyncedAt { get; set; }
    }
}
