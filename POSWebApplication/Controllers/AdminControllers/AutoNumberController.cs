﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers
{
    [Authorize]
    public class AutoNumberController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public AutoNumberController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            _dbContext = new POSWebAppDbContext(optionsBuilder.Options);
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
                var autoNumberList = await _dbContext.ms_autonumber.ToListAsync();
                return View(autoNumberList);
            }
            catch (Exception ex)
            {
                TempData["alert message"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AutoNumber autoNumber)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                var dbAutoNumber = await _dbContext.ms_autonumber.FindAsync(autoNumber.AutoNoId);

                if (dbAutoNumber != null)
                {
                    dbAutoNumber.PosId = autoNumber.PosId;
                    dbAutoNumber.BizDte = autoNumber.BizDte;
                    dbAutoNumber.NoOfShift = autoNumber.NoOfShift;
                    dbAutoNumber.CurShift = autoNumber.CurShift;
                    dbAutoNumber.PosDefLoc = autoNumber.PosDefLoc;
                    dbAutoNumber.PosDefSaleTyp = autoNumber.PosDefSaleTyp;
                    dbAutoNumber.BillPrefix = autoNumber.BillPrefix;
                    dbAutoNumber.RunningNo = autoNumber.RunningNo;
                    dbAutoNumber.LastUsedNo = autoNumber.LastUsedNo;
                    dbAutoNumber.ZeroLeading = autoNumber.ZeroLeading;
                    dbAutoNumber.AllowAccessRoom = autoNumber.AllowAccessRoom;
                    _dbContext.ms_autonumber.Update(dbAutoNumber);
                }
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Auto number is successfully updated.";


            }
            return RedirectToAction(nameof(Index));
        }

        private bool AutoNumberExists(short id)
        {
            return (_dbContext.ms_autonumber?.Any(e => e.AutoNoId == id)).GetValueOrDefault();
        }

        #endregion


        #region // JS methods //

        public async Task<IActionResult> EditAutoNumberPartial(short autoNoId)
        {
            var autoNumberList = await _dbContext.ms_autonumber.ToListAsync();
            var autoNumber = await _dbContext.ms_autonumber.FindAsync(autoNoId);
            var locations = await _dbContext.ms_location.ToListAsync();

            var autoNumberModelList = new AutoNumberModelList
            {
                AutoNumbers = autoNumberList,
                AutoNumber = autoNumber,
                Locations = locations
            };

            return PartialView("_EditAutoNumberPartial", autoNumberModelList);
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

                ViewData["Business Date"] = bizDte.ToString("dd MMM yyyy");
            }
        }

        #endregion

    }
}
