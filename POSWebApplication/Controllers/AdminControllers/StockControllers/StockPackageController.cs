using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;
using System.Text;
using SixLabors.ImageSharp;
using Microsoft.Extensions.Caching.Memory;

namespace POSWebApplication.Controllers.AdminControllers.StockControllers
{
    [Authorize]
    public class StockPackageController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StockPackageController(DatabaseServices dbServices, IHttpContextAccessor accessor, IWebHostEnvironment hostingEnvironment, IMemoryCache cache)
        {
            var connection = accessor.HttpContext?.Session.GetString("Connection") ?? "";
            if (connection.IsNullOrEmpty())
            {
                accessor.HttpContext?.Response.Redirect("../SystemSettings/Index");
            }
            else
            {
                _dbContext = new POSWebAppDbContext(dbServices.ConnectDatabase(connection));
                _hostingEnvironment = hostingEnvironment;
                _cache = cache;
            }

        }


        #region // Stock Package H methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            var stockPackageList = await _dbContext.ms_stockpkgh.ToListAsync();
            return View(stockPackageList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockPkgH stockPkgH)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning message"] = "Required fields must be filled";
                return RedirectToAction(nameof(Index));
            }

            if (await _dbContext.ms_stockpkgh.AnyAsync(pkgh => pkgh.PkgNme == stockPkgH.PkgNme))
            {
                TempData["warning message"] = "Package Name already exists. Please enter a different one.";
                return RedirectToAction(nameof(Index));
            }

            try
            {

                if (stockPkgH.ImageFile != null)
                {
                    if (stockPkgH.ImageFile.Length > 0 && stockPkgH.ImageFile.Length <= Stock.MaxImageSize)
                    {
                        using var image = Image.Load(stockPkgH.ImageFile.OpenReadStream()); // to change image file to varBinary

                        using var memoryStream = new MemoryStream();
                        stockPkgH.ImageFile.CopyTo(memoryStream);
                        stockPkgH.Image = memoryStream.ToArray();
                    }
                    else
                    {
                        TempData["warning message"] = "Image size needs to be less than 500KB.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                else //adding default image
                {
                    string defaultImagePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "default.jpg");
                    var defaultImageBytes = System.IO.File.ReadAllBytes(defaultImagePath);
                    if (defaultImageBytes != null && defaultImageBytes.Length > 0)
                    {
                        stockPkgH.Image = defaultImageBytes;
                    }
                }
            }
            catch (Exception)
            {
                TempData["warning message"] = "Uploaded file is not an image.";
                return RedirectToAction(nameof(Index));
            }

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

            var userId = await _dbContext.ms_user
                .Where(u => u.UserCde == userCde)
                .Select(user => user.UserId)
                .FirstOrDefaultAsync();

            stockPkgH.RevDtetime = DateTime.Now;
            stockPkgH.UserId = userId;

            _dbContext.ms_stockpkgh.Add(stockPkgH);
            TempData["info message"] = "Stock Package is successfully created.";

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(StockPkgH stockPkgH)
        {
            if (ModelState.IsValid)
            {
                var UserCde = HttpContext.User.Claims.First()?.Value;

                var userId = await _dbContext.ms_user
                    .Where(u => u.UserCde == UserCde)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();

                stockPkgH.UserId = userId;

                try
                {

                    if (stockPkgH.ImageFile != null)
                    {
                        if (stockPkgH.ImageFile.Length > 0 && stockPkgH.ImageFile.Length <= Stock.MaxImageSize)
                        {
                            using var image = Image.Load(stockPkgH.ImageFile.OpenReadStream()); // to change image file to varBinary

                            using var memoryStream = new MemoryStream();
                            stockPkgH.ImageFile.CopyTo(memoryStream);
                            stockPkgH.Image = memoryStream.ToArray();
                        }
                        else
                        {
                            TempData["warning message"] = "Image size needs to be less than 500KB.";
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
                catch (Exception)
                {
                    TempData["warning message"] = "Uploaded file is not an image.";
                    return RedirectToAction(nameof(Index));
                }

                _dbContext.ms_stockpkgh.Update(stockPkgH);

                await _dbContext.SaveChangesAsync();
                TempData["info message"] = "Stock Package is successfully updated!";
            }
            else
            {
                TempData["warning message"] = "Required fields must be filled.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Stock stock)
        {
            var dbStock = await _dbContext.ms_stock.FindAsync(stock.ItemId);

            if (dbStock != null)
            {
                _dbContext.ms_stock.Remove(dbStock);
            }

            var thisStockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == stock.ItemId).ToListAsync();
            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == stock.ItemId).ToListAsync();

            if (thisStockUOMs != null)
            {
                foreach (var uom in thisStockUOMs)
                {
                    _dbContext.ms_stockuom.Remove(uom);
                }
            }

            if (thisStockBOMs != null)
            {
                foreach (var bom in thisStockBOMs)
                {
                    _dbContext.ms_stockbom.Remove(bom);
                }
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "Stock is successfully deleted.";
            return RedirectToAction(nameof(Index));
        }



        public IActionResult AddStockPackagePartial()
        {
            return PartialView("_AddStockPackagePartial");
        }

        public async Task<IActionResult> EditStockPackagePartial(int pkgHId)
        {
            var stockPkg = await _dbContext.ms_stockpkgh.FirstOrDefaultAsync(pkg => pkg.PkgHId == pkgHId);

            if (stockPkg != null)
            {
                var userCde = await _dbContext.ms_user
                    .Where(u => u.UserId == stockPkg.UserId)
                    .Select(u => u.UserCde)
                    .FirstOrDefaultAsync();

                stockPkg.UserCde = userCde;
                stockPkg.Base64Image = stockPkg.Image != null ? Convert.ToBase64String(stockPkg.Image) : "";
            }

            return PartialView("_EditStockPackagePartial", stockPkg);
        }

        public async Task<IActionResult> DeleteStockPackagePartial(int pkgHId)
        {
            var stockPkg = await _dbContext.ms_stockpkgh.FirstOrDefaultAsync(pkg => pkg.PkgHId == pkgHId);

            if (stockPkg != null)
            {
                var userCde = await _dbContext.ms_user
                    .Where(u => u.UserId == stockPkg.UserId)
                    .Select(u => u.UserCde)
                    .FirstOrDefaultAsync();

                stockPkg.UserCde = userCde;
            }

            return PartialView("_DeleteStockPackagePartial", stockPkg);
        }

        #endregion


        #region // Stock Package D methods //

        [HttpPost]
        public void SavePackageItems(int pkgHId, string[][] packageItems)
        {
            // Delete the previous data first

            _dbContext.ms_stockpkgd.Where(d => d.PkgHId == pkgHId).ExecuteDelete();

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var userId = _dbContext.ms_user
                .Where(user => user.UserCde == userCde)
                .Select(user => user.UserId)
                .FirstOrDefault();

            foreach (var item in packageItems)
            {
                var packageItem = new StockPkgD()
                {
                    PkgHId = pkgHId,
                    ItemId = item[0] ?? "",
                    ItemDesc = item[1] ?? "",
                    BaseUnit = item[2] ?? "",
                    Qty = ParseDecimal(item[3] ?? ""),
                    /*Price = ParseDecimal(item[4])*/
                    RevDtetime = DateTime.Now,
                    UserId = userId
                };

                _dbContext.ms_stockpkgd.Add(packageItem);
            }

            _dbContext.SaveChanges();
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


        #region // Package Items methods //

        public IEnumerable<StockPkgD> GetPackageItemsList(int pkgHId)
        {
            var packageItem = _dbContext.ms_stockpkgd.Where(d => d.PkgHId == pkgHId).ToList();

            return packageItem;
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

        public async Task<List<StockUOM>> GetStockUOMs(string itemId)
        {
            var stockUOMs = await _dbContext.ms_stockuom.Where(uom => uom.ItemId == itemId).ToListAsync();
            return stockUOMs;
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
