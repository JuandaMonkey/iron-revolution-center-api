using Microsoft.AspNetCore.Mvc;

namespace iron_revolution_center_api.Controllers
{
    public class MembershipController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
