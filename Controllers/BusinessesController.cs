using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InoxicoHP.Data;
using InoxicoHP.Models;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PayFast;
using PayFast.AspNetCore;
using System.Text;
using Mono.Web;
using System.Globalization;
using System.Collections.Generic;

namespace InoxicoHP.Controllers
{
    public class BusinessesController : Controller
    {
        #region Fields
        private readonly InoxicoHPContext _context;
        private readonly PayFastSettings payFastSettings;
        private readonly ILogger logger;
        repository repo = new repository();
        #endregion Fields

        #region Constructor
        /*
        public BusinessesController(InoxicoHPContext context)
        {
            _context = context;
        }

        public BusinessesController(IOptions<PayFastSettings> payFastSettings, ILogger<BusinessesController> logger)
        {
            this.payFastSettings = payFastSettings.Value;
            this.logger = logger;
        }
        */
        public BusinessesController(InoxicoHPContext context, IOptions<PayFastSettings> payFastSettings, ILogger<BusinessesController> logger)
        {
            this.payFastSettings = payFastSettings.Value;
            this.logger = logger;
            _context = context;
        }
        #endregion Constructor

        #region Methods
        // GET: Businesses
        public async Task<IActionResult> Index()
        {
            return View(await _context.Business.ToListAsync());
        }

        public async Task<IActionResult> BusinessDetails(string? id)
        {
            var businessInox = repo.GetBusinessInoxById(id);
            
            if (businessInox == null)
            {
                return NotFound();
            }/*
            var businessInox = new BusinessInox
            {
                Country = Countries.RSA,
                Name = "Cars",
                NoxID = id
            };*/
            ViewBag.countryID = (int)businessInox.Country;
            TempData["NoxID"] = businessInox.NoxID;
            TempData["Name"] = businessInox.Name;
            TempData.Keep();
            return View(repo.GetBusinessList());
            /*return View(new List<Business>
            {
                new Business 
                {
                    Id = 1,
                    Country = Countries.RSA,
                    Product = Products.QuickView,
                    Description = "It's quick",
                    Available = Choice.Yes,
                    Turnaround = 1,
                    Price = 7.99M
                },
                new Business
                {
                    Id = 2,
                    Country = Countries.RSA,
                    Product = Products.Research,
                    Description = "It's slow",
                    Available = Choice.Yes,
                    Turnaround = 8,
                    Price = 900.00M
                }
            });*/
        }

        // GET: Businesses/Purchase
        public async Task<IActionResult> Purchase(int? id)
        {
            var business = repo.GetBusinessById(id);
            if (business == null)
            {
                return NotFound();
            }/*
            var business = new Business
            {
                Id = 2,
                Country = Countries.RSA,
                Product = Products.Research,
                Description = "It's slow",
                Available = Choice.Yes,
                Turnaround = 8,
                Price = 900.00M
            };*/
            TempData["Product"] = (business.Product).ToString();
            TempData["Price"] = business.Price.ToString();
            TempData["NoxID"] = TempData["NoxID"];
            TempData["Name"] = TempData["Name"];
            TempData.Keep();
            return View();
        }

