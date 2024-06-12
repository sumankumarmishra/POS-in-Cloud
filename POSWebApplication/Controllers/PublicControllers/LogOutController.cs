using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using POSWebApplication.Models;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;

namespace POSWebApplication.Controllers.PublicControllers
{
    [Authorize]
    public class LogOutController : Controller
    {
        public async Task<IActionResult> Index()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "LogIn");
        }
    }
}
