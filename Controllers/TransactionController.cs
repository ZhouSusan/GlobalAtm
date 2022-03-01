using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using GlobalATM.Models;
using System.Net.Mail;
using System.Net;

namespace GlobalATM.Controllers
{

    public class TransactionController : Controller
    {

        private int? UUID
        {
            get
            {
                return HttpContext.Session.GetInt32("UserId");
            }
        }

        private bool isLoggedIn
        {
            get
            {
                return UUID != null;
            }
        }

        private readonly ILogger<HomeController> _logger;
        private MyContext db;

        public TransactionController(ILogger<HomeController> logger, MyContext context)
        {
            _logger = logger;
            db = context;
        }

        [HttpGet("Withdraw")]
        public IActionResult Withdraw()
        {
            if(!isLoggedIn)
            {
                return RedirectToAction("LogIn", "Home");
            }
            Account userAccount = db.Accounts
                                    .Include("Transactions")
                                    .FirstOrDefault(a => a.AccountNumber == HttpContext.Session.GetString("AccountNumber"));
            ViewBag.UserAccount = userAccount;                          
            return View("Withdraw");
        }

        [HttpGet("Deposit")]
        public IActionResult Deposit()
        {
            if(!isLoggedIn)
            {
                return RedirectToAction("LogIn", "Home");
            }
            Account userAccount = db.Accounts
                                    .Include("Transactions")
                                    .FirstOrDefault(a => a.AccountNumber == HttpContext.Session.GetString("AccountNumber"));
            ViewBag.UserAccount = userAccount;                          
            return View("Deposit");
        }

        [HttpPost("Add")]
        public IActionResult Add(double amount)
        {
            if (isLoggedIn)
            {
                User currentUser =  db.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
                Account userAccount = db.Accounts.Include("Transactions").FirstOrDefault(a => a.AccountNumber == HttpContext.Session.GetString("AccountNumber"));
                Transaction newTrans = null; 
                if (amount > 0)
                {
                    //Create a new isntance of an object of transaction
                    newTrans = new Transaction {
                    Amount = amount,
                    CreatedAt = DateTime.Now,
                    UserId = currentUser.UserId
                    };
                userAccount.Transactions.Add(newTrans);
                db.Add(newTrans);
                db.SaveChanges();
                return Redirect("Deposit");
                }
            }
        return View("Index");
        }

        [HttpPost("Subtract")]
        public IActionResult Subtract(double amount)
        {
            if (isLoggedIn)
            {
                User currentUser =  db.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
                Account userAccount = db.Accounts.Include("Transactions").FirstOrDefault(a => a.AccountNumber == HttpContext.Session.GetString("AccountNumber"));
                Transaction newTrans = null; 
                amount = amount* -1;
            if (amount + userAccount.Balance < 0)
                {
                    ModelState.AddModelError("Amount", "Insufficient funds");
                    return RedirectToAction("Index");
                }
                else 
                {
                    newTrans = new Transaction {
                    Amount = amount,
                    CreatedAt = DateTime.Now,
                    UserId = currentUser.UserId
                };
                    userAccount.Transactions.Add(newTrans);
                    db.Add(newTrans);
                    db.SaveChanges();
                    return Redirect("Withdraw");
                }
            }

            return View("Index");
        }


        [HttpGet("/transactions/currency-converter")]
        public IActionResult CurrencyConverter()
        {
            if(!isLoggedIn)
            {
                return RedirectToAction("LogIn", "Home");
            }

            return View("CurrencyConverter");
        }

        // *****************************************************//
        // Transaction Receipt

        [HttpGet("/receipt-button")]
        public IActionResult ReceiptButton()
        {
            return View("SendReceipt");
        }

        [HttpPost("/send-receipt")]
        public async Task<ActionResult> SendReceipt() //Can pass variables into function
        {
            // String body = "<p>Email From: {0} ({1})</p><p>Message:</p><p>{2}</p>";
            MailMessage message = new MailMessage();
            message.To.Add(new MailAddress("teamawesome2022@mail.com")); //will replace with user email
            message.From = new MailAddress("CSharpGlobalBank@mail.com");
            message.Subject = "Do Not Reply - CSharp Online Banking System - Your automated transaction receipt";
            message.Body = "<p> Message: testing 123 Here is the receipt for your transaction </p>"; //will replace with transaction details.
            message.IsBodyHtml = true;

            using (var smtp = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = "CSharpGlobalBank@mail.com",
                    Password = System.IO.File.ReadAllText("EmailSenderPW.txt"),
                };
                smtp.Credentials = credential;
                smtp.Host = "smtp.mail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
                return RedirectToAction("ReceiptButton");
            }
        }
    }
}
