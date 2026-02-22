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

        private Student GetLoggedInStudent()
        {
            var studentId = HttpContext.Session.GetInt32("StudentId");
            if (studentId == null) return null;

            return _context.Students
                .Include(s => s.BorrowedBooks)
                    .ThenInclude(b => b.Book)
                .Include(s => s.RoomReservations)
                    .ThenInclude(r => r.Room)
                .FirstOrDefault(s => s.Id == studentId.Value);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult StudentLogin()
        {
            return View();
        }

        public IActionResult BookSearch(string search)
        {
            var booksQuery = _context.Books.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.ToLower().Contains(search.ToLower()) ||
                    b.Id.ToString() == search);
            }

            var books = booksQuery.ToList();
            return View(books);
        }

        public IActionResult BorrowBook(int id)
        {
            var student = GetLoggedInStudent();
            if (student == null)
                return RedirectToAction("Login");

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

            return RedirectToAction("BorrowedBooks");
        }

        public IActionResult BorrowedBooks()
        {
            var student = GetLoggedInStudent();
            if (student == null)
                return RedirectToAction("Login");

            var borrowedBooks = student.BorrowedBooks.ToList();
            return View(borrowedBooks);
        }

        public IActionResult RoomReservation()
        {
            var rooms = _context.MeetingRooms
                                .Include(r => r.RoomReservations)
                                    .ThenInclude(rr => rr.Student)
                                .ToList();

            return View(rooms);
        }

        public IActionResult ReserveRoom(int id)
        {
            var student = GetLoggedInStudent();
            if (student == null) return RedirectToAction("Login");

            var reservation = new RoomReservation
            {
                RoomId = id,
                StudentId = student.Id,
                ReservationDateTime = DateTime.Now,
                EndDateTime = DateTime.Now.AddHours(2),
                IsConfirmedByAdmin = false
            };

            _context.RoomReservations.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("RoomReservation");
        }

        public IActionResult Feedback()
        {
            var student = GetLoggedInStudent();
            if (student == null) return RedirectToAction("Login");

            var feedbacks = student.Feedbacks
                                   .OrderByDescending(f => f.SubmittedAt)
                                   .ToList();

            ViewBag.PastFeedbacks = feedbacks;
            return View();
        }

        [HttpPost]
        public IActionResult Feedback(string message)
        {
            var student = GetLoggedInStudent();
            if (student == null) return RedirectToAction("Login");
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

        public IActionResult Register()
{
    return View();
}

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

    student.PasswordHash = BCrypt.Net.BCrypt.HashPassword(student.Password);

    _context.Students.Add(student);
    _context.SaveChanges();

    HttpContext.Session.SetInt32("StudentId", student.Id);

    return RedirectToAction("Index");
}

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

    HttpContext.Session.SetInt32("StudentId", student.Id);
    return RedirectToAction("Profile");
}

public IActionResult Logout()
{
    HttpContext.Session.Clear();
    return RedirectToAction("Index", "Home");
}

public IActionResult Profile()
{
    var student = GetLoggedInStudent();
    if (student == null)
        return RedirectToAction("Login");

    return View(student);
}
    }
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
        //public IActionResult EditProfile(Student student)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Students.Update(student);
        //        _context.SaveChanges();
        //        return RedirectToAction("Profile");
        //    }
        //    return View(student);
        //}

