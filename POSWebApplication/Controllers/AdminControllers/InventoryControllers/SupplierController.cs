using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IMemoryCache _cache;

        public SupplierController(DatabaseServices dbServices, IHttpContextAccessor accessor, IMemoryCache cache)
        {
            var connection = accessor.HttpContext?.Session.GetString("Connection") ?? "";
            if (connection.IsNullOrEmpty())
            {
                accessor.HttpContext?.Response.Redirect("../SystemSettings/Index");
            }
            else
            {
                _dbContext = new POSWebAppDbContext(dbServices.ConnectDatabase(connection));
                _cache = cache;
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


        #region // JS methods //

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
            var stocks = GetAllStocks();

            return stocks;
        }

        public Stock GetStockById(string itemId)
        {
            var stock = GetAllStocks().FirstOrDefault(stk => stk.ItemId == itemId);
            return stock ?? new Stock();
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

        #endregion
    }
}
