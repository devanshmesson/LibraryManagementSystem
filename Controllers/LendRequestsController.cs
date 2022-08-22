using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LoginPage1.Models;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace LoginPage1.Controllers
{
    public class LendRequestsController : Controller
    {
        private readonly LibraryDataContext _context;
        private readonly ILoginRepo _iloginrepo;

        public LendRequestsController(LibraryDataContext context, ILoginRepo _iloginrepo)
        {
            _context = context;
            this._iloginrepo = _iloginrepo;
        }

        // GET: LendRequests
        public async Task<IActionResult> Index()
        {
            var libraryDataContext = _context.LendRequests.Include(l => l.Book).Include(l => l.Account);
            return View(await libraryDataContext.ToListAsync());
        }

        // GET: LendRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lendRequest = await _context.LendRequests
                .Include(l => l.Book)
                .Include(l => l.Account)
                .FirstOrDefaultAsync(m => m.LendId == id);
            if (lendRequest == null)
            {
                return NotFound();
            }

            return View(lendRequest);
        }
        public IActionResult AdminDashboard()
        {
            var res = _context.LendRequests.Where(L=>L.LendStatus=="Requested").Include(L => L.Book).Include(_L => _L.Account);
            return View(res);
        }
        public IActionResult AddUserRequest(int BookId)
        {
            var user = HttpContext.Session.GetString("Username");
            var accountobject = _iloginrepo.GetUsername(user);
           

            
            if (accountobject != null && BookId>0) 
            {
                var record = _context.Books.Where(l => l.BookId == BookId).ToList();
                foreach (var item in record)
                {
                    item.NoOfCopies--;
                    
                }

                
                LendRequest l = new LendRequest()
                {
                    LendStatus = "Requested",
                    UserId = accountobject.UserId,
                    Bookid = BookId,
                    FineAmount = 0
                };
                _context.Add(l);
                _context.SaveChanges();
            }
            return RedirectToAction("BookUser","Books");
        }
        public IActionResult DeclineRequest(int LendId, int BookId)
        {
            var records = (from record in _context.LendRequests
             where record.LendId == LendId
             select record).ToList();
            //DateTime? nullDateTime = null;
            foreach (var record in records)
            {
                record.LendStatus = "Declined";
            }
            _context.Books.Where(l => l.BookId == BookId).ToList().ForEach(l => l.NoOfCopies++);

            _context.SaveChanges();
            return RedirectToAction("AdminDashboard", "LendRequests");
        }
        public IActionResult FilterRequest(int LendId)
        {
            //Updating the LendStatus to Accepted

            var records = (from record in _context.LendRequests.Include(l=>l.Book)
                           where record.LendId == LendId
                           select record).ToList();
            foreach(var item in records)
            {
                item.LendStatus = "Accepted";
                item.LendDate = DateTime.Now;
                item.Book.IssuedBooks++;
            }
            
            _context.SaveChanges();
            return RedirectToAction("AdminDashboard","LendRequests");
        }
        public IActionResult LentBooks(int LendId)
        {           
            var res = _context.LendRequests.Where(l=>l.LendStatus=="Accepted").Include(_l=> _l.Book).Include(r=>r.Account);
            return View(res.ToList());
        }
        public IActionResult LendReturnBooks()
        {
            var res = _context.LendRequests.Where(l => l.LendStatus == "Returned" || l.LendStatus == "Declined").Include(l => l.Book);
            return View(res);
        }
    
        public IActionResult UserRecords()
        {
            var user = HttpContext.Session.GetString("Username");
            var accountobject=_iloginrepo.GetUsername(user);
            var res = _context.LendRequests.Include(l => l.Book).Include(_l => _l.Account).Where(l => (l.LendStatus == "Returned" || l.LendStatus == "Declined") && l.UserId==accountobject.UserId);
            return View(res);
        }
        public IActionResult UserIssuedBooks()
        {
            var currentuser = HttpContext.Session.GetString("Username");
            Account a= _iloginrepo.GetUsername(currentuser);
            var res=_context.LendRequests.Where(l=>l.LendStatus=="Accepted" && l.UserId==a.UserId).Include(_l => _l.Book).Include(l => l.Account);
            return View(res);
        }
        public IActionResult ReturnBook(int BookId)
        {
            var currentuser = HttpContext.Session.GetString("Username");
            Account user = _iloginrepo.GetUsername(currentuser);

            var rec = (from record in _context.LendRequests.Include(l => l.Book)
                          where user.UserId == record.UserId && record.Book.BookId == BookId
                          select record).ToList();
            foreach(var item in rec)
            {
               item.LendStatus = "Returned";
               item.ReturnDate = DateTime.Now;
               TimeSpan timeSpan = item.ReturnDate - item.LendDate.AddDays(7);
               item.FineAmount = timeSpan.Days * 20;
               if(item.FineAmount < 0 )item.FineAmount = 0;    
            }
            var change = _context.Books.Where(l => l.BookId == BookId).ToList();
            foreach (var item in change)
            {
                item.NoOfCopies++;
                item.IssuedBooks--;
            }
            _context.SaveChanges();
            return RedirectToAction("BookUser","Books");

        }

        // GET: LendRequests/Create
        public IActionResult Create()
        {
            ViewData["Bookid"] = new SelectList(_context.Books, "BookId", "BookId");
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password");
            return View();
        }
    

        // POST: LendRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LendId,LendStatus,LendDate,ReturnDate,UserId,Bookid,FineAmount")] LendRequest lendRequest)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lendRequest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Bookid"] = new SelectList(_context.Books, "BookId", "BookId", lendRequest.Bookid);
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password", lendRequest.UserId);
            return View(lendRequest);
        }

        // GET: LendRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lendRequest = await _context.LendRequests.FindAsync(id);
            if (lendRequest == null)
            {
                return NotFound();
            }
            ViewData["Bookid"] = new SelectList(_context.Books, "BookId", "BookId", lendRequest.Bookid);
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password", lendRequest.UserId);
            return View(lendRequest);
        }

        // POST: LendRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LendId,LendStatus,LendDate,ReturnDate,UserId,Bookid,FineAmount")] LendRequest lendRequest)
        {
            if (id != lendRequest.LendId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(lendRequest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LendRequestExists(lendRequest.LendId))
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
            ViewData["Bookid"] = new SelectList(_context.Books, "BookId", "BookId", lendRequest.Bookid);
            ViewData["UserId"] = new SelectList(_context.Accounts, "UserId", "Password", lendRequest.UserId);
            return View(lendRequest);
        }

        // GET: LendRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lendRequest = await _context.LendRequests
                .Include(l => l.Book)
                .Include(l => l.Account)
                .FirstOrDefaultAsync(m => m.LendId == id);
            if (lendRequest == null)
            {
                return NotFound();
            }

            return View(lendRequest);
        }

        // POST: LendRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lendRequest = await _context.LendRequests.FindAsync(id);
            _context.LendRequests.Remove(lendRequest);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LendRequestExists(int id)
        {
            return _context.LendRequests.Any(e => e.LendId == id);
        }
    }
}
