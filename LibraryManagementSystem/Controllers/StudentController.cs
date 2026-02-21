using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Http;        
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LibraryManagement.Controllers
{
    public class StudentController : Controller
    {
        private readonly MyDbContext _context;

        public StudentController(MyDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StudentLogin()
        {
            return View();
        }

        // 🔥 Show all books
        public IActionResult BookSearch(string search)
        {
            // TEMP: create a student if none exists
            if (!_context.Students.Any())
            {
                _context.Students.Add(new Student
                {
                    Name = "Test Student",
                    Email = "test@student.com",
                    PhoneNumber = "123456789"
                });

                _context.SaveChanges();
            }

            // Fetch books from DB
            var booksQuery = _context.Books.AsQueryable();

            // Apply search if provided
            if (!string.IsNullOrEmpty(search))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Id.ToString() == search ||
                    b.Title.ToLower().Contains(search.ToLower()));
            }

            var books = booksQuery.ToList();
            return View(books);
        }


        // 🔥 Borrow action
        public IActionResult BorrowBook(int id)
        {
            var student = _context.Students.FirstOrDefault();

            if (student == null)
                return RedirectToAction("BookSearch");

            var borrowRequest = new BorrowedBook
            {
                BookId = id,
                StudentId = student.Id,
                BorrowDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                IsConfirmedByAdmin = false
            };

            _context.BorrowedBooks.Add(borrowRequest);
            _context.SaveChanges();

            return RedirectToAction("BookSearch");
        }

        public IActionResult BorrowedBooks()
        {
            var student = _context.Students.FirstOrDefault(); // for now, just our test student
            var borrowedBooks = _context.BorrowedBooks
                .Include(b => b.Book)
                .Where(b => b.StudentId == student.Id)
                .ToList();

            return View(borrowedBooks);
        }


        public IActionResult RoomReservation()
        {
            var rooms = _context.MeetingRooms
                                .Include(r => r.RoomReservations)
                                .ThenInclude(rr => rr.Student)
                                .ToList();

            return View(rooms); // Pass the list of rooms directly
        }
        public IActionResult ReserveRoom(int id)
        {
            var student = _context.Students.FirstOrDefault();
            if (student == null) return RedirectToAction("RoomReservation");

            var reservation = new RoomReservation
            {
                RoomId = id,
                StudentId = student.Id,
                ReservationDateTime = DateTime.Now,
                IsConfirmedByAdmin = false,
                EndDateTime = DateTime.Now.AddHours(2) // fixed 2 hours
            };

            _context.RoomReservations.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("RoomReservation");
        }
        // GET: Show feedback form and past feedbacks
        public IActionResult Feedback()
        {
            var student = _context.Students.FirstOrDefault(); // TEMP: just the test student
            if (student == null) return RedirectToAction("Index");

            // Get past feedbacks
            var feedbacks = _context.Feedbacks
                .Where(f => f.StudentId == student.Id)
                .OrderByDescending(f => f.SubmittedAt)
                .ToList();

            ViewBag.PastFeedbacks = feedbacks;
            return View();
        }

        // POST: Submit feedback
        [HttpPost]
        public IActionResult Feedback(string message)
        {
            var student = _context.Students.FirstOrDefault();
            if (student == null) return RedirectToAction("Index");

            if (!string.IsNullOrWhiteSpace(message))
            {
                var fb = new Feedback
                {
                    StudentId = student.Id,
                    Message = message,
                    SubmittedAt = DateTime.Now,
                    IsReviewed = false
                };

                _context.Feedbacks.Add(fb);
                _context.SaveChanges();
            }

            return RedirectToAction("Feedback");
        }
       

        // GET: Students/Register
        public IActionResult Register()
        {
            return View();
        }
        // POST: Students/Register
        [HttpPost]
        public IActionResult Register(Student student)
        {
            if (_context.Students.Any(s => s.Email == student.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
                return View(student);
            }

            if (string.IsNullOrWhiteSpace(student.Password))
            {
                ModelState.AddModelError("Password", "Password is required");
                return View(student);
            }

            // Hash the password
            student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(student.Password);

            _context.Students.Add(student);
            _context.SaveChanges();

            // Optional: set session if you want to track logged-in user
            HttpContext.Session.SetInt32("StudentId", student.Id);

            // Redirect to home page
            return RedirectToAction("Index");
        }
        // GET: Students/Login
        public IActionResult Login()
        {
           return View();
        }

[HttpPost]
public IActionResult Login(string email, string password)
{
    var student = _context.Students.FirstOrDefault(s => s.Email == email);

    if (student == null || !BCrypt.Net.BCrypt.Verify(password, student.PasswordHash))
    {
        ModelState.AddModelError("", "Invalid email or password");
        return View();
    }

    // Set session for logged-in student
    HttpContext.Session.SetInt32("StudentId", student.Id);

    return RedirectToAction("Profile");
}

        // GET: Logout
        // StudentController
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // clear all session data
            return RedirectToAction("Index", "Home"); // make sure you redirect to an existing page
        }

        public IActionResult Profile()
        {
            // TEMP: just pick the first student
            var student = _context.Students
                .Include(s => s.BorrowedBooks)
                    .ThenInclude(b => b.Book)
                .Include(s => s.RoomReservations)
                    .ThenInclude(r => r.Room)
                .FirstOrDefault();

            if (student == null)
                return RedirectToAction("Register"); // if no students exist

            return View(student);
        }

        //// GET
        //public IActionResult Register()
        //    {
        //        return View();
        //    }

        //    // POST
        //    [HttpPost]
        //    public IActionResult Register(Student student)
        //    {
        //        if (_context.Students.Any(s => s.Email == student.Email))
        //        {
        //            ModelState.AddModelError("Email", "Email already exists");
        //            return View(student);
        //        }

        //        _context.Students.Add(student);
        //        _context.SaveChanges();

        //        // set session
        //        HttpContext.Session.SetInt32("StudentId", student.Id);

        //        return RedirectToAction("Profile");
        //    }

        //    // GET
        //    public IActionResult Login()
        //    {
        //        return View();
        //    }

        //    // POST
        //    [HttpPost]
        //    public IActionResult Login(string email, string password)
        //    {
        //        var student = _context.Students.FirstOrDefault(s => s.Email == email && s.Password == password);

        //        if (student == null)
        //        {
        //            ModelState.AddModelError("", "Invalid email or password");
        //            return View();
        //        }

        //        HttpContext.Session.SetInt32("StudentId", student.Id);
        //        return RedirectToAction("Profile");
        //    }

        // Update Profile to get the logged-in student
        //public IActionResult Profile()
        //{
        //    var studentId = HttpContext.Session.GetInt32("StudentId");
        //    if (studentId == null)
        //        return RedirectToAction("Login");

        //    var student = _context.Students
        //        .Include(s => s.BorrowedBooks)
        //            .ThenInclude(b => b.Book)
        //        .Include(s => s.RoomReservations)
        //            .ThenInclude(r => r.Room)
        //        .FirstOrDefault(s => s.Id == studentId.Value);

        //    return View(student);
        //}


        //[HttpPost]
        //    public IActionResult EditProfile(Student student)
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            _context.Students.Update(student);
        //            _context.SaveChanges();
        //            return RedirectToAction("Profile");
        //        }
        //        return View(student);
        //    }


    }
}
