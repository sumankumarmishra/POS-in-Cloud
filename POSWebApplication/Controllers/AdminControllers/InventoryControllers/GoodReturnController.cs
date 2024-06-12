using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Reporting.NETCore;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class GoodReturnController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GoodReturnController(DatabaseServices dbServices, IHttpContextAccessor accessor, IWebHostEnvironment webHostEnvironment)
        {
            var connection = accessor.HttpContext?.Session.GetString("Connection") ?? "";
            if (connection.IsNullOrEmpty())
            {
                accessor.HttpContext?.Response.Redirect("../SystemSettings/Index");
            }
            else
            {
                _dbContext = new POSWebAppDbContext(dbServices.ConnectDatabase(connection));
                _webHostEnvironment = webHostEnvironment;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            }

        }

        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            var GoodReturnList = await _dbContext.icarap
                .Join(_dbContext.ms_apvend,
                    inventory => inventory.ApId,
                    supplier => supplier.ApId,
                    (inventory, supplier) => new InventoryBillH
                    {
                        ArapId = inventory.ArapId,
                        IcTranTypCde = inventory.IcTranTypCde,
                        IcRefNo = inventory.IcRefNo,
                        TranDte = inventory.TranDte,
                        Supplier = supplier.ApNme
                    })
                .Where(inventory => inventory.IcTranTypCde == "GRTN")
                .ToListAsync();

            var count = 1;
            foreach (var GoodReturn in GoodReturnList)
            {
                GoodReturn.No = count;
                count++;
                GoodReturn.StringTranDate = ChangeDateFormat(GoodReturn.TranDte);
            }

            var suppliers = await _dbContext.ms_apvend.ToListAsync();

            var goodReturn = new InventoryBillH()
            {
                TranDte = GetBizDate(),
                DepositDte = GetBizDate()
            };

            var GoodReturnModelList = new GoodReturnModelList
            {
                GoodReturnList = GoodReturnList,
                Suppliers = suppliers,
                RefNo = GenerateRefNo(),
                GoodReturn = goodReturn,
            };

            return View(GoodReturnModelList);
        }

        #endregion


        #region // Common methods //
        private static string ChangeDateFormat(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            return dateOnly.ToString("dd-MM-yyyy");
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

        protected DateTime GetBizDate()
        {
            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserCde == userCde);
            var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

            var bizDte = _dbContext.ms_autonumber
                .Where(auto => auto.PosId == POS.POSid)
                .Select(auto => auto.BizDte)
                .FirstOrDefault();

            return bizDte;
        }

        public string GenerateRefNo()
        {
            // for good return refNo
            var refNo = _dbContext.ictranautonumber.FirstOrDefault(auto => auto.IcTranTypCde == "GRTN");

            var generateNo = (refNo.LastUsedNo + 1).ToString();
            if (refNo.ZeroLeading)
            {
                var totalWidth = refNo.RunningNo - refNo.IcTranPrefix.Length - generateNo.Length;
                string paddedString = new string('0', totalWidth) + generateNo;
                return refNo.IcTranPrefix + paddedString;
            }
            else
            {
                return refNo.IcTranPrefix + generateNo;
            }
        }

        #endregion


        #region // Good Return methods //

        [HttpPost]
        public async Task<String> AddGoodReturnDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                var formData = jsView.FormData;
                var tableData = jsView.TableData;

                var goodReturn = BuildGoodReturnFromFormData(0, formData);

                var inventoryAutoNumber = await _dbContext.ictranautonumber
                    .FirstOrDefaultAsync(auto => auto.IcTranTypCde == "GRTN");

                if (inventoryAutoNumber != null)
                {
                    goodReturn.IcTranTypId = inventoryAutoNumber.IcTranTypId;
                    goodReturn.IcTranTypCde = inventoryAutoNumber.IcTranTypCde;

                    inventoryAutoNumber.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(inventoryAutoNumber);
                }

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                goodReturn.CreateUserId = userId;
                goodReturn.RevUserId = userId;

                goodReturn.CreateDteTime = DateTime.Now;
                goodReturn.RevDteTime = DateTime.Now;

                _dbContext.icarap.Add(goodReturn);
                await _dbContext.SaveChangesAsync();

                var dbArapId = await _dbContext.icarap
                    .OrderByDescending(head => head.ArapId)
                    .Select(head => head.ArapId)
                    .FirstOrDefaultAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, dbArapId, goodReturn.IcRefNo, userId);
                    _dbContext.icarapdetail.Add(inventoryBillD);
                }

                await _dbContext.SaveChangesAsync();
                return "OK";
            }
            catch
            {
                return "Error";
            }

        }

        public async Task<String> UpdateGoodReturnDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                Dictionary<string, string> formData = jsView.FormData;
                List<List<string>> tableData = jsView.TableData;
                var arapId = jsView.ArapId;
                var goodReturn = BuildGoodReturnFromFormData(arapId, formData);

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                goodReturn.RevUserId = userId;
                goodReturn.RevDteTime = DateTime.Now;

                _dbContext.icarap.Update(goodReturn);

                //Delete previous good return details to add new one
                await _dbContext.icarapdetail.Where(details => details.ArapId == arapId).ExecuteDeleteAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, arapId, goodReturn.IcRefNo, userId);
                    _dbContext.icarapdetail.Update(inventoryBillD);
                }


                await _dbContext.SaveChangesAsync();
                return "OK";
            }
            catch
            {
                return "Error";
            }
        }

        private InventoryBillH BuildGoodReturnFromFormData(int arapId, Dictionary<string, string> formData)
        {
            var goodReturn = new InventoryBillH();

            if (arapId != 0)
            {
                goodReturn = _dbContext.icarap.FirstOrDefault(head => head.ArapId == arapId);
            }

            if (goodReturn == null)
            {
                return new InventoryBillH();
            }

            foreach (var field in formData)
            {
                if (field.Value != null)
                {
                    switch (field.Key)
                    {
                        case "refNo":
                            goodReturn.IcRefNo = field.Value;
                            break;
                        case "cancelFlg":
                            goodReturn.CancelFlg = ParseBool(field.Value);
                            break;
                        case "tranDte":
                            goodReturn.TranDte = ParseDateTime(field.Value);
                            break;
                        case "refNo2":
                            goodReturn.RefNo2 = field.Value;
                            break;
                        case "apId":
                            goodReturn.ApId = ParseInt32(field.Value);
                            break;
                        case "returnDesp":
                            goodReturn.ArapDesc = field.Value;
                            break;
                        case "remark":
                            goodReturn.Remark = field.Value;
                            break;
                        case "billAmt":
                            goodReturn.BillAmt = ParseDecimal(field.Value);
                            break;
                    }
                }
            }

            return goodReturn;
        }

        private static InventoryBillD BuildInventoryBillD(List<string> rowData, int arapId, string refNo, short userId)
        {
            return new InventoryBillD
            {
                ArapId = arapId,
                IcRefNo = refNo,
                OrdNo = short.Parse(rowData[0]),
                FromLoc = rowData[1],
                ToLoc = rowData[1],
                ItemId = rowData[2],
                ItemDesc = rowData[3],
                UOM = rowData[4],
                UOMRate = decimal.Parse(rowData[5]),
                Qty = -decimal.Parse(rowData[6]), // Since it is return, qty should be negative.
                UnitCost = decimal.Parse(rowData[7]),
                DiscAmt = decimal.Parse(rowData[8]),
                Price = decimal.Parse(rowData[7]),
                RevUserId = userId,
                RevDteTime = DateTime.Now
            };
        }

        public async Task<List<InventoryBillD>> FindGoodReturnDetails(int arapId)
        {
            var goodReturnDetailsList = await _dbContext.icarapdetail.Where(detail => detail.ArapId == arapId).ToListAsync();
            return goodReturnDetailsList;
        }

        public async Task<InventoryBillH> FindGoodReturnH(int arapId)
        {
            var goodReturnH = await _dbContext.icarap.FirstOrDefaultAsync(head => head.ArapId == arapId);
            return goodReturnH;
        }

        [HttpPost]
        public async Task DeleteGoodReturnDetails(int arapId)
        {
            await _dbContext.icarapdetail.Where(ic => ic.ArapId == arapId).ExecuteDeleteAsync();
            await _dbContext.icarap.Where(ic => ic.ArapId == arapId).ExecuteDeleteAsync();
            _dbContext.SaveChanges();
        }

        #endregion


        #region // Print methods //

        public async Task<IActionResult> PrintReview(string refNo)
        {
            var icArap = await _dbContext.icarap.Where(head => head.IcRefNo == refNo)
                .Select(head => new InventoryReport
                {
                    icrefno = head.IcRefNo,
                    apid = _dbContext.ms_apvend.Where(supplier => supplier.ApId == head.ApId).Select(supplier => supplier.ApAcCde).FirstOrDefault(),
                    billtermday = head.BillTermDay,
                    tendercde = "MMK",
                    trandte = head.TranDte.ToString("dd-MM-yyyy")
                })
                .ToListAsync();

            var detailList = await _dbContext.icarapdetail
                .Where(detail => detail.IcRefNo == refNo)
                .Select(billD => new InventoryReport
                {
                    fromloc = billD.FromLoc,
                    itemid = billD.ItemId,
                    uom = billD.UOM,
                    uomrate = billD.UOMRate,
                    qty = -billD.Qty,
                    unitcost = billD.UnitCost,
                    discamt = billD.DiscAmt
                })
                .ToListAsync();
            try
            {
                var report = new Microsoft.Reporting.NETCore.LocalReport();
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\GoodReturnReport.rdlc";

                report.ReportPath = path;
                report.DataSources.Add(new ReportDataSource("ArapDetails", detailList));
                report.DataSources.Add(new ReportDataSource("Arap", icArap));

                byte[] htmlBytes = report.Render("HTML4.0");

                return File(htmlBytes, "text/html");
            }
            catch
            {
                return BadRequest("An error occurred while generating the report. Please try again later.");
            }

        }

        #endregion


        #region // Parsing methods //

        static Boolean ParseBool(string value)
        {
            if (Boolean.TryParse(value, out Boolean result))
            {
                return result;
            }
            return default;
        }

        static DateTime ParseDateTime(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
            {
                return result;
            }
            return default;
        }

        static int ParseInt32(string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }
            return default;
        }

        static decimal ParseDecimal(string value)
        {
            if (decimal.TryParse(value, out decimal result))
            {
                return result;
            }
            return default;
        }

        #endregion


        #region // JS methods //

        public async Task<IEnumerable<Location>> GetLocations()
        {
            var locations = await _dbContext.ms_location.Where(location => location.IsOutlet == false).ToListAsync();
            return locations;
        }

        public async Task<IEnumerable<Stock>> GetStocks()
        {
            var stocks = await _dbContext.ms_stock.ToListAsync();
            return stocks;
        }

        public async Task<int> GetApIdbyArapId(int arapId)
        {
            var apId = await _dbContext.icarap
                .Where(ic => ic.ArapId == arapId)
                .Select(ic => ic.ApId)
                .FirstOrDefaultAsync();

            return apId;
        }

        public async Task<IEnumerable<SupplierItem>> GetStocksByApId(int apId)
        {
            var stocks = await _dbContext.ms_stockapvend.Where(stk => stk.ApId == apId).ToListAsync();
            return stocks;
        }

        public async Task<IEnumerable<SupplierItem>> GetStocksByArapId(int arapId)
        {
            var stocks = await _dbContext.icarap
                .Where(ic => ic.ArapId == arapId)
                .Join(_dbContext.ms_stockapvend,
                ic => ic.ApId,
                stk => stk.ApId,
                (ic, stk) => new SupplierItem
                {
                    ItemId = stk.ItemId
                })
                .ToListAsync();

            return stocks;
        }

        public async Task<Stock> GetStocksByItemId(string itemId)
        {
            var stock = await _dbContext.ms_stock.FirstOrDefaultAsync(stock => stock.ItemId == itemId);
            return stock ?? new Stock();
        }

        public async Task<List<StockUOM>> GetStockUOMs(string itemId)
        {
            var stockUOMs = await _dbContext.ms_stockuom.Where(uom => uom.ItemId == itemId).ToListAsync();
            return stockUOMs;
        }

        public async Task<StockUOM> GetStockUOMsByUOMCde(string itemId, string uomCde)
        {
            var stockUOM = await _dbContext.ms_stockuom.FirstOrDefaultAsync(uom => uom.ItemId == itemId && uom.UOMCde == uomCde);
            return stockUOM ?? new StockUOM();
        }

        #endregion
    }
}

