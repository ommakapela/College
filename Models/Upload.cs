using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ibhayiPharmacy.Models
{
    public class Upload
    {
        [Key] 
        public int UploadId { get; set; }
        [ForeignKey("CustomerId")]
        public string CustomerName { get; set; }
        [ForeignKey("DoctorId")]
        public string DoctorName { get; set; } 
        [ForeignKey("MedicationId")]
        public string MedicationName { get; set; }
    }
}
