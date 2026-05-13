using System.ComponentModel.DataAnnotations;

namespace ibhayiPharmacy.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Supplier Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Contact Person Name")]
        public string ContactName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Contact Person Surname")]
        public string ContactSurname { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        // Computed Property
        [Display(Name = "Contact Person")]
        public string ContactFullName => $"{ContactName} {ContactSurname}";

        // Navigation Properties
        public virtual ICollection<Medication> Medications { get; set; } = new List<Medication>();
    }
}
