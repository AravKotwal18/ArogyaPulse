using System.ComponentModel.DataAnnotations;

namespace ArogyaPulse.Api.Models
{
    /// <summary>
    /// Immutable audit trail entry for every clinically significant action.
    /// Supports traceability requirements for healthcare systems.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int? PatientId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string PerformedBy { get; set; } = "System";

        [MaxLength(2000)]
        public string Details { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
