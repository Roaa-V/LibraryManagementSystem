using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult LoginChoice()
        {
            return View();
        }
    }
}
