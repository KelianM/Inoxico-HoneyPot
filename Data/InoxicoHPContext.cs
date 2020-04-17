using Microsoft.EntityFrameworkCore;
using InoxicoHP.Models;

namespace InoxicoHP.Data
{
    public class InoxicoHPContext : DbContext
    {
        public InoxicoHPContext(DbContextOptions<InoxicoHPContext> options)
            : base(options)
        {
        }

        public DbSet<Business> Business { get; set; }
        public DbSet<BusinessInox> BusinessInox { get; set; }
        public DbSet<Customer> Customer { get; set; }

    }
}   

