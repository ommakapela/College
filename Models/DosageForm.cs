using System.ComponentModel.DataAnnotations;

namespace ibhayiPharmacy.Models
{
    public class DosageForm
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Dosage Form")]
        public string Name { get; set; } // e.g., Tablet, Capsule, Syrup, Injection

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
    }
}
