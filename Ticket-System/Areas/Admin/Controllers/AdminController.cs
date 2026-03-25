using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ticket_System.Areas.Admin.Controllers
{
    [Area("Admin")]                    // gehört zur Area "Admin"
    [Authorize(Roles = "Admin")]       // nur Admins dürfen rein
    public class AdminController : Controller
    {
        // GET: /Admin/Admin/Index → Admin-Startseite
        public IActionResult Index()
        {
            return View();
        }
    }
}
