using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

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

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Admin admin)
        {
            if (_context.Admins.Any(a => a.Email == admin.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(admin);
            }

            if (string.IsNullOrWhiteSpace(admin.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                return View(admin);
            }

            // Hash the password
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.Password);

            _context.Admins.Add(admin);
            _context.SaveChanges();

            // Redirect to login
            return RedirectToAction("Index");
        }

        public IActionResult Login()
        {
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Email == email);

            if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            // Set session for logged-in admin
            HttpContext.Session.SetInt32("AdminId", admin.Id);

            // Redirect to Admin dashboard
            return RedirectToAction("Index" , "Admin");
        }

        // GET: Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminId");
            return RedirectToAction("Index", "Home");
        }

        // ✅ Show all books
        public IActionResult ManageBooks()
        {
            ViewData["ExportAction"] = "ExportReports";

            var books = _context.Books.ToList();
            return View(books);
        }

        public IActionResult BorrowRequests()
        {
            ViewData["ExportAction"] = "ExportReports";
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

            ViewData["ExportAction"] = "ExportReports";

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
            ViewData["ExportAction"] = "ExportReports";

            var requests = _context.RoomReservations
                .Include(r => r.Room)
                .Include(r => r.Student)
                .Where(r => r.IsConfirmedByAdmin == false)
                .ToList();

            return View(requests);
        }

        public IActionResult ApproveRoom(int id)
        {
            var reservation = _context.RoomReservations.Find(id);
            
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

  


        public IActionResult ReviewFeedback()
        {
            ViewData["ExportAction"] = "ExportReports";

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


        public IActionResult ExportReports(string type = "borrowings", string search = null)
        {
            StringBuilder csv = new();

            if (type == "borrowings")
            {
                var query = _context.BorrowedBooks
                                    .Include(b => b.Student)
                                    .Include(b => b.Book)
                                    .AsQueryable();

                // Apply search filter if provided (e.g., by student name or book title)
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(b =>
                        b.Student.Name.Contains(search) ||
                        b.Book.Title.Contains(search));
                }

                var borrowings = query.ToList();

                // CSV Header
                csv.AppendLine("StudentName,StudentEmail,BookTitle,BookId,BorrowDate,ReturnDate");

                foreach (var b in borrowings)
                {
                    string studentName = b.Student?.Name ?? "Unknown";
                    string studentEmail = b.Student?.Email ?? "Unknown";
                    string bookTitle = b.Book?.Title ?? "Unknown";
                    int bookId = b.Book?.Id ?? 0;

                    csv.AppendLine($"{studentName},{studentEmail},{bookTitle},{bookId},{b.BorrowDate},{b.ReturnDate}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "BorrowingsReport.csv");
            }
            else if (type == "reservations")
            {
                var query = _context.RoomReservations
                                    .Include(r => r.Student)
                                    .Include(r => r.Room)
                                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r =>
                        r.Student.Name.Contains(search) ||
                        r.Room.RoomName.Contains(search));
                }

                var reservations = query.ToList();

                // CSV Header
                csv.AppendLine("StudentName,StudentEmail,RoomName,Location,ReservationDate,EndDate,Confirmed");

                foreach (var r in reservations)
                {
                    string studentName = r.Student?.Name ?? "Unknown";
                    string studentEmail = r.Student?.Email ?? "Unknown";
                    string roomName = r.Room?.RoomName ?? "Unknown";
                    string location = r.Room?.Location ?? "Unknown";

                    csv.AppendLine($"{studentName},{studentEmail},{roomName},{location},{r.ReservationDateTime},{r.EndDateTime},{r.IsConfirmedByAdmin}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "RoomReservationsReport.csv");
            }

            else if (type == "feedback")
            {
                var feedbacks = _context.Feedbacks
                                        .Include(f => f.Student)
                                        .ToList();

                csv.AppendLine("StudentName,StudentEmail,Feedback,DateSubmitted");

                foreach (var f in feedbacks)
                {
                    csv.AppendLine($"{f.Student?.Name},{f.Student?.Email},{f.Message}");
                }

                return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "FeedbackReport.csv");
            }
            return BadRequest("Invalid report type.");
        }


    }
}
