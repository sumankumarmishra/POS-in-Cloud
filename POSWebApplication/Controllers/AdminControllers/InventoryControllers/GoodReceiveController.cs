using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class GoodReceiveController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly DatabaseSettings _databaseSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public GoodReceiveController(DatabaseSettings databaseSettings, IWebHostEnvironment webHostEnvironment)
        {
            _databaseSettings = databaseSettings;
            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            _dbContext = new POSWebAppDbContext(optionsBuilder.Options);
            _webHostEnvironment = webHostEnvironment;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }


        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            var GoodReceiveList = await _dbContext.icarap
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
                .Where(inventory => inventory.IcTranTypCde == "GRN")
                .ToListAsync();

            var count = 1;
            foreach (var GoodReceive in GoodReceiveList)
            {
                GoodReceive.No = count;
                count++;
                GoodReceive.StringTranDate = ChangeDateFormat(GoodReceive.TranDte);
            }

            var suppliers = await _dbContext.ms_apvend.ToListAsync();

            var goodReceive = new InventoryBillH()
            {
                TranDte = GetBizDate(),
                DepositDte = GetBizDate()
            };

            var GoodReceiveModelList = new GoodReceiveModelList
            {
                GoodReceiveList = GoodReceiveList,
                Suppliers = suppliers,
                RefNo = GenerateRefNo(),
                GoodReceive = goodReceive
            };

            return View(GoodReceiveModelList);
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
                    ViewData["Database"] = $"{_databaseSettings.DbName}({company.POSPkgNme})";
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
            // for goodReceive refNo
            var refNo = _dbContext.ictranautonumber.FirstOrDefault(auto => auto.IcTranTypCde == "GRN");

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


        #region // Good Receive methods //

        [HttpPost]
        public async Task<String> AddGoodReceiveDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                var formData = jsView.FormData;
                var tableData = jsView.TableData;

                var goodReceive = BuildGoodReceiveFromFormData(0, formData);

                var inventoryAutoNumber = await _dbContext.ictranautonumber
                    .FirstOrDefaultAsync(auto => auto.IcTranTypCde == "GRN");

                if (inventoryAutoNumber != null)
                {
                    goodReceive.IcTranTypId = inventoryAutoNumber.IcTranTypId;
                    goodReceive.IcTranTypCde = inventoryAutoNumber.IcTranTypCde;

                    inventoryAutoNumber.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(inventoryAutoNumber);
                }

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                goodReceive.CreateUserId = userId;
                goodReceive.RevUserId = userId;

                goodReceive.CreateDteTime = DateTime.Now;
                goodReceive.RevDteTime = DateTime.Now;

                _dbContext.icarap.Add(goodReceive);
                await _dbContext.SaveChangesAsync(); // Save first to get id for details

                var dbArapId = await _dbContext.icarap
                    .OrderByDescending(head => head.ArapId)
                    .Select(head => head.ArapId)
                    .FirstOrDefaultAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, dbArapId, goodReceive.IcRefNo, userId);
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

        public async Task<String> UpdateGoodReceiveDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                Dictionary<string, string> formData = jsView.FormData;
                List<List<string>> tableData = jsView.TableData;
                var arapId = jsView.ArapId;
                var goodReceive = BuildGoodReceiveFromFormData(arapId, formData);

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                goodReceive.RevUserId = userId;
                goodReceive.RevDteTime = DateTime.Now;

                _dbContext.icarap.Update(goodReceive);

                //Delete previous good receive details to add new one
                await _dbContext.icarapdetail.Where(details => details.ArapId == arapId).ExecuteDeleteAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, arapId, goodReceive.IcRefNo, userId);
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

        private InventoryBillH BuildGoodReceiveFromFormData(int arapId, Dictionary<string, string> formData)
        {
            var goodReceive = new InventoryBillH();

            if (arapId != 0)
            {
                goodReceive = _dbContext.icarap.FirstOrDefault(head => head.ArapId == arapId);
            }

            if (goodReceive == null)
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
                            goodReceive.IcRefNo = field.Value;
                            break;
                        case "cancelFlg":
                            goodReceive.CancelFlg = ParseBool(field.Value);
                            break;
                        case "tranDte":
                            goodReceive.TranDte = ParseDateTime(field.Value);
                            break;
                        case "refNo2":
                            goodReceive.RefNo2 = field.Value;
                            break;
                        case "depositDte":
                            goodReceive.DepositDte = ParseDateTime(field.Value);
                            break;
                        case "apId":
                            goodReceive.ApId = ParseInt32(field.Value);
                            break;
                        case "arapDesc":
                            goodReceive.ArapDesc = field.Value;
                            break;
                        case "depositAmt":
                            goodReceive.DepositAmt = ParseDecimal(field.Value);
                            break;
                        case "tradeCurrCde":
                            goodReceive.TradeCurrCde = field.Value;
                            break;
                        case "billTerm":
                            goodReceive.BillTerm = field.Value;
                            break;
                        case "billTermDay":
                            goodReceive.BillTermDay = ParseInt16(field.Value);
                            break;
                        case "otherChrgAmt":
                            goodReceive.OtherChrgAmt = ParseDecimal(field.Value);
                            break;
                        case "remark":
                            goodReceive.Remark = field.Value;
                            break;
                        case "tenderCde":
                            goodReceive.TenderCde = field.Value;
                            break;
                        case "billDiscAmt":
                            goodReceive.BillDiscAmt = ParseDecimal(field.Value);
                            break;
                        case "billAmt":
                            goodReceive.BillAmt = ParseDecimal(field.Value);
                            break;
                    }
                }
            }

            return goodReceive;
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
                Qty = decimal.Parse(rowData[6]),
                UnitCost = decimal.Parse(rowData[7]),
                DiscAmt = decimal.Parse(rowData[8]),
                Price = decimal.Parse(rowData[7]),
                RevUserId = userId,
                RevDteTime = DateTime.Now
            };
        }

        public async Task<List<InventoryBillD>> FindGoodReceiveDetails(int arapId)
        {
            var goodReceiveDetailsList = await _dbContext.icarapdetail.Where(detail => detail.ArapId == arapId).ToListAsync();
            return goodReceiveDetailsList;
        }

        public async Task<InventoryBillH> FindGoodReceiveH(int arapId)
        {
            var goodReceiveH = await _dbContext.icarap.FirstOrDefaultAsync(head => head.ArapId == arapId);
            return goodReceiveH;
        }

        [HttpPost]
        public async Task DeleteGoodReceiveDetails(int arapId)
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
                    tendercde = head.TenderCde,
                    tradecurrcde = head.TradeCurrCde,
                    trandte = head.TranDte.ToString("dd-MM-yyyy")
                })
                .ToListAsync();

            if (icArap == null)
            {
                return NotFound();
            }

            var detailList = await _dbContext.icarapdetail
                .Where(detail => detail.IcRefNo == refNo)
                .Select(billD => new InventoryReport
                {
                    fromloc = billD.FromLoc,
                    itemid = billD.ItemId,
                    uom = billD.UOM,
                    uomrate = billD.UOMRate,
                    qty = billD.Qty,
                    unitcost = billD.UnitCost,
                    discamt = billD.DiscAmt
                })
                .ToListAsync();

            try
            {
                var report = new Microsoft.Reporting.NETCore.LocalReport();
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\GoodReceiveReport.rdlc";

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

        static short ParseInt16(string value)
        {
            if (short.TryParse(value, out short result))
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
            return stock;
        }

        public async Task<List<StockUOM>> GetStockUOMs(string itemId)
        {
            var stockUOMs = await _dbContext.ms_stockuom.Where(uom => uom.ItemId == itemId).ToListAsync();
            return stockUOMs;
        }

        public async Task<StockUOM> GetStockUOMsByUOMCde(string itemId, string uomCde)
        {
            var stockUOM = await _dbContext.ms_stockuom.FirstOrDefaultAsync(uom => uom.ItemId == itemId && uom.UOMCde == uomCde);
            return stockUOM;
        }

        #endregion


    }
}

