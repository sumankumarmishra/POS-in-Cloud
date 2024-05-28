using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.StockControllers
{
    [Authorize]
    public class StockGroupController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public StockGroupController(DatabaseSettings databaseSettings)
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

            var groups = await _dbContext.ms_stockgroup.ToListAsync();

            var groupModelList = new StockGroupModelList()
            {
                StockGroups = groups
            };

            return View(groupModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockGroup stockGroup)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(stockGroup);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Stock Group is successfully created.";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StockGroup stockGroup)
        {
            if (ModelState.IsValid)
            {
                _dbContext.ms_stockgroup.Update(stockGroup);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Stock Group is successfully updated!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(StockGroup stockGroup)
        {
            var dbStockGroup = await _dbContext.ms_stockgroup.FindAsync(stockGroup.StkGrpId);
            if (dbStockGroup != null)
            {
                _dbContext.ms_stockgroup.Remove(dbStockGroup);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Stock Group is successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        #endregion


        #region // JS methods //

        public async Task<IActionResult> AddStockGroupPartial()
        {
            var groups = await _dbContext.ms_stockgroup.ToListAsync();

            var groupModelList = new StockGroupModelList()
            {
                StockGroups = groups
            };

            return PartialView("_AddStockGroupPartial", groupModelList);
        }

        public async Task<IActionResult> EditStockGroupPartial(short stkGrpId)
        {
            var group = await _dbContext.ms_stockgroup.FindAsync(stkGrpId);

            var groups = await _dbContext.ms_stockgroup.ToListAsync();

            var groupModelList = new StockGroupModelList()
            {
                StockGroups = groups,
                StockGroup = group
            };

            return PartialView("_EditStockGroupPartial", groupModelList);
        }


        public async Task<IActionResult> DeleteStockGroupPartial(short stkGrpId)
        {
            var group = await _dbContext.ms_stockgroup.FindAsync(stkGrpId);

            var groups = await _dbContext.ms_stockgroup.ToListAsync();

            var groupModelList = new StockGroupModelList()
            {
                StockGroups = groups,
                StockGroup = group
            };

            return PartialView("_DeleteStockGroupPartial", groupModelList);
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
                    ViewData["Database"] = $"{_databaseSettings.DbName}({company.POSPkgNme})";
                }

            }
        }

        #endregion
    }
}
