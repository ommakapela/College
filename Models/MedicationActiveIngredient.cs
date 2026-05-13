using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ibhayiPharmacy.Models
{
    public class MedicationActiveIngredient
    {
        [Required]
        public int MedicationId { get; set; }

        [Required]
        public int ActiveIngredientId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Strength")]
        public string Strength { get; set; } // e.g., "500mg", "10ml", "2.5g"

        // Navigation Properties
        [ForeignKey("MedicationId")]
        public virtual Medication Medication { get; set; }

        [ForeignKey("ActiveIngredientId")]
        public virtual ActiveIngredient ActiveIngredient { get; set; }
    }
}
