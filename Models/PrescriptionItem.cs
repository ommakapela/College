using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ibhayiPharmacy.Models
{
    public class PrescriptionItem
    {
        [Key]
        public int Id { get; set; }

        // Link to Prescription
        [Required]
        public int PrescriptionId { get; set; }

        [ForeignKey("PrescriptionId")]
        public Prescription Prescription { get; set; }

        // Link to Medication
        [Required]
        public int MedicationId { get; set; }

        [ForeignKey("MedicationId")]
        public Medication Medication { get; set; }

        // Quantity and Instructions
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [StringLength(500)]
        public string? Instructions { get; set; }

        // Pricing (added for two-step workflow)
        [Column(TypeName = "decimal(18, 2)")]
        public decimal UnitPrice { get; set; }

        // Repeats Management
        [Range(0, 12, ErrorMessage = "Repeats must be between 0 and 12")]
        public int Repeats { get; set; }

        // Track remaining repeats (added for two-step workflow)
        public int RepeatsRemaining { get; set; }
    }
}