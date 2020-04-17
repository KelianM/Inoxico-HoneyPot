using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InoxicoHP.Models
{
    public class Customer
    {
        public int Id { get; set; }
        [Display(Name = "Full Name")]
        [Required]
        [StringLength(30)]
        public string FullName { get; set; }
        [Required]
        [StringLength(30)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        //[RegularExpression(@"^[A-Z]+[a-zA-Z""'\s-]*$")]
        [Required]
        [StringLength(30)]
        [Display(Name = "Company Country")]
        public string CompanyCountry { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        public bool PaymentReceived { get; set; } = false;
        public string PaymentID { get; set; }
    }
}