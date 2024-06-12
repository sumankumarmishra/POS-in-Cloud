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
    public class StockBOMController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public StockBOMController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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

            var stocks = await _dbContext.ms_stock.Where(u => u.FinishGoodFlg == true).ToListAsync();

            var bOMModelList = new StockBOMModelList()
            {
                Stocks = stocks
            };

            return View(bOMModelList);
        }

        public async Task<IActionResult> Create(StockBOM stockBOM)
        {
            if (ModelState.IsValid)
            {
                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

                var userId = _dbContext.ms_user
                    .Where(u => u.UserCde == userCde)
                    .Select(u => u.UserId)
                    .FirstOrDefault();

                stockBOM.RevDteTime = DateTime.Now;
                stockBOM.UserId = userId;

                await _dbContext.AddAsync(stockBOM);
                await _dbContext.SaveChangesAsync();
            }

            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == stockBOM.BOMId).OrderBy(u => u.OrdId).ToListAsync();

            foreach (var bom in thisStockBOMs)
            {
                bom.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList()
            {
                ThisStockBOMs = thisStockBOMs,
                StockBOM = new StockBOM()
                {
                    BOMId = stockBOM.BOMId
                }
            };

            return PartialView("_StockBOMPartial", bOMModelList);
        }

        public async Task<IActionResult> Edit(StockBOM stockBOM)
        {
            if (ModelState.IsValid)
            {
                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

                var userId = _dbContext.ms_user
                    .Where(u => u.UserCde == userCde)
                    .Select(u => u.UserId)
                    .FirstOrDefault();

                stockBOM.RevDteTime = DateTime.Now;
                stockBOM.UserId = userId;

                _dbContext.ms_stockbom.Update(stockBOM);
                await _dbContext.SaveChangesAsync();
            }

            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == stockBOM.BOMId).OrderBy(u => u.OrdId).ToListAsync();

            foreach (var bom in thisStockBOMs)
            {
                bom.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList()
            {
                ThisStockBOMs = thisStockBOMs,
                StockBOM = new StockBOM()
                {
                    BOMId = stockBOM.BOMId
                }
            };

            return PartialView("_StockBOMPartial", bOMModelList);
        }

        #endregion


        #region // Stock BOM Items methods //

        public async Task<IEnumerable<StockBOM>> GetStockBOMList(string bomId)
        {
            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).OrderBy(u => u.OrdId).ToListAsync();

            return thisStockBOMs;
        }

        public IEnumerable<StockBOM> GetStocks()
        {
            var stocks = _dbContext.ms_stock
                .Select(stock => new StockBOM
                {
                    ItemId = stock.ItemId,
                    BaseUnit = stock.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'I'
                })
                .ToList();

            var serviceItems = _dbContext.ms_serviceitem
                .Select(serviceItem => new StockBOM
                {
                    ItemId = serviceItem.SrvcItemId,
                    BaseUnit = serviceItem.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'S'
                })
                .ToList();

            var unionStocks = stocks.Union(serviceItems).Select(sItem => new StockBOM
            {
                ItemId = sItem.ItemId,
                BaseUnit = sItem.BaseUnit,
                UOMRate = 1,
                StockFlg = sItem.StockFlg
            });


            return unionStocks;
        }

        #endregion


        #region // JS methods //

        public async Task<IActionResult> StockBOMPartial(string bomId)
        {
            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).OrderBy(u => u.OrdId).ToListAsync();

            foreach (var stockBOM in thisStockBOMs)
            {
                stockBOM.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList()
            {
                ThisStockBOMs = thisStockBOMs,
                StockBOM = new StockBOM()
                {
                    BOMId = bomId
                }
            };

            return PartialView("_StockBOMPartial", bOMModelList);
        }

        public async Task<IActionResult> AddStockBOMPartial(string bomId)
        {
            var stocks = await _dbContext.ms_stock
                .Select(stock => new StockBOM
                {
                    ItemId = stock.ItemId,
                    BaseUnit = stock.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'I'
                })
                .ToListAsync();

            var serviceItems = await _dbContext.ms_serviceitem
                .Select(serviceItem => new StockBOM
                {
                    ItemId = serviceItem.SrvcItemId,
                    BaseUnit = serviceItem.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'S'
                })
                .ToListAsync();

            var ordId = await _dbContext.ms_stockbom
                .Where(u => u.BOMId == bomId)
                .Select(u => u.OrdId)
                .OrderBy(u => u)
                .LastOrDefaultAsync();

            var unionStockBOMs = stocks.Union(serviceItems);
            var firstItemId = unionStockBOMs.FirstOrDefault()?.ItemId;
            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).OrderBy(u => u.OrdId).ToListAsync();
            var thisStockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == firstItemId).ToListAsync();

            foreach (var stockBOM in thisStockBOMs)
            {
                stockBOM.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList()
            {
                UnionStockBOMs = unionStockBOMs,
                ThisStockBOMs = thisStockBOMs,
                ThisStockUOMs = thisStockUOMs,
                StockBOM = new StockBOM()
                {
                    OrdId = (short)(ordId + 1),
                    BOMId = bomId,
                    UOMRate = 1
                }
            };

            return PartialView("_AddStockBOMPartial", bOMModelList);
        }

        public async Task<IActionResult> EditStockBOMPartial(string bomId, int stkBOMId)
        {
            var thisStockUOMs = new List<StockUOM>();
            Boolean flag = false;

            var stocks = _dbContext.ms_stock
                .Select(stock => new StockBOM
                {
                    ItemId = stock.ItemId,
                    BaseUnit = stock.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'I'
                });

            var serviceItems = _dbContext.ms_serviceitem
                .Select(serviceItem => new StockBOM
                {
                    ItemId = serviceItem.SrvcItemId,
                    BaseUnit = serviceItem.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'S'
                });

            var ordId = _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).Select(u => u.OrdId).OrderBy(u => u).LastOrDefault();

            var unionStockBOMs = stocks.Union(serviceItems);
            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).OrderBy(u => u.OrdId).ToListAsync();
            var thisStockBOM = await _dbContext.ms_stockbom.FindAsync(stkBOMId);
            var itemId = thisStockBOM?.ItemId;

            if (itemId != null)
            {
                thisStockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == itemId).ToListAsync();
                if (thisStockUOMs.Count <= 0)
                {
                    flag = true;
                }
            }

            foreach (var stockBOM in thisStockBOMs)
            {
                stockBOM.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList
            {
                UnionStockBOMs = unionStockBOMs,
                ThisStockBOMs = thisStockBOMs,
                ThisStockUOMs = thisStockUOMs,
                StockBOM = thisStockBOM,
                BaseUnitFlg = flag
            };

            return PartialView("_EditStockBOMPartial", bOMModelList);
        }

        public async Task<IActionResult> DeleteStockBOMPartial(string bomId, int stkbomId)
        {
            var dbStkBOM = await _dbContext.ms_stockbom.FindAsync(stkbomId);

            if (dbStkBOM != null)
            {
                _dbContext.ms_stockbom.Remove(dbStkBOM);
                await _dbContext.SaveChangesAsync();
            }

            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).OrderBy(u => u.OrdId).ToListAsync();

            foreach (var stockBOM in thisStockBOMs)
            {
                stockBOM.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList()
            {
                ThisStockBOMs = thisStockBOMs,
                StockBOM = new StockBOM()
                {
                    BOMId = bomId
                }
            };

            return PartialView("_StockBOMPartial", bOMModelList);
        }

        public async Task<IActionResult> SelectItemPartial(string bomId, string itemId)
        {
            Boolean flag = false;

            var stocks = await _dbContext.ms_stock
                .Select(stock => new StockBOM
                {
                    ItemId = stock.ItemId,
                    BaseUnit = stock.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'I'
                })
                .ToListAsync();

            var serviceItems = await _dbContext.ms_serviceitem
                .Select(serviceItem => new StockBOM
                {
                    ItemId = serviceItem.SrvcItemId,
                    BaseUnit = serviceItem.BaseUnit,
                    UOMRate = 1,
                    StockFlg = 'S'
                })
                .ToListAsync();

            var ordId = await _dbContext.ms_stockbom
                .Where(u => u.BOMId == bomId)
                .Select(u => u.OrdId)
                .OrderBy(u => u)
                .LastOrDefaultAsync();

            var unionStockBOMs = stocks.Union(serviceItems);
            var thisStockBOMs = await _dbContext.ms_stockbom.Where(u => u.BOMId == bomId).OrderBy(u => u.OrdId).ToListAsync();
            var thisStockUOMs = await _dbContext.ms_stockuom.Where(u => u.ItemId == itemId).ToListAsync();

            var thisStockBOM = unionStockBOMs.Where(u => u.ItemId == itemId).Select(stock => new StockBOM
            {
                BOMId = bomId,
                OrdId = (short)(ordId + 1),// to get next ord
                ItemId = stock.ItemId,
                BaseUnit = stock.BaseUnit,
                UOMRate = stock.UOMRate,
                Qty = stock.Qty,
                StockFlg = stock.StockFlg
            }).FirstOrDefault();

            if (thisStockBOM?.StockFlg == 'S')
            {
                flag = true;
            }

            foreach (var stockBOM in thisStockBOMs)
            {
                stockBOM.UOMRate = 1;
            }

            var bOMModelList = new StockBOMModelList()
            {
                UnionStockBOMs = unionStockBOMs,
                ThisStockBOMs = thisStockBOMs,
                ThisStockUOMs = thisStockUOMs,
                StockBOM = thisStockBOM,
                BaseUnitFlg = flag
            };

            return PartialView("_AddStockBOMPartial", bOMModelList);
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
