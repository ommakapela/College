using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ibhayiPharmacy.Models
{
    public class PharmacistReport
    {
        [Key]
        public string PharmacistReportID {get;set;}
        [ForeignKey("MedicationId")]
        public string Medication { get; set; } 
        [ForeignKey("CustomerId")]
        public string CustomerName { get; set; }

    }
}
