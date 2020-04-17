using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InoxicoHP.Data;
using System;
using System.Linq;

namespace InoxicoHP.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            //var req = serviceProvider.GetRequiredService<
                    //DbContextOptions<InoxicoHPContext>>();
            using (var context = new InoxicoHPContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<InoxicoHPContext>>()))
            {
                // Look for any movies.
                if (context.Business.Any())
                {
                    return;   // DB has been seeded
                }

                context.Business.AddRange(
                    new Business
                    {
                        Country = Countries.RSA,
                        Product = Products.QuickView,
                        Description = "It's quick",
                        Available = Choice.Yes,
                        Turnaround = 1,
                        Price = 7.99M
                    },

                    new Business
                    {
                        Country = Countries.RSA,
                        Product = Products.Research,
                        Description = "It's slow",
                        Available = Choice.Yes,
                        Turnaround = 8,
                        Price = 900.00M
                    },
                    new Business
                    {
                        Country = Countries.Angola,
                        Product = Products.QuickView,
                        Description = "It's quick",
                        Available = Choice.Yes,
                        Turnaround = 1,
                        Price = 5.99M
                    },

                    new Business
                    {
                        Country = Countries.Angola,
                        Product = Products.Research,
                        Description = "It's slow",
                        Available = Choice.No,
                        Turnaround = 7,
                        Price = 500.00M
                    }
                );
                context.SaveChanges();
            }
        }
    }
}