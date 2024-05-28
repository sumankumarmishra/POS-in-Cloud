using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.PublicControllers
{
    public class SystemSettingsController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        public SystemSettingsController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
        }


        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            if (HttpContext.User.Claims.Count() <= 0 || _databaseSettings == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["alert message"] = "Session Expired!";
                return View();
            }

            try
            {
                var userName = HttpContext.User.Claims.First().Value;

                if (_databaseSettings.ConnectionString != null)
                {
                    var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);

                    using var dbContext = new POSWebAppDbContext(optionsBuilder.Options);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                ViewBag.AlertMessage = ex.Message;
                return View();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DatabaseSettings dbSettings)
        {
            //var defaultServer = "WIN-A56617PG2SR";
            var defaultServer = "DESKTOP-PS2SRRC";

            if (ModelState.IsValid)
            {
                //string connectionString = $"Server={defaultServer};DataBase={dbSettings.DbName};" +
                // $"User Id=sa;Password=sa;Encrypt=False;MultipleActiveResultSets=True";
                string connectionString = $"Server={defaultServer};DataBase={dbSettings.DbName};" +
                $"User Id=n0isy;Password=luvie;Encrypt=False;MultipleActiveResultSets=True";


                var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(connectionString);

                using var dbContext = new POSWebAppDbContext(optionsBuilder.Options);

                if (!await dbContext.Database.CanConnectAsync())
                {
                    ViewBag.AlertMessage = "Failed to establish a database connection: " + $"Server={defaultServer}";
                    return View(dbSettings);
                }

                var databaseSettings = HttpContext.RequestServices.GetService<DatabaseSettings>();

                if (databaseSettings != null)
                {
                    databaseSettings.DbName = dbSettings.DbName;
                    databaseSettings.ConnectionString = connectionString; // Assign connection string
                }

                TempData["InfoMessage"] = "Database Connection is successfully established.";
                return RedirectToAction("Index", "LogIn");
            }
            return View(dbSettings);
        }

        #endregion
    }
}
