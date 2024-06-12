using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class ICReasonController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public ICReasonController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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

            var groups = await _dbContext.icreason.ToListAsync();

            return View(groups);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ICReason icReason)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(icReason);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Inventory Issue Reason is successfully created.";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ICReason icReason)
        {
            if (ModelState.IsValid)
            {
                _dbContext.icreason.Update(icReason);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Inventory Issue Reason is successfully updated!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ICReason icReason)
        {
            var dbICReason = await _dbContext.icreason.FindAsync(icReason.ICReasonId);
            if (dbICReason != null)
            {
                _dbContext.icreason.Remove(dbICReason);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Inventory Issue Reason is successfully deleted.";
            return RedirectToAction(nameof(Index));
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


        #region // JS methods //

        public IActionResult AddICReasonPartial()
        {
            return PartialView("_AddICReasonPartial");
        }

        public async Task<IActionResult> EditICReasonPartial(short icReasonId)
        {
            var reason = await _dbContext.icreason.FindAsync(icReasonId);

            return PartialView("_EditICReasonPartial", reason);
        }

        public async Task<IActionResult> DeleteICReasonPartial(short icReasonId)
        {
            var reason = await _dbContext.icreason.FindAsync(icReasonId);

            return PartialView("_DeleteICReasonPartial", reason);
        }

        #endregion
    }
}
