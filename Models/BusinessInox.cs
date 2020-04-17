using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InoxicoHP.Models
{
    public class BusinessInox
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Countries Country { get; set; }
        public string NoxID { get; set; }
    }
}