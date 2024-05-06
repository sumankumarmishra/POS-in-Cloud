using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class ICReasonController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public ICReasonController(POSWebAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

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

        protected void SetLayOutData()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);

            if (user != null)
            {
                ViewData["Username"] = user.UserNme;

                var accLevel = _dbContext.ms_usermenuaccess.FirstOrDefault(u => u.MnuGrpId == user.MnuGrpId)?.AccLevel;
                ViewData["User Role"] = accLevel.HasValue ? $"accessLvl{accLevel}" : null;

                var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

                var bizDte = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == POS.POSid)
                    .Select(auto => auto.BizDte)
                    .FirstOrDefault();

                ViewData["Business Date"] = bizDte.ToString("dd MMM yyyy");
            }
        }

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
    }
}
