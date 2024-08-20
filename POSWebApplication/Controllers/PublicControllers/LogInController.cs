using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;
using POSinCloud.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Memory;

namespace POSWebApplication.Controllers.PublicControllers
{
    public class LogInController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public LogInController(DatabaseServices dbServices, IHttpContextAccessor accessor, IMemoryCache cache)
        {
            var connection = accessor.HttpContext?.Session.GetString("Connection") ?? "";
            if (connection.IsNullOrEmpty())
            {
                accessor.HttpContext?.Response.Redirect("../SystemSettings/Index");
            }
            _dbContext = new POSWebAppDbContext(dbServices.ConnectDatabase(connection));
            _cache = cache;
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
                ViewBag.SuccessMessage = TempData["InfoMessage"];
            }

            ViewBag.DatabaseName = HttpContext.Session.GetString("Database");
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

                        // preload stock cache
                        GetAllStocks();

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
                ViewBag.AlertMessage = "The wrong database is connected.";
            }
            return View(user);
        }


        #endregion


        #region // Stock cache method //

        public List<Stock> GetAllStocks()
        {
            var dbName = HttpContext.Session.GetString("Database");
            if (_cache.TryGetValue(dbName + "_StockList", out List<Stock>? stockList))
            {
                return stockList ?? new List<Stock>();
            }
            else
            {
                var stocks = _dbContext.ms_stock.ToList();
                var pkgs = _dbContext.ms_stockpkgh.ToList();

                if (pkgs != null)
                {
                    foreach (var pkg in pkgs)
                    {
                        var stockPkg = new Stock()
                        {
                            ItemId = pkg.PkgNme,
                            ItemDesc = pkg.PkgNme,
                            SellingPrice = pkg.SellingPrice,
                            PkgHId = pkg.PkgHId,
                            Image = pkg.Image
                        };
                        stocks.Add(stockPkg);
                    }
                }

                foreach (var stock in stocks)
                {
                    stock.Base64Image = stock.Image != null ? Convert.ToBase64String(stock.Image) : "";
                }
                _cache.Set(dbName + "_StockList", stocks, TimeSpan.FromMinutes(30));

                return stocks;
            }
        }

        public List<Stock>? RestartStocks()
        {
            var dbName = HttpContext.Session.GetString("Database");
            if (_cache.TryGetValue(dbName + "StockList", out List<Stock>? stockList))
            {
                _cache.Remove(dbName + "_StockList");
            }

            var stocks = GetAllStocks();

            return stocks;
        }

        #endregion

    }
}
