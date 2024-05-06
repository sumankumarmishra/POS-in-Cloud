using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Reporting.NETCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.InventoryControllers
{
    [Authorize]
    public class ICIssueController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ICIssueController(POSWebAppDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }

        // Common functions

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            var inventoryIssueList = await _dbContext.icmove
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
                .Where(inventory => inventory.IcTranTypCde == "ISSU")
                .GroupBy(inventory => inventory.IcMoveId)
                .Select(group => group.FirstOrDefault())
                .ToListAsync();

            var count = 1;

            foreach (var InventoryIssue in inventoryIssueList)
            {
                InventoryIssue.No = count;
                count++;
                InventoryIssue.StringTranDate = ChangeDateFormat(InventoryIssue.TranDte);
            }

            var reasons = await _dbContext.icreason.ToListAsync();

            var icIssue = new InventoryMoveBillH()
            {
                TranDte = GetBizDate()
            };

            var ICIssueModelList = new ICIssueModelList
            {
                ICIssueList = inventoryIssueList,
                ICReasonList = reasons,
                RefNo = GenerateRefNo(),
                ICIssue = icIssue
            };

            return View(ICIssueModelList);
        }

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

                var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

                var bizDte = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == POS.POSid)
                    .Select(auto => auto.BizDte)
                    .FirstOrDefault();

                ViewData["Business Date"] = bizDte.ToString("dd MMM yyyy");
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
            // for inventory issue refNo
            var refNo = _dbContext.ictranautonumber.FirstOrDefault(auto => auto.IcTranTypCde == "ISSU");

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

        /*Add and update Inventory Issue*/

        [HttpPost]
        public async Task<String> AddInventoryIssueDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                var formData = jsView.FormData;
                var tableData = jsView.TableData;

                var inventoryIssue = BuildICIssueFromFormData(0, formData);

                var inventoryAutoNumber = await _dbContext.ictranautonumber
                    .FirstOrDefaultAsync(auto => auto.IcTranTypCde == "ISSU");

                if (inventoryAutoNumber != null)
                {
                    inventoryIssue.IcTranTypId = inventoryAutoNumber.IcTranTypId;
                    inventoryIssue.IcTranTypCde = inventoryAutoNumber.IcTranTypCde;

                    inventoryAutoNumber.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(inventoryAutoNumber);
                }

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                inventoryIssue.RevUserId = userId;
                inventoryIssue.RevDteTime = DateTime.Now;

                _dbContext.icmove.Add(inventoryIssue); // save first to get id for details
                await _dbContext.SaveChangesAsync();

                var dbIcMoveId = await _dbContext.icmove
                    .OrderByDescending(head => head.IcMoveId)
                    .Select(head => head.IcMoveId)
                    .FirstOrDefaultAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, dbIcMoveId, inventoryIssue.IcRefNo, userId);
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

        public async Task<String> UpdateInventoryIssueDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                Dictionary<string, string> formData = jsView.FormData;
                List<List<string>> tableData = jsView.TableData;
                var icmoveId = jsView.IcMoveId;
                var inventoryIssue = BuildICIssueFromFormData(icmoveId, formData);

                var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
                var userId = await _dbContext.ms_user
                    .Where(user => user.UserCde == userCde)
                    .Select(user => user.UserId)
                    .FirstOrDefaultAsync();

                inventoryIssue.RevUserId = userId;
                inventoryIssue.RevDteTime = DateTime.Now;

                _dbContext.icmove.Update(inventoryIssue);

                //Delete previous inventory issue details to add new one
                await _dbContext.icarapdetail.Where(details => details.IcMoveId == icmoveId).ExecuteDeleteAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, icmoveId, inventoryIssue.IcRefNo, userId);
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

        private InventoryMoveBillH BuildICIssueFromFormData(int icMoveId, Dictionary<string, string> formData)
        {
            var inventoryIssue = new InventoryMoveBillH();

            if (icMoveId != 0)
            {
                inventoryIssue = _dbContext.icmove.FirstOrDefault(head => head.IcMoveId == icMoveId);
            }

            if (inventoryIssue == null)
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
                            inventoryIssue.IcRefNo = field.Value;
                            break;
                        case "cancelFlg":
                            inventoryIssue.CancelFlg = ParseBool(field.Value);
                            break;
                        case "tranDte":
                            inventoryIssue.TranDte = ParseDateTime(field.Value);
                            break;
                        case "refNo2":
                            inventoryIssue.IcRefNo2 = field.Value;
                            break;
                        case "reasonId":
                            inventoryIssue.ReasonId = ParseInt16(field.Value);
                            break;
                        case "remark":
                            inventoryIssue.Remark = field.Value;
                            break;
                    }
                }
            }

            return inventoryIssue;
        }

        private static InventoryBillD BuildInventoryBillD(List<string> rowData, int icMoveId, string refNo, short userId)
        {
            return new InventoryBillD
            {
                IcMoveId = icMoveId,
                IcRefNo = refNo,
                OrdNo = short.Parse(rowData[0]),
                FromLoc = rowData[1],
                ToLoc = rowData[1],
                ItemId = rowData[2],
                ItemDesc = rowData[3],
                UOM = rowData[4],
                UOMRate = decimal.Parse(rowData[5]),
                Qty = -decimal.Parse(rowData[6]), // Since it is issue, qty should be negative.
                UnitCost = decimal.Parse(rowData[7]),
                DiscAmt = decimal.Parse(rowData[8]),
                Price = decimal.Parse(rowData[7]),
                RevUserId = userId,
                RevDteTime = DateTime.Now
            };
        }

        /*Edit Inventory Issue*/

        public async Task<List<InventoryBillD>> FindICIssueDetails(int icMoveId)
        {
            var inventoryIssueDetailsList = await _dbContext.icarapdetail.Where(detail => detail.IcMoveId == icMoveId).ToListAsync();
            return inventoryIssueDetailsList;
        }

        public async Task<InventoryMoveBillH> FindICIssueH(int icMoveId)
        {
            var inventoryIssueH = await _dbContext.icmove.FirstOrDefaultAsync(head => head.IcMoveId == icMoveId);
            return inventoryIssueH;
        }

        /*Delete Inventory Issue*/
        [HttpPost]
        public async Task DeleteICIssueDetails(int icmoveId)
        {
            await _dbContext.icarapdetail.Where(ic => ic.IcMoveId == icmoveId).ExecuteDeleteAsync();
            await _dbContext.icmove.Where(ic => ic.IcMoveId == icmoveId).ExecuteDeleteAsync();
            _dbContext.SaveChanges();
        }

        /*Print Report*/

        public async Task<IActionResult> PrintReview(string refNo)
        {
            var icMove = await _dbContext.icmove.Where(head => head.IcRefNo == refNo)
                .Select(head => new InventoryReport
                {
                    icrefno = head.IcRefNo,
                    reasonid = _dbContext.icreason.Where(r => r.ICReasonId == head.ReasonId).Select(r => r.ICReasonCde).FirstOrDefault(),
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
                    qty = -billD.Qty,
                    unitcost = billD.UnitCost,
                    discamt = billD.DiscAmt
                })
                .ToListAsync();

            try
            {
                var report = new Microsoft.Reporting.NETCore.LocalReport();
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\ICIssueReport.rdlc";

                report.ReportPath = path;
                report.DataSources.Add(new ReportDataSource("ICIssueDataSet", detailList));
                report.DataSources.Add(new ReportDataSource("ICIssueH", icMove));

                byte[] htmlBytes = report.Render("HTML4.0");

                return File(htmlBytes, "text/html");
            }
            catch
            {
                return BadRequest("An error occurred while generating the report. Please try again later.");
            }
        }

        /* Utility methods for parsing */

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

        /* For JSView Input */

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
    }
}

