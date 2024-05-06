using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public SupplierController(POSWebAppDbContext dbContext)
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

            var suppliers = await _dbContext.ms_apvend.ToListAsync();

            return View(suppliers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier supplier)
        {
            supplier.Addr ??= "";
            supplier.Phone ??= "";
            supplier.Email ??= "";
            supplier.PTerm ??= "";
            supplier.PTermDay ??= 0;
            supplier.CreditLimit ??= 0;
            supplier.DiscPerc ??= 0;

            if (ModelState.IsValid)
            {
                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(u => u.UserCde == userCde)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();
                supplier.UserId = userId;
                supplier.RevDteTime = DateTime.Now;
                _dbContext.Add(supplier);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Supplier is successfully created.";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier supplier)
        {
            supplier.Addr ??= "";
            supplier.Phone ??= "";
            supplier.Email ??= "";
            supplier.PTerm ??= "";
            supplier.PTermDay ??= 0;
            supplier.CreditLimit ??= 0;
            supplier.DiscPerc ??= 0;

            if (ModelState.IsValid)
            {
                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(u => u.UserCde == userCde)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();
                supplier.UserId = userId;
                supplier.RevDteTime = DateTime.Now;
                _dbContext.ms_apvend.Update(supplier);
                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Supplier is successfully updated!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int apId)
        {
            var dbSupplier = await _dbContext.ms_apvend.FindAsync(apId);
            if (dbSupplier != null)
            {
                _dbContext.ms_apvend.Remove(dbSupplier);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Supplier is successfully deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public void SaveSupplierItems(int apId, string[][] supplierItems)
        {
            // Delete the previous data first

            _dbContext.ms_stockapvend.Where(d => d.ApId == apId).ExecuteDelete();

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var userId = _dbContext.ms_user
                .Where(user => user.UserCde == userCde)
                .Select(user => user.UserId)
                .FirstOrDefault();

            foreach (var item in supplierItems)
            {
                var supplierItem = new SupplierItem()
                {
                    ApId = apId,
                    ItemId = item[0] ?? "",
                    RevDteTime = DateTime.Now,
                    UserId = userId
                };

                _dbContext.ms_stockapvend.Add(supplierItem);
            }

            _dbContext.SaveChanges();
        }

        public IEnumerable<Stock> GetStocks()
        {
            var stocks = _dbContext.ms_stock.ToList();

            return stocks;
        }

        public Stock GetStockById(string itemId)
        {
            var stock = _dbContext.ms_stock.FirstOrDefault(stk => stk.ItemId == itemId);
            return stock;
        }

        public IEnumerable<Stock> GetSupplierItemsList(int apId)
        {
            var itemList = _dbContext.ms_stockapvend
                .Where(stk => stk.ApId == apId)
                .Join(_dbContext.ms_stock,
                apvend => apvend.ItemId,
                stk => stk.ItemId,
                (apvend, stk) => new Stock
                {
                    ItemId = apvend.ItemId,
                    ItemDesc = stk.ItemDesc
                })
                .ToList();

            return itemList;
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

        public IActionResult AddSupplierPartial()
        {
            return PartialView("_AddSupplierPartial");
        }

        public async Task<IActionResult> EditSupplierPartial(int apId)
        {
            var supplier = await _dbContext.ms_apvend.FindAsync(apId);

            return PartialView("_EditSupplierPartial", supplier);
        }

        public async Task<IActionResult> DeleteSupplierPartial(int apId)
        {
            var supplier = await _dbContext.ms_apvend.FindAsync(apId);

            return PartialView("_DeleteSupplierPartial", supplier);
        }
    }
}
