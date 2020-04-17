using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using InoxicoHP.Models;
using Microsoft.Extensions.Options;

namespace InoxicoHP.Data
{
    public class repository
    {
        private DbContextOptionsBuilder<InoxicoHPContext> options = new DbContextOptionsBuilder<InoxicoHPContext>().UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=InoxicoHPContext-1;Trusted_Connection=True;MultipleActiveResultSets=true");
        private static Random random = new Random();

        public void AddCustomer(Customer customer)
        {
            using (var context = new InoxicoHPContext(options.Options))
            {
                context.Add(customer);
                context.SaveChanges();
            }
        }

        public string GenerateCustomerPaymentID(int customerID)
        {
            using (var context = new InoxicoHPContext(options.Options))
            {
                var customer = context.Customer.FirstOrDefault(cst => cst.Id == customerID);
                if (customer == null) return "";
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string paymentID = new string(Enumerable.Repeat(chars, 32)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
                customer.PaymentID = paymentID;
                context.Update(customer);
                context.SaveChanges();
                return paymentID;
            }
        }

        public void CustomerPaymentReceived(string paymentID)
        {
            using (var context = new InoxicoHPContext(options.Options))
            {
                var customer = context.Customer.FirstOrDefault(cst => cst.PaymentID == paymentID);
                customer.PaymentReceived = true;
                context.Update(customer);
                context.SaveChanges();
            }
        }

        public Business GetBusinessById(int? businessId)
        {
            if (businessId == null)
            {
                return null;
            }
            using (var context = new InoxicoHPContext(options.Options))
            {
                var business = context.Business.FirstOrDefault(bsn => bsn.Id == businessId);
                return business;
            }
        }

        public List<Business> GetBusinessList()
        {
            using (var context = new InoxicoHPContext(options.Options))
            {
                return context.Business.ToList();
            }
        }

        public BusinessInox GetBusinessInoxById(string? businessInoxId)
        {
            if (businessInoxId == null)
            {
                return null;
            }
            using (var context = new InoxicoHPContext(options.Options))
            {
                var businessIx = context.BusinessInox.FirstOrDefault(bsnIx => bsnIx.NoxID == businessInoxId);
                return businessIx;
            }
        }
    }
}
