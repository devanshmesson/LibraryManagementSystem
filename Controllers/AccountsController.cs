using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoginPage1.Models;
using Microsoft.AspNetCore.Http;

namespace LoginPage1.Controllers
{
    public class AccountsController : Controller
    {
        private readonly LibraryDataContext _context;
        private readonly ILoginRepo _user;

        public AccountsController(LibraryDataContext context, ILoginRepo _user)
        {
            _context = context;
            this._user = _user;
        }

        // GET: Accounts

        public IActionResult Login(string username, string password)
        {
            
            //SetString(key,value)  value is that we enter in the login form (see in parameters)
            if (username != null && password != null)
            {
                
                var user = _user.GetUsername(username);
                if (user == null)
                {
                    ViewBag.Message = "⚠ Invalid Credentials, Please Try Again!";
                    
                }
                if (username.Equals("admin") && password.Equals("admin"))
                {
                    return RedirectToAction("AdminDashboard", "LendRequests");
                }
                else if (user!=null && username.Equals(user.UserName) && password.Equals(user.Password))
                {
                    HttpContext.Session.SetString("Username", username);
                    return RedirectToAction("BookUser", "Books");
                }
                else
                {
                    ViewBag.Message = "⚠ Invalid Credentials, Please Try Again!";
                    return View();
                }
            }
            return View();
        }
        public IActionResult Index()
        {

            var usr = from us in _context.Accounts
                      select us;
            return View(usr.ToList());
        }

    // GET: Accounts/Details/5
    public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Accounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,UserName,Password,Role")] Account account)
        {
            if (ModelState.IsValid)
            {
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,UserName,Password,Role")] Account account)
        {
            if (id != account.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AccountExists(account.UserId))
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
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AccountExists(int id)
        {
            return _context.Accounts.Any(e => e.UserId == id);
        }
    }
}
