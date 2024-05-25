using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.UsersControllers
{
    [Authorize]
    public class MenuGroupController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public MenuGroupController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            _dbContext = new POSWebAppDbContext(optionsBuilder.Options);
        }

        #region // Main methods //

        public IActionResult Index()
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
                var userMenuGroup = _dbContext.ms_usermenugrp.ToList();
                return View(userMenuGroup);
            }
            catch (Exception ex)
            {
                TempData["alert message"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(UserMenuGroup userMenuGroup)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                await _dbContext.ms_usermenugrp.AddAsync(userMenuGroup);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Menu Group is successfully created!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(UserMenuGroup mnuGrp)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                _dbContext.ms_usermenugrp.Update(mnuGrp);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Menu Group is successfully updated!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(UserMenuGroup mnuGrp)
        {
            SetLayOutData();

            var dbMnuGrp = await _dbContext.ms_usermenugrp.FindAsync(mnuGrp.MnuGrpId);

            if (dbMnuGrp != null)
            {
                _dbContext.ms_usermenugrp.Remove(dbMnuGrp);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Menu Group is successfully deleted!";
            return RedirectToAction(nameof(Index));
        }

        #endregion


        #region // JS methods //

        public IActionResult AddMenuGroupPartial()
        {
            return PartialView("_AddMenuGroupPartial");
        }

        public async Task<IActionResult> EditMenuGroupPartial(short MnuGrpId)
        {
            var menuGroup = await _dbContext.ms_usermenugrp.FindAsync(MnuGrpId);

            return PartialView("_EditMenuGroupPartial", menuGroup);
        }


        public async Task<IActionResult> DeleteMenuGroupPartial(short MnuGrpId)
        {
            var menuGroup = await _dbContext.ms_usermenugrp.FindAsync(MnuGrpId);

            return PartialView("_DeleteMenuGroupPartial", menuGroup);
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

                var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

                var bizDte = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == POS.POSid)
                    .Select(auto => auto.BizDte)
                    .FirstOrDefault();

                ViewData["Business Date"] = bizDte.ToString("dd-MM-yyyy");
                ViewData["Database"] = _databaseSettings.DbName;
            }
        }

        #endregion


    }
}
