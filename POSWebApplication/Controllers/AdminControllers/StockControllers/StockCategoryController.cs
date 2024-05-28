using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.StockControllers
{
    [Authorize]
    public class StockCategoryController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public StockCategoryController(DatabaseSettings databaseSettings)
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

            var categories = await _dbContext.ms_stockcategory.ToListAsync();

            var categoryModelList = new StockCategoryModelList()
            {
                StockCategories = categories
            };

            return View(categoryModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockCategory stockCategory)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Add(stockCategory);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Stock Category is successfully created.";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StockCategory stockCategory)
        {
            if (ModelState.IsValid)
            {
                _dbContext.ms_stockcategory.Update(stockCategory);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Stock Category is successfully updated!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(StockCategory stockCategory)
        {
            var dbStockCategory = await _dbContext.ms_stockcategory.FindAsync(stockCategory.CatgId);
            if (dbStockCategory != null)
            {
                _dbContext.ms_stockcategory.Remove(dbStockCategory);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Stock Category is successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        #endregion


        #region // JS methods //

        public async Task<IActionResult> AddStockCategoryPartial()
        {
            var categories = await _dbContext.ms_stockcategory.ToListAsync();

            var categoryModelList = new StockCategoryModelList()
            {
                StockCategories = categories
            };

            return PartialView("_AddStockCategoryPartial", categoryModelList);
        }

        public async Task<IActionResult> EditStockCategoryPartial(int catgId)
        {
            var category = await _dbContext.ms_stockcategory.FindAsync(catgId);
            var categories = await _dbContext.ms_stockcategory.ToListAsync();

            var categoryModelList = new StockCategoryModelList
            {
                StockCategories = categories,
                StockCategory = category
            };

            return PartialView("_EditStockCategoryPartial", categoryModelList);
        }

        public async Task<IActionResult> DeleteStockCategoryPartial(int catgId)
        {
            var category = await _dbContext.ms_stockcategory.FindAsync(catgId);
            var categories = await _dbContext.ms_stockcategory.ToListAsync();

            var categoryModelList = new StockCategoryModelList
            {
                StockCategories = categories,
                StockCategory = category
            };

            return PartialView("_DeleteStockCategoryPartial", categoryModelList);
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
