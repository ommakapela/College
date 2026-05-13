using System.ComponentModel.DataAnnotations;

namespace ibhayiPharmacy.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100)]
        public string Surname { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Registration Number")]
        public string RegistrationNumber { get; set; } // Health Council Registration Number

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Practice Number")]
        public string? PracticeNumber { get; set; }

        // Computed Property
        [Display(Name = "Full Name")]
        public string FullName => $"Dr. {Name} {Surname}";

        // Navigation Properties
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
