using Microsoft.AspNetCore.Mvc;
using WebAuth.Services;

namespace WebAuth.Controllers
{
    public class AdminController(AuthenticationService authService) : Controller
    {
        private readonly AuthenticationService _authService = authService;

        public IActionResult Index()
        {
            var users = _authService.GetAllUsernames();
            return View(users);
        }

        [HttpPost]
        public IActionResult Delete(string username)
        {
            _authService.DeleteUser(username);
            return RedirectToAction("Index");
        }
    }
}