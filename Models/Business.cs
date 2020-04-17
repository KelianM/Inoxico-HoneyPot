using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InoxicoHP.Models
{
    public enum Countries {RSA, Angola, Nigeria, Zambia}
    public enum Products {QuickView, Research}
    public enum Choice { Yes, No }

    public class Business
    {
        public int Id { get; set; }
        public Countries Country { get; set; }
        public Products Product { get; set; }
        public string Description { get; set; }
        public Choice Available { get; set; }
        public int Turnaround { get; set; }

        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
    }
}