        // POST: Businesses/Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase([Bind("Id,FullName,CompanyName,CompanyCountry,EmailAddress")] Customer customer)
        {
            //var errors = ModelState.Values.SelectMany(v => v.Errors);
            customer.Id = 0; // Error where customer Id gets set to the business ID, temporary fix
            if (ModelState.IsValid)
            {
                repo.AddCustomer(customer);
                TempData["CustomerID"] = customer.Id;
                TempData["Product"] = TempData["Product"];
                TempData["Price"] = TempData["Price"];
                TempData["NoxID"] = TempData["NoxID"];
                TempData["Name"] = TempData["Name"];
                TempData.Keep(); 
                return RedirectToAction(nameof(OnceOff));
            }
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Notify([ModelBinder(BinderType = typeof(PayFastNotifyModelBinder))]PayFastNotify payFastNotifyViewModel)
        {
            if (payFastNotifyViewModel != null) Ok();
            int CustomerID = (int)TempData["CustomerID"];
            payFastNotifyViewModel.SetPassPhrase(this.payFastSettings.PassPhrase);
            var calculatedSignature = payFastNotifyViewModel.GetCalculatedSignature();
            var isValid = payFastNotifyViewModel.signature == calculatedSignature;

            logger.LogInformation($"Signature Validation Result: {isValid}");

            var payfastValidator = new PayFastValidator(this.payFastSettings, payFastNotifyViewModel, this.HttpContext.Connection.RemoteIpAddress);

            var merchantIdValidationResult = payfastValidator.ValidateMerchantId();

            logger.LogInformation($"Merchant Id Validation Result: {merchantIdValidationResult}");

            var ipAddressValidationResult = await payfastValidator.ValidateSourceIp();

            logger.LogInformation($"Ip Address Validation Result: {ipAddressValidationResult}");
            // Currently seems that the data validation only works for success
            if (payFastNotifyViewModel.payment_status == PayFastStatics.CompletePaymentConfirmation)
            {
                var dataValidationResult = await payfastValidator.ValidateData();

                logger.LogInformation($"Data Validation Result: {dataValidationResult}");
            }

            if (payFastNotifyViewModel.payment_status == PayFastStatics.CancelledPaymentConfirmation)
            {
                logger.LogInformation($"Subscription was cancelled");
            }
            repo.CustomerPaymentReceived(payFastNotifyViewModel.m_payment_id);
            return Ok();
        }

        public IActionResult OnceOff()
        {
            TempData["Product"] = TempData["Product"];
            TempData["Price"] = TempData["Price"];
            TempData["NoxID"] = TempData["NoxID"];
            TempData["Name"] = TempData["Name"];
            TempData.Keep();
            var onceOffRequest = new PayFastRequest(this.payFastSettings.PassPhrase);
            // Merchant Details
            onceOffRequest.merchant_id = this.payFastSettings.MerchantId;
            onceOffRequest.merchant_key = this.payFastSettings.MerchantKey;
            onceOffRequest.return_url = this.payFastSettings.ReturnUrl;
            onceOffRequest.cancel_url = this.payFastSettings.CancelUrl;
            onceOffRequest.notify_url = this.payFastSettings.NotifyUrl;

            // Buyer Details
            //onceOffRequest.email_address = "sbtu01@payfast.co.za";

            // Transaction Details
            onceOffRequest.m_payment_id = repo.GenerateCustomerPaymentID((int)TempData["CustomerID"]);
            onceOffRequest.amount = Convert.ToDouble(TempData["Price"]);
            onceOffRequest.item_name = TempData["Product"].ToString();
            onceOffRequest.item_description = "Once off payment for " + TempData["Name"] + TempData["Product"];
            // Transaction Options
            //onceOffRequest.email_confirmation = true;
            //onceOffRequest.confirmation_address = "sbtu01@payfast.co.za";

            StringBuilder str = new StringBuilder();
            str.Append("merchant_id=" + HttpUtility.UrlEncode(onceOffRequest.merchant_id));
            str.Append("&merchant_key=" + HttpUtility.UrlEncode(onceOffRequest.merchant_key));
            str.Append("&return_url=" + HttpUtility.UrlEncode(onceOffRequest.return_url));
            str.Append("&cancel_url=" + HttpUtility.UrlEncode(onceOffRequest.cancel_url));
            str.Append("&notify_url=" + HttpUtility.UrlEncode(onceOffRequest.notify_url));

            str.Append("&m_payment_id=" + HttpUtility.UrlEncode(onceOffRequest.m_payment_id));
            str.Append("&amount=" + HttpUtility.UrlEncode(onceOffRequest.amount.ToString("G", CultureInfo.InvariantCulture)));
            str.Append("&item_name=" + HttpUtility.UrlEncode(onceOffRequest.item_name));
            str.Append("&item_description=" + HttpUtility.UrlEncode(onceOffRequest.item_description));
            var redirectUrl = $"{this.payFastSettings.ProcessUrl}{str.ToString()}";

            return Redirect(redirectUrl);
        }

        public IActionResult Return()
        {
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }


        /*
        public async Task<IActionResult> PurchaseSuccessful()
        {
            int CustomerID = (int)TempData["CustomerID"];
            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.Id == CustomerID);
            customer.paymentReceived = true;
            try
            {
                _context.Update(customer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return View();
        }

        // GET: Businesses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Business
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // GET: Businesses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Businesses/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Country,Product,Description,Available,Turnaround,Price")] Business business)
        {
            if (ModelState.IsValid)
            {
                _context.Add(business);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(business);
        }

        // GET: Businesses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Business.FindAsync(id);
            if (business == null)
            {
                return NotFound();
            }
            return View(business);
        }

        // POST: Businesses/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Country,Product,Description,Available,Turnaround,Price")] Business business)
        {
            if (id != business.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(business);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusinessExists(business.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(business);
        }

        // GET: Businesses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var business = await _context.Business
                .FirstOrDefaultAsync(m => m.Id == id);
            if (business == null)
            {
                return NotFound();
            }

            return View(business);
        }

        // POST: Businesses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var business = await _context.Business.FindAsync(id);
            _context.Business.Remove(business);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/

        private bool BusinessExists(int id)
        {
            return _context.Business.Any(e => e.Id == id);
        }
        #endregion Methods
    }
}
