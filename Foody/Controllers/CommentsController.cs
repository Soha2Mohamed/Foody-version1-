using Microsoft.AspNetCore.Mvc;

namespace Foody.Controllers
{
    public class CommentsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
