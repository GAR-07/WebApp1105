using Microsoft.AspNetCore.Mvc;

namespace WebApp1105.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}