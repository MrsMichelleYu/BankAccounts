using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BankAccounts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace BankAccounts.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;
        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("register")]
        public IActionResult Register(User thisUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == thisUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                thisUser.Password = Hasher.HashPassword(thisUser, thisUser.Password);
                dbContext.Users.Add(thisUser);
                dbContext.SaveChanges();
                HttpContext.Session.SetInt32("UserID", thisUser.UserId);
                return RedirectToAction("Account", new { Id = thisUser.UserId });
            }
            return View("Index");
        }

        [HttpGet("/account/{Id}")] //page that shows the list of transaction and balance
        public IActionResult Account(int Id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                ModelState.AddModelError("Email", "You must be logged in first");
                return View("Login");
            }
            int? ID = HttpContext.Session.GetInt32("UserID");
            User someUser = dbContext.Users
                .Include(user => user.CreatedTransaction)
                .FirstOrDefault(user => user.UserId == Id);
            someUser.CreatedTransaction = someUser.CreatedTransaction.OrderByDescending(t => t.CreatedAt).ToList();
            if (HttpContext.Session.GetString("NoMoney") != null)
            {
                ModelState.AddModelError("Amount", HttpContext.Session.GetString("NoMoney"));
                HttpContext.Session.Remove("NoMoney");//once message in session is shown, remove that message key and value from session.
                ViewBag.User = someUser;
                ViewBag.Transaction = someUser.CreatedTransaction;
                return View("Account");
            }
            ViewBag.User = someUser;
            ViewBag.Transaction = someUser.CreatedTransaction;
            return View();
        }

        [HttpPost("add")]
        public IActionResult Add(Transaction submittedInfo)
        {
            int? ID = HttpContext.Session.GetInt32("UserID");
            User someUser = dbContext.Users
                .Include(user => user.CreatedTransaction)
                .FirstOrDefault(user => user.UserId == ID);
            someUser.CreatedTransaction = someUser.CreatedTransaction.OrderByDescending(t => t.CreatedAt).ToList();
            Decimal total = 0.00M;
            foreach (var transaction in someUser.CreatedTransaction)
            {
                total += transaction.Amount;// to find current total balance
            }
            if (total + submittedInfo.Amount < 0)
            {
                HttpContext.Session.SetString("NoMoney", "You do not have enough funds in this account to make this transaction");
                return RedirectToAction("Account", new { Id = ID });
                // ModelState.AddModelError("Amount", "You do not have enough funds in this account to make this transaction");
                // ViewBag.User = someUser;
                // ViewBag.Transaction = someUser.CreatedTransaction;
                // return View("Account");
            }
            dbContext.Transactions.Add(submittedInfo);
            dbContext.SaveChanges();
            return RedirectToAction("Account", new { Id = ID });
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("enter")] //log into the site 
        public IActionResult Enter(LoginUser userSubmission)
        {
            if (ModelState.IsValid)
            {
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
                if (userInDb == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Login");
                }
                var hasher = new PasswordHasher<LoginUser>();
                var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);
                if (result == 0)// result is a failure
                {
                    ModelState.AddModelError("Password", "Invalid Email/Password");
                    return View("Login");
                }
                HttpContext.Session.SetInt32("UserID", userInDb.UserId);
                return RedirectToAction("Account", new { Id = userInDb.UserId });
            }
            return View("Login");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Login");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
