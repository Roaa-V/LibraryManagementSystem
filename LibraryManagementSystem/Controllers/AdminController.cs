using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryManagement.Controllers
{
    public class AdminController : Controller
    {
        private readonly MyDbContext _context;

        public AdminController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminLogin()
        {
            return View();
        }

        // ✅ Show all books
        public IActionResult ManageBooks()
        {
            var books = _context.Books.ToList();
            return View(books);
        }

        public IActionResult BorrowRequests()
        {
            var requests = _context.BorrowedBooks
                .Include(b => b.Book)
                .Include(b => b.Student)
                .Where(b => b.IsConfirmedByAdmin == false)
                .ToList();

            return View(requests);
        }

        public IActionResult ApproveRequest(int id)
        {
            var request = _context.BorrowedBooks
                .Include(b => b.Book)
                .FirstOrDefault(b => b.Id == id);

            if (request != null)
            {
                request.IsConfirmedByAdmin = true;

                // Reduce available copies
                if (request.Book.CopiesAvailable > 0)
                {
                    request.Book.CopiesAvailable--;
                }

                _context.SaveChanges();
            }

            return RedirectToAction("BorrowRequests");
        }

        public IActionResult RejectRequest(int id)
        {
            var request = _context.BorrowedBooks.FirstOrDefault(b => b.Id == id);

            if (request != null)
            {
                _context.BorrowedBooks.Remove(request);
                _context.SaveChanges();
            }

            return RedirectToAction("BorrowRequests");
        }



        public IActionResult ManageRooms()
        {
            var rooms = _context.MeetingRooms.ToList();
            return View(rooms);
        }



        // GET to create room
        public IActionResult CreateRoom()
        {
            return View();
        }

        // POST to create room
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateRoom(MeetingRoom room)
        {
            if (ModelState.IsValid)
            {
                _context.MeetingRooms.Add(room);
                _context.SaveChanges();
                return RedirectToAction("ManageRooms");
            }
            return View(room);
        }


        // Show pending room reservations
        public IActionResult RoomRequests()
        {
            var requests = _context.RoomReservations
                .Include(r => r.Room)
                .Include(r => r.Student)
                .Where(r => r.IsConfirmedByAdmin == false)
                .ToList();

            return View(requests);
        }

        public IActionResult ApproveRoom(int id)
        {
            var reservation = _context.RoomReservations.FirstOrDefault(r => r.Id == id);
            if (reservation != null)
            {
                reservation.IsConfirmedByAdmin = true;
                _context.SaveChanges();
            }
            return RedirectToAction("RoomRequests");
        }

        public IActionResult RejectRoom(int id)
        {
            var reservation = _context.RoomReservations.FirstOrDefault(r => r.Id == id);
            if (reservation != null)
            {
                _context.RoomReservations.Remove(reservation);
                _context.SaveChanges();
            }
            return RedirectToAction("RoomRequests");
        }

        public IActionResult ConfirmRoom(int id)
        {
            var reservation = _context.RoomReservations.Find(id);

            if (reservation != null)
            {
                reservation.IsConfirmedByAdmin = true;
                _context.SaveChanges();
            }

            return RedirectToAction("RoomRequests");
        }


        public IActionResult ReviewFeedback()
        {
            var feedbacks = _context.Feedbacks
                .Include(f => f.Student)
                .ToList();

            return View(feedbacks);
        }

        public IActionResult MarkAsReviewed(int id)
        {
            var fb = _context.Feedbacks.FirstOrDefault(f => f.Id == id);
            if (fb != null)
            {
                fb.IsReviewed = true;
                _context.SaveChanges();
            }
            return RedirectToAction("ReviewFeedback");
        }

        public IActionResult ExportReports()
        {
            return View();
        }

        // GET
        public IActionResult CreateBook()
        {
            return View();
        }

        // POST
        [HttpPost]
        public IActionResult CreateBook(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Add(book);
                _context.SaveChanges();
                return RedirectToAction("ManageBooks");
            }

            return View(book);
        }


        // GET
        public IActionResult EditBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditBook(Book book)
        {
            if (ModelState.IsValid)
            {
                _context.Books.Update(book);
                _context.SaveChanges();
                return RedirectToAction("ManageBooks");
            }

            return View(book);
        }

        public IActionResult DeleteBook(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);

            if (book == null)
                return NotFound();

            return View(book);
        }


        [HttpPost, ActionName("DeleteBook")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteBookConfirmed(int id)
        {
            var book = _context.Books.FirstOrDefault(b => b.Id == id);

            if (book != null)
            {
                _context.Books.Remove(book);
                _context.SaveChanges();
            }

            return RedirectToAction("ManageBooks");
        }

    }
}
