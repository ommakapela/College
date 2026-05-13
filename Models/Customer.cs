using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ibhayiPharmacy.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        [ForeignKey("DoctorId")]
        public string DoctorName { get;set; }
        [ForeignKey("PrescriptionId")]
        public string PrescriptionDate { get;set; }
        [ForeignKey("PrescriptionId")]
        public string Quantity { get;set; }
        [ForeignKey("PrescriptionId")]  
        public string Instruction { get;set; }
        [ForeignKey("PrescriptionId")] 
        public string Repeats { get;set; }
        [ForeignKey("PrescriptionId")]  
        public string PaymentMethod { get;set; }
        [ForeignKey("PrescriptionId")]  
        public string Notes { get;set; }
        [ForeignKey("PrescriptionId")] 
        public string Strength { get;set; }
        [ForeignKey("PrescriptionId")] 
        public string DosageName { get;set; }
        [ForeignKey("DosageId")]
        public string PrescriptionName { get;set; }
        [ForeignKey("PrescriptionId")]
        public string CustomerLastName { get; set; }
        public string CustomerGender { get; set; }
        public string CustomerCell { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerIDNumber { get; set; } 
        public string InsuranceName { get; set; } 
        public int Cost { get; set; }
    }
}
