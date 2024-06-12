using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;
using Microsoft.IdentityModel.Tokens;

namespace POSWebApplication.Controllers.PublicControllers
{
    public class SystemSettingsController : Controller
    {
        private readonly IConfiguration _configuration;

        public SystemSettingsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            if (HttpContext.User.Claims.Count() <= 0 || HttpContext.Session.GetString("Connection").IsNullOrEmpty())
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["alert message"] = "Session Expired!";
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DatabaseSettings dbSettings)
        {
            var defaultServer = _configuration["Connections:Server"];
            var defaultUserID = _configuration["Connections:UserID"];
            var defaultPassword = _configuration["Connections:Password"];

            if (ModelState.IsValid)
            {
                string connectionString = $"Server={defaultServer};DataBase={dbSettings.DbName};" +
                $"User Id={defaultUserID};Password={defaultPassword};Encrypt=False;MultipleActiveResultSets=True";

                var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(connectionString);

                using var dbContext = new POSWebAppDbContext(optionsBuilder.Options);

                if (!await dbContext.Database.CanConnectAsync())
                {
                    ViewBag.AlertMessage = "Failed to establish a database connection: " + $"Server={defaultServer}";
                    return View(dbSettings);
                }

                HttpContext.Session.SetString("Connection", connectionString);
                HttpContext.Session.SetString("Database", dbSettings.DbName);
                TempData["InfoMessage"] = "Database Connection is successfully established.";
                return RedirectToAction("Index", "LogIn");
            }
            return View(dbSettings);
        }

        #endregion
    }
}
