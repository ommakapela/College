using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ibhayiPharmacy.Models
{
    public class Medication
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Schedule")]
        public string Schedule { get; set; } // 0-6

        [Required]
        [Display(Name = "Dosage Form")]
        public int DosageFormId { get; set; }

        [Required]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Stock on Hand")]
        public int StockOnHand { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        [Display(Name = "Price")]
        public decimal Price { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        // Navigation Properties
        [ForeignKey("DosageFormId")]
        public virtual DosageForm DosageForm { get; set; }

        [ForeignKey("SupplierId")]
        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<MedicationActiveIngredient> MedicationActiveIngredients { get; set; }
            = new List<MedicationActiveIngredient>();

        public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; }
            = new List<PrescriptionItem>();

        // Computed Properties
        [Display(Name = "Needs Reorder")]
        public bool NeedsReorder => StockOnHand <= ReorderLevel;

        [Display(Name = "Stock Status")]
        public string StockStatus
        {
            get
            {
                if (StockOnHand == 0) return "Out of Stock";
                if (StockOnHand <= ReorderLevel) return "Low Stock";
                if (StockOnHand <= ReorderLevel + 10) return "Near Reorder Level";
                return "In Stock";
            }
        }

        // Helper property to get active ingredients as a list
        [NotMapped]
        public IEnumerable<ActiveIngredient> ActiveIngredients =>
            MedicationActiveIngredients?.Select(mai => mai.ActiveIngredient) ?? new List<ActiveIngredient>();
    }
}
