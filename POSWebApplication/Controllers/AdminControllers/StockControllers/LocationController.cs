using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.StockControllers
{
    [Authorize]
    public class LocationController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public LocationController(DatabaseServices dbServices, IHttpContextAccessor accessor)
        {
            var connection = accessor.HttpContext?.Session.GetString("Connection") ?? "";
            if (connection.IsNullOrEmpty())
            {
                accessor.HttpContext?.Response.Redirect("../SystemSettings/Index");
            }
            else
            {
                _dbContext = new POSWebAppDbContext(dbServices.ConnectDatabase(connection));
            }
        }

        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            if (TempData["info message"] != null)
            {
                ViewBag.InfoMessage = TempData["info message"];
            }
            if (TempData["warning message"] != null)
            {
                ViewBag.WarningMessage = TempData["warning message"];
            }
            try
            {
                var locationList = await _dbContext.ms_location.ToListAsync();
                return View(locationList);
            }
            catch (Exception ex)
            {
                TempData["alert message"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LocCde,LocDesc,IsOutlet")] Location location)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                if (await _dbContext.ms_location.AnyAsync(u => u.LocCde == location.LocCde))
                {
                    TempData["warning message"] = "Location Code already exists.";
                }
                else
                {
                    _dbContext.Add(location);
                    await _dbContext.SaveChangesAsync();
                    TempData["info message"] = "Location is successfully created!";

                }
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Location location)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                _dbContext.Update(location);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Location is successfully updated!";

            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Location location)
        {
            SetLayOutData();

            var dbLocation = await _dbContext.ms_location.FindAsync(location.LocCde);

            if (dbLocation != null)
            {
                _dbContext.ms_location.Remove(dbLocation);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Location is successfully deleted!";
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(string id)
        {
            return (_dbContext.ms_location?.Any(e => e.LocCde == id)).GetValueOrDefault();
        }

        #endregion


        #region // JS methods //
        public async Task<IActionResult> AddLocationPartial()
        {
            var locations = await _dbContext.ms_location.ToListAsync();

            var locationModelList = new LocationModelList()
            {
                Locations = locations
            };

            return PartialView("_AddLocationPartial", locationModelList);
        }

        public async Task<IActionResult> EditLocationPartial(string LocCde)
        {
            var locations = await _dbContext.ms_location.ToListAsync();
            var location = await _dbContext.ms_location.FirstOrDefaultAsync(u => u.LocCde == LocCde);

            var locationModelList = new LocationModelList()
            {
                Locations = locations,
                Location = location
            };

            return PartialView("_EditLocationPartial", locationModelList);
        }

        public async Task<IActionResult> DeleteLocationPartial(string LocCde)
        {
            var locations = await _dbContext.ms_location.ToListAsync();
            var location = await _dbContext.ms_location.FirstOrDefaultAsync(u => u.LocCde == LocCde);

            var locationModelList = new LocationModelList()
            {
                Locations = locations,
                Location = location
            };

            return PartialView("_DeleteLocationPartial", locationModelList);
        }

        #endregion


        #region // Common methods //
        protected void SetLayOutData()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);

            if (user != null)
            {
                ViewData["Username"] = user.UserNme;

                var accLevel = _dbContext.ms_usermenuaccess.FirstOrDefault(u => u.MnuGrpId == user.MnuGrpId)?.AccLevel;
                ViewData["User Role"] = accLevel.HasValue ? $"accessLvl{accLevel}" : null;

                var posId = _dbContext.ms_userpos
                    .Where(pos => pos.UserId == user.UserId)
                    .Select(pos => pos.POSid)
                    .FirstOrDefault();

                var company = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == posId)
                    .FirstOrDefault();

                if (company != null)
                {
                    ViewData["Business Date"] = company.BizDte.ToString("dd-MM-yyyy");

                    var dbName = HttpContext.Session.GetString("Database");
                    ViewData["Database"] = $"{dbName}({company.POSPkgNme})";
                }

            }
        }

        #endregion

    }
}
