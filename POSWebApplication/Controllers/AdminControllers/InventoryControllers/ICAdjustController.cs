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
    public class ICAdjustController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ICAdjustController(DatabaseServices dbServices, IHttpContextAccessor accessor, IWebHostEnvironment webHostEnvironment)
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
            }

        }


        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            var inventoryAdjustList = await _dbContext.icmove
                .Join(_dbContext.icarapdetail,
                    inventory => inventory.IcMoveId,
                    detail => detail.IcMoveId,
                    (inventory, detail) => new InventoryMoveBillH
                    {
                        IcMoveId = inventory.IcMoveId,
                        IcTranTypCde = inventory.IcTranTypCde,
                        IcRefNo = inventory.IcRefNo,
                        TranDte = inventory.TranDte,
                        FromLoc = detail.FromLoc
                    })
                .Where(inventory => inventory.IcTranTypCde == "ADJ")
                .GroupBy(inventory => inventory.IcMoveId)
                .Select(group => group.FirstOrDefault())
                .ToListAsync();

            var count = 1;
            foreach (var InventoryAdjust in inventoryAdjustList)
            {
                InventoryAdjust.No = count;
                count++;
                InventoryAdjust.StringTranDate = ChangeDateFormat(InventoryAdjust.TranDte);
            }

            var reasons = await _dbContext.icreason.ToListAsync();

            var icAdjust = new InventoryMoveBillH()
            {
                TranDte = GetBizDate()
            };

            var ICAdjustModelList = new ICAdjustModelList
            {
                ICAdjustList = inventoryAdjustList,
                ICReasonList = reasons,
                RefNo = GenerateRefNo(),
                ICAdjust = icAdjust
            };

            return View(ICAdjustModelList);
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
            // for inventory adjust refNo
            var refNo = _dbContext.ictranautonumber.FirstOrDefault(auto => auto.IcTranTypCde == "ADJ");

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


        #region // Inventory Adjust methods //

        [HttpPost]
        public async Task<string> AddInventoryAdjustDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                var formData = jsView.FormData;
                var tableData = jsView.TableData;

                var inventoryAdjust = BuildICAdjustFromFormData(0, formData);

                var inventoryAutoNumber = await _dbContext.ictranautonumber
                    .FirstOrDefaultAsync(auto => auto.IcTranTypCde == "ADJ");

                if (inventoryAutoNumber != null)
                {
                    inventoryAdjust.IcTranTypId = inventoryAutoNumber.IcTranTypId;
                    inventoryAdjust.IcTranTypCde = inventoryAutoNumber.IcTranTypCde;

                    inventoryAutoNumber.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(inventoryAutoNumber);
                }

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                inventoryAdjust.RevUserId = userId;
                inventoryAdjust.RevDteTime = DateTime.Now;

                _dbContext.icmove.Add(inventoryAdjust);
                await _dbContext.SaveChangesAsync(); // save first to get id for details

                var dbIcMoveId = await _dbContext.icmove
                    .OrderByDescending(head => head.IcMoveId)
                    .Select(head => head.IcMoveId)
                    .FirstOrDefaultAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, dbIcMoveId, inventoryAdjust.IcRefNo, userId);
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

        public async Task<string> UpdateInventoryAdjustDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                Dictionary<string, string> formData = jsView.FormData;
                List<List<string>> tableData = jsView.TableData;
                var icmoveId = jsView.IcMoveId;
                var inventoryAdjust = BuildICAdjustFromFormData(icmoveId, formData);

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                inventoryAdjust.RevUserId = userId;
                inventoryAdjust.RevDteTime = DateTime.Now;

                _dbContext.icmove.Update(inventoryAdjust);

                //Delete previous inventory adjust details to add new one
                await _dbContext.icarapdetail.Where(details => details.IcMoveId == icmoveId).ExecuteDeleteAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, icmoveId, inventoryAdjust.IcRefNo, userId);
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

        private InventoryMoveBillH BuildICAdjustFromFormData(int icMoveId, Dictionary<string, string> formData)
        {
            var inventoryAdjust = new InventoryMoveBillH();

            if (icMoveId != 0)
            {
                inventoryAdjust = _dbContext.icmove.FirstOrDefault(head => head.IcMoveId == icMoveId);
            }

            if (inventoryAdjust == null)
            {
                return new InventoryMoveBillH();
            }

            foreach (var field in formData)
            {
                if (field.Value != null)
                {
                    switch (field.Key)
                    {
                        case "refNo":
                            inventoryAdjust.IcRefNo = field.Value;
                            break;
                        case "cancelFlg":
                            inventoryAdjust.CancelFlg = ParseBool(field.Value);
                            break;
                        case "tranDte":
                            inventoryAdjust.TranDte = ParseDateTime(field.Value);
                            break;
                        case "refNo2":
                            inventoryAdjust.IcRefNo2 = field.Value;
                            break;
                        case "reasonId":
                            inventoryAdjust.ReasonId = ParseInt16(field.Value);
                            break;
                        case "remark":
                            inventoryAdjust.Remark = field.Value;
                            break;
                    }
                }
            }

            return inventoryAdjust;
        }

        private static InventoryBillD BuildInventoryBillD(List<string> rowData, int icmoveId, string refNo, short userId)
        {
            var adjust = rowData[1];
            var inventoryBillD = new InventoryBillD
            {
                IcMoveId = icmoveId,
                IcRefNo = refNo,
                OrdNo = short.Parse(rowData[0]),
                FromLoc = rowData[2],
                ToLoc = rowData[2],
                ItemId = rowData[3],
                ItemDesc = rowData[4],
                UOM = rowData[5],
                UOMRate = decimal.Parse(rowData[6]),
                Qty = decimal.Parse(rowData[7]),
                UnitCost = decimal.Parse(rowData[8]),
                Price = decimal.Parse(rowData[8]),
                RevUserId = userId,
                RevDteTime = DateTime.Now
            };
            if (adjust == "-")
            {
                inventoryBillD.Qty = -inventoryBillD.Qty; // make - when adjust is -
            }
            return inventoryBillD;
        }

        public async Task<List<InventoryBillD>> FindICAdjustDetails(int icMoveId)
        {
            var inventoryAdjustDetailsList = await _dbContext.icarapdetail.Where(detail => detail.IcMoveId == icMoveId).ToListAsync();
            return inventoryAdjustDetailsList;
        }

        public async Task<InventoryMoveBillH> FindICAdjustH(int icMoveId)
        {
            var inventoryAdjustH = await _dbContext.icmove.FirstOrDefaultAsync(head => head.IcMoveId == icMoveId);
            return inventoryAdjustH ?? new InventoryMoveBillH();
        }

        [HttpPost]
        public async Task DeleteICAdjustDetails(int icmoveId)
        {
            await _dbContext.icarapdetail.Where(ic => ic.IcMoveId == icmoveId).ExecuteDeleteAsync();
            await _dbContext.icmove.Where(ic => ic.IcMoveId == icmoveId).ExecuteDeleteAsync();
            _dbContext.SaveChanges();
        }

        #endregion


        #region // Print methods //

        public async Task<IActionResult> PrintReview(string refNo)
        {
            var icMove = await _dbContext.icmove.Where(head => head.IcRefNo == refNo)
                .Select(head => new InventoryReport
                {
                    icrefno = head.IcRefNo,
                    reasonid = _dbContext.icreason.Where(r => r.ICReasonId == head.ReasonId).Select(r => r.ICReasonCde).FirstOrDefault() ?? "",
                    revuserid = head.RevUserId,
                    remark = head.Remark,
                    trandte = head.TranDte.ToString("dd-MM-yyyy")
                })
                .ToListAsync();

            if (icMove == null)
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

            foreach (var detail in detailList) // change back to positive
            {
                if (detail.qty < 0)
                {
                    detail.qty = -detail.qty;
                    detail.toloc = "-";
                }
                else
                {
                    detail.toloc = "+";
                }
            }

            try
            {
                var report = new Microsoft.Reporting.NETCore.LocalReport();
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\ICAdjustReport.rdlc";

                report.ReportPath = path;
                report.DataSources.Add(new ReportDataSource("ICAdjustDataSet", detailList));
                report.DataSources.Add(new ReportDataSource("ICAdjustH", icMove));

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

