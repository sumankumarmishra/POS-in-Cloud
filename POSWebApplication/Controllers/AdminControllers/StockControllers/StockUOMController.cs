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
    public class StockUOMController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public StockUOMController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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
                var stockList = await _dbContext.ms_stockuom.ToListAsync();
                return View(stockList);
            }
            catch (Exception ex)
            {
                TempData["alert message"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockUOM stockUOM)
        {

            if (ModelState.IsValid)
            {
                await _dbContext.AddAsync(stockUOM);
                await _dbContext.SaveChangesAsync();
            }

            var stockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == stockUOM.ItemId).ToListAsync();
            var stockList = await _dbContext.ms_stock.ToListAsync();
            var firstStockUOM = stockUOMs.FirstOrDefault();

            var stockModelList = new StockModelList
            {
                StockUOM = firstStockUOM,
                StockList = stockList,
                StockUOMs = stockUOMs
            };

            return PartialView("_UOMPartial", stockModelList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StockUOM stockUOM)
        {
            if (ModelState.IsValid)
            {
                var dbStockUOM = await _dbContext.ms_stockuom.FirstOrDefaultAsync(u => u.UOMCde == stockUOM.UOMCde && u.ItemId == stockUOM.ItemId);

                if (dbStockUOM != null)
                {
                    dbStockUOM.UOMRate = stockUOM.UOMRate;
                    dbStockUOM.UnitCost = stockUOM.UnitCost;
                    dbStockUOM.SellingPrice = stockUOM.SellingPrice;
                    _dbContext.ms_stockuom.Update(dbStockUOM);
                }

                var dbStock = await _dbContext.ms_stock.FirstOrDefaultAsync(stk => stk.ItemId == stockUOM.ItemId);

                if (dbStock != null && dbStock.BaseUnit == stockUOM.UOMCde)
                {
                    dbStock.UnitCost = stockUOM.UnitCost;
                    dbStock.SellingPrice = stockUOM.SellingPrice;
                    _dbContext.ms_stock.Update(dbStock);
                }

                await _dbContext.SaveChangesAsync();
            }

            var stockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == stockUOM.ItemId).ToListAsync();
            var stockList = await _dbContext.ms_stock.ToListAsync();
            var firstStockUOM = stockUOMs.FirstOrDefault();

            var stockModelList = new StockModelList
            {
                StockUOM = firstStockUOM,
                StockList = stockList,
                StockUOMs = stockUOMs
            };

            return PartialView("_UOMPartial", stockModelList);
        }


        #endregion


        #region // StockUOM items methods //

        public async Task<IEnumerable<StockUOM>> GetStockUOMsList(string itemId)
        {
            var stockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == itemId).ToListAsync();
            return stockUOMs;
        }

        [HttpPost]
        public void SaveStockUOMItems(string itemId, string[][] stockUOMItems)
        {
            // Delete the previous data first
            try
            {
                _dbContext.ms_stockuom.Where(uom => uom.ItemId == itemId).ExecuteDelete();

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefault();

                foreach (var uom in stockUOMItems)
                {
                    var uomItem = new StockUOM()
                    {
                        ItemId = itemId,
                        OrdNo = 0,
                        UOMCde = uom[0] ?? "",
                        UOMRate = ParseDecimal(uom[1] ?? ""),
                        UnitCost = ParseDecimal(uom[2] ?? ""),
                        SellingPrice = ParseDecimal(uom[3] ?? "")
                    };

                    _dbContext.ms_stockuom.Add(uomItem);

                    var dbStock = _dbContext.ms_stock.FirstOrDefault(stk => stk.ItemId == uomItem.ItemId);

                    if (dbStock != null && dbStock.BaseUnit == uomItem.UOMCde)
                    {
                        dbStock.UnitCost = uomItem.UnitCost;
                        dbStock.SellingPrice = uomItem.SellingPrice;
                        _dbContext.ms_stock.Update(dbStock);
                    }
                }

                _dbContext.SaveChanges();
            }
            catch
            {
            }
        }

        #endregion


        #region // Utility Parsing methods //

        static decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return default;
        }

        static int ParseInt(string value)
        {
            if (Int32.TryParse(value, out int result))
            {
                return result;
            }
            return default;
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
