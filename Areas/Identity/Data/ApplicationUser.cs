// ==================== AREAS/IDENTITY/DATA/ApplicationUser.cs ====================
using ibhayiPharmacy.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ibhayiPharmacy.Areas.Identity.Data
{
    /// <summary>
    /// Custom user model extending IdentityUser with pharmacy-specific fields
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // ==================== PERSONAL INFORMATION ====================

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
        [Display(Name = "First Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Surname is required")]
        [StringLength(100, ErrorMessage = "Surname cannot exceed 100 characters")]
        [Display(Name = "Surname")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "ID Number is required")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "ID Number must be exactly 13 digits")]
        [Display(Name = "ID Number")]
        public string IdNumber { get; set; } // Changed from IdNumber to match database

        // ==================== CONTACT INFORMATION ====================

        [Display(Name = "Cellphone")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Cellphone { get; set; } // Maps to PhoneNumber in most cases, but keeping separate

        // ==================== MEDICAL INFORMATION ====================

        [StringLength(1000, ErrorMessage = "Allergies list cannot exceed 1000 characters")]
        [Display(Name = "Allergies to Active Ingredients")]
        public string? Allergies { get; set; } // Comma-separated list of active ingredient names

        // ==================== PROFESSIONAL INFORMATION (for Pharmacists/Doctors) ====================

        [StringLength(50)]
        [Display(Name = "Health Council Registration Number")]
        public string? HealthCouncilRegNumber { get; set; } // For pharmacists and doctors

        // ==================== SECURITY ====================

        [Display(Name = "Must Change Password")]
        public bool MustChangePassword { get; set; } = false;

        // ==================== INHERITED FROM IdentityUser ====================
        // The following properties are already available from IdentityUser base class:

        // public override string Id { get; set; }                    // Unique user ID (GUID)
        // public override string UserName { get; set; }              // Username (we use Email)
        // public override string Email { get; set; }                 // Email address
        // public override bool EmailConfirmed { get; set; }          // Email confirmation status
        // public override string PhoneNumber { get; set; }           // Phone number from Identity
        // public override bool PhoneNumberConfirmed { get; set; }    // Phone confirmation status
        // public override string PasswordHash { get; set; }          // Hashed password
        // public override string SecurityStamp { get; set; }         // Security token
        // public override bool TwoFactorEnabled { get; set; }        // 2FA status
        // public override bool LockoutEnabled { get; set; }          // Lockout status
        // public override DateTimeOffset? LockoutEnd { get; set; }   // Lockout end time
        // public override int AccessFailedCount { get; set; }        // Failed login attempts

        // ==================== COMPUTED PROPERTIES ====================

        /// <summary>
        /// Gets the user's full name
        /// </summary>
        [Display(Name = "Full Name")]
        public string FullName => $"{Name} {Surname}";

        /// <summary>
        /// Checks if user has any allergies recorded
        /// </summary>
        public bool HasAllergies => !string.IsNullOrWhiteSpace(Allergies);

        /// <summary>
        /// Gets list of individual allergens
        /// </summary>
        public string[] AllergyList => HasAllergies
            ? Allergies.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(a => a.Trim())
                       .ToArray()
            : Array.Empty<string>();

        // ==================== NAVIGATION PROPERTIES ====================

        /// <summary>
        /// Collection of prescriptions associated with this customer
        /// </summary>
        public virtual ICollection<Prescription>? Prescriptions { get; set; }
    }
}

// ==================== USAGE NOTES ====================
/*
 * FIELD MAPPING FROM REGISTRATION FORM:
 * 
 * Registration Field    →  ApplicationUser Property  →  Database Column
 * -------------------      -------------------------     ----------------
 * Name                  →  Name                       →  Name
 * Surname               →  Surname                    →  Surname
 * IDNumber              →  IDNumber                   →  IDNumber
 * Email                 →  Email                      →  Email
 * Email                 →  UserName                   →  UserName (same as email)
 * Cellphone             →  Cellphone                  →  Cellphone
 * PhoneNumber           →  PhoneNumber                →  PhoneNumber (from IdentityUser)
 * Allergies             →  Allergies                  →  Allergies
 * HealthCouncilRegNumber→  HealthCouncilRegNumber     →  HealthCouncilRegNumber
 * MustChangePassword    →  MustChangePassword         →  MustChangePassword
 * Password              →  PasswordHash               →  PasswordHash (hashed by Identity)
 * 
 * IMPORTANT NOTES:
 * 1. Both Cellphone and PhoneNumber exist - use Cellphone for display, sync with PhoneNumber if needed
 * 2. Email is used as UserName (set in Register.cshtml.cs)
 * 3. Password is automatically hashed by Identity - never stored as plain text
 * 4. Id is auto-generated GUID by Identity
 * 5. Allergies are stored as comma-separated string (e.g., "Penicillin, Aspirin, Codeine")
 * 6. IDNumber changed to match exact database column name
 * 7. HealthCouncilRegNumber is for pharmacists and doctors only
 * 8. MustChangePassword flag for forcing password reset on first login
 */