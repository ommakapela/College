using System.ComponentModel.DataAnnotations;

namespace ibhayiPharmacy.Models
{
    public class ActiveIngredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Active Ingredient Name")]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation Properties
        public virtual ICollection<MedicationActiveIngredient> MedicationActiveIngredients { get; set; }
            = new List<MedicationActiveIngredient>();
    }
}
