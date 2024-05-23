using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.PublicControllers
{
    public class LogInController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public LogInController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            _dbContext = new POSWebAppDbContext(optionsBuilder.Options);
        }

        #region // Main methods //

        public IActionResult Index()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity != null && claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            if (TempData["InfoMessage"] != null)
            {
                ViewBag.AlertMessage = TempData["InfoMessage"];
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(User user)
        {
            try
            {

                var userList = await _dbContext.ms_user.ToListAsync();

                if (ModelState.IsValid)
                {
                    var dbUser = userList.FirstOrDefault(u => u.UserCde.ToLower() == user.UserCde.ToLower() && u.Pwd == user.Pwd);

                    if (dbUser != null)
                    {
                        var claims = new List<Claim>() {
                            new Claim(ClaimTypes.NameIdentifier, user.UserCde)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var properties = new AuthenticationProperties()
                        {
                            AllowRefresh = true
                        };

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.AlertMessage = "Authentication failed. Please check your credentials.";
                    }
                }
                else
                {
                    ViewBag.AlertMessage = "All fields must be filled!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.AlertMessage = ex.Message;
            }
            return View(user);
        }


        #endregion

    }
}
