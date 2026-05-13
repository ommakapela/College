using ibhayiPharmacy.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ibhayiPharmacy.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Prescription Date")]
        public DateTime PrescriptionDate { get; set; }

        // NEW: Track when uploaded
        [Display(Name = "Upload Date")]
        public DateTime UploadDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(200)]
        [Display(Name = "Patient Name")]
        public string PatientName { get; set; }

        // NEW: Optional patient ID
        [StringLength(13)]
        [Display(Name = "Patient ID Number")]
        public string? PatientIDNumber { get; set; }

        // Foreign Keys
        [Required]
        public string CustomerId { get; set; }

        // CHANGED: Now nullable - filled by pharmacist during processing
        [Display(Name = "Doctor")]
        public int? DoctorId { get; set; }  // Changed from [Required] int to int?

        // PDF File Storage
        public byte[]? PrescriptionFileData { get; set; }

        [StringLength(500)]
        public string? FileName { get; set; }

        [StringLength(100)]
        public string? ContentType { get; set; }

        // Status for dispensing workflow
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending Processing";  // Changed default

        // NEW: Workflow tracking fields
        [Display(Name = "Uploaded By")]
        public string? UploadedById { get; set; }

        [Display(Name = "Processed By")]
        public string? ProcessedById { get; set; }

        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }

        [Display(Name = "Dispensed By")]
        public string? DispensedById { get; set; }

        [Display(Name = "Dispensed Date")]
        public DateTime? DispensedDate { get; set; }

        [Display(Name = "Is Dispensed")]
        public bool IsDispensed { get; set; } = false;

        [Display(Name = "Ready for Collection")]
        public bool ReadyForCollection { get; set; } = false;

        [Display(Name = "Customer Notified")]
        public bool CustomerNotified { get; set; } = false;

        [Display(Name = "Is Collected")]
        public bool IsCollected { get; set; } = false;

        [Display(Name = "Collection Date")]
        public DateTime? CollectionDate { get; set; }

        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public virtual ApplicationUser? Customer { get; set; }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        // NEW: Track who uploaded/processed/dispensed
        [ForeignKey("UploadedById")]
        public virtual ApplicationUser? UploadedBy { get; set; }

        [ForeignKey("ProcessedById")]
        public virtual ApplicationUser? ProcessedBy { get; set; }

        [ForeignKey("DispensedById")]
        public virtual ApplicationUser? DispensedBy { get; set; }

        public virtual ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

        // Computed Properties
        [NotMapped]
        public string PrescriptionId => $"RX{Id:D6}";

        [NotMapped]
        public bool IsPending => Status == "Pending Processing";

        [NotMapped]
        public bool IsProcessed => Status == "Processed";

        [NotMapped]
        public bool HasMedications => Items != null && Items.Any();

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            "Pending Processing" => "badge-warning",
            "Processed" => "badge-success",
            "Dispensed" => "badge-info",
            "Ready for Collection" => "badge-primary",
            "Collected" => "badge-secondary",
            _ => "badge-dark"
        };

        [NotMapped]
        public string StatusIcon => Status switch
        {
            "Pending Processing" => "fa-clock",
            "Processed" => "fa-check-circle",
            "Dispensed" => "fa-box",
            "Ready for Collection" => "fa-gift",
            "Collected" => "fa-check-double",
            _ => "fa-question-circle"
        };
    }
} 