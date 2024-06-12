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
    public class ICAssemblyController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ICAssemblyController(DatabaseServices dbServices, IHttpContextAccessor accessor, IWebHostEnvironment webHostEnvironment)
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

            var inventoryAssemblyList = await _dbContext.icmove
                .Join(_dbContext.icarapdetail,
                    inventory => inventory.IcMoveId,
                    detail => detail.IcMoveId,
                    (inventory, detail) => new InventoryMoveBillH
                    {
                        IcMoveId = inventory.IcMoveId,
                        IcTranTypCde = inventory.IcTranTypCde,
                        IcRefNo = inventory.IcRefNo2,
                        TranDte = inventory.TranDte,
                        FromLoc = detail.FromLoc
                    })
                .Where(inventory => inventory.IcTranTypCde == "REC")
                .GroupBy(inventory => inventory.IcMoveId)
                .Select(group => group.FirstOrDefault())
                .ToListAsync();

            var count = 1;
            foreach (var InventoryAssembly in inventoryAssemblyList)
            {
                InventoryAssembly.No = count;
                count++;
                InventoryAssembly.StringTranDate = ChangeDateFormat(InventoryAssembly.TranDte);
            }

            var reasons = await _dbContext.icreason.ToListAsync();

            var icAssembly = new InventoryMoveBillH()
            {
                TranDte = GetBizDate()
            };

            var ICAssemblyModelList = new ICAssemblyModelList
            {
                ICAssemblyList = inventoryAssemblyList,
                ICReasonList = reasons,
                RefNo = GenerateRefNo("ASM"),
                ICAssembly = icAssembly
            };

            return View(ICAssemblyModelList);
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

        public string GenerateRefNo(string Code)
        {
            // for inventory assembly refNo
            var refNo = _dbContext.ictranautonumber.FirstOrDefault(auto => auto.IcTranTypCde == Code);

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


        #region // Inventory Assembly methods //

        [HttpPost]
        public async Task<string> AddInventoryAssemblyDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                var formData = jsView.FormData;
                var tableData = jsView.TableData;

                var inventoryReceiveAssembly = BuildICAssemblyFromFormData(0, formData);
                var inventoryIssueAssembly = BuildICAssemblyFromFormData(0, formData);

                // Update ASM RefNo
                var inventoryAutoNumber = await _dbContext.ictranautonumber
                    .FirstOrDefaultAsync(auto => auto.IcTranTypCde == "ASM");

                if (inventoryAutoNumber != null)
                {
                    inventoryReceiveAssembly.IcRefNo2 = GenerateRefNo("ASM");
                    inventoryIssueAssembly.IcRefNo2 = GenerateRefNo("ASM");
                    inventoryAutoNumber.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(inventoryAutoNumber);
                }

                var receiveRefNo = GenerateRefNo("REC");
                var issueRefNo = GenerateRefNo("ISSU");

                // Update REC RefNo
                var icReceiveRefNo = await _dbContext.ictranautonumber.FirstOrDefaultAsync(auto => auto.IcTranTypCde == "REC");
                if (icReceiveRefNo != null)
                {
                    inventoryReceiveAssembly.IcRefNo = receiveRefNo;
                    inventoryReceiveAssembly.IcTranTypId = icReceiveRefNo.IcTranTypId;
                    inventoryReceiveAssembly.IcTranTypCde = icReceiveRefNo.IcTranTypCde;
                    icReceiveRefNo.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(icReceiveRefNo);
                }

                // Update ISSU RefNo
                var icIssueRefNo = await _dbContext.ictranautonumber.FirstOrDefaultAsync(auto => auto.IcTranTypCde == "ISSU");
                if (icIssueRefNo != null)
                {
                    inventoryIssueAssembly.IcRefNo = issueRefNo;
                    inventoryIssueAssembly.IcTranTypId = icIssueRefNo.IcTranTypId;
                    inventoryIssueAssembly.IcTranTypCde = icIssueRefNo.IcTranTypCde;
                    icIssueRefNo.LastUsedNo += 1;
                    _dbContext.ictranautonumber.Update(icIssueRefNo);
                }

                _dbContext.icmove.Add(inventoryReceiveAssembly);
                _dbContext.icmove.Add(inventoryIssueAssembly);
                await _dbContext.SaveChangesAsync();

                // Find Receive Details Id
                var dbIcMoveReceiveId = await _dbContext.icmove
                    .OrderByDescending(head => head.IcMoveId)
                    .Where(head => head.IcTranTypCde == "REC")
                    .Select(head => head.IcMoveId)
                    .FirstOrDefaultAsync();

                // Find Issue Details Id
                var dbIcMoveIssueId = await _dbContext.icmove
                    .OrderByDescending(head => head.IcMoveId)
                    .Where(head => head.IcTranTypCde == "ISSU")
                    .Select(head => head.IcMoveId)
                    .FirstOrDefaultAsync();

                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, dbIcMoveReceiveId, receiveRefNo, inventoryReceiveAssembly.RevUserId);
                    _dbContext.icarapdetail.Add(inventoryBillD);

                    var stockBOMList = await _dbContext.ms_stockbom
                        .Where(bom => bom.BOMId == inventoryBillD.ItemId)
                        .ToListAsync();

                    foreach (var stockBOM in stockBOMList)
                    {
                        var thisStock = await _dbContext.ms_stock
                            .FirstOrDefaultAsync(stock => stock.ItemId == stockBOM.ItemId);

                        if (thisStock != null)
                        {
                            var icIssueBillD = new InventoryBillD()
                            {
                                IcMoveId = dbIcMoveIssueId,
                                IcRefNo = issueRefNo,
                                OrdNo = stockBOM.OrdId,
                                FromLoc = row[1], // issue location
                                ToLoc = row[1], // issue location
                                ItemId = stockBOM.ItemId,
                                ItemDesc = thisStock.ItemDesc,
                                UOM = stockBOM.BaseUnit,
                                UOMRate = 1, // default
                                Qty = -(inventoryBillD.Qty * stockBOM.Qty), // issue
                                UnitCost = thisStock.UnitCost,
                                DiscAmt = 0, // default
                                Price = thisStock.SellingPrice,
                                RevUserId = inventoryReceiveAssembly.RevUserId,
                                RevDteTime = DateTime.Now
                            };
                            _dbContext.icarapdetail.Add(icIssueBillD);
                        }

                    }
                }

                await _dbContext.SaveChangesAsync();
                return "OK";
            }
            catch
            {
                return "Error";
            }
        }

        public async Task<string> UpdateInventoryAssemblyDetails([FromBody] InventoryJSView jsView)
        {
            try
            {
                Dictionary<string, string> formData = jsView.FormData;
                List<List<string>> tableData = jsView.TableData;
                var asmRefNo = jsView.ASMRefNo;

                var inventoryReceiveAssembly = UpdateICAssemblyFromFormData(asmRefNo, "REC", formData);
                var inventoryIssueAssembly = UpdateICAssemblyFromFormData(asmRefNo, "ISSU", formData);

                _dbContext.icmove.Update(inventoryReceiveAssembly);
                _dbContext.icmove.Update(inventoryIssueAssembly);

                //Delete previous inventory receive details to add new one
                await _dbContext.icarapdetail.Where(details => details.IcMoveId == inventoryReceiveAssembly.IcMoveId).ExecuteDeleteAsync();
                //Delete previous inventory issue details to add new one
                await _dbContext.icarapdetail.Where(details => details.IcMoveId == inventoryIssueAssembly.IcMoveId).ExecuteDeleteAsync();


                foreach (var row in tableData)
                {
                    var inventoryBillD = BuildInventoryBillD(row, inventoryReceiveAssembly.IcMoveId, inventoryReceiveAssembly.IcRefNo, inventoryReceiveAssembly.RevUserId);
                    _dbContext.icarapdetail.Add(inventoryBillD);

                    var stockBOMList = await _dbContext.ms_stockbom
                        .Where(bom => bom.BOMId == inventoryBillD.ItemId)
                        .ToListAsync();

                    foreach (var stockBOM in stockBOMList)
                    {
                        var thisStock = await _dbContext.ms_stock
                            .FirstOrDefaultAsync(stock => stock.ItemId == stockBOM.ItemId);

                        if (thisStock != null)
                        {
                            var icIssueBillD = new InventoryBillD()
                            {
                                IcMoveId = inventoryIssueAssembly.IcMoveId,
                                IcRefNo = inventoryIssueAssembly.IcRefNo,
                                OrdNo = stockBOM.OrdId,
                                FromLoc = row[1], // issue location
                                ToLoc = row[1], // issue location
                                ItemId = stockBOM.ItemId,
                                ItemDesc = thisStock.ItemDesc,
                                UOM = stockBOM.BaseUnit,
                                UOMRate = 1, // default
                                Qty = -(inventoryBillD.Qty * stockBOM.Qty),
                                UnitCost = thisStock.UnitCost,
                                DiscAmt = 0, // default
                                Price = thisStock.SellingPrice,
                                RevUserId = inventoryIssueAssembly.RevUserId,
                                RevDteTime = DateTime.Now
                            };
                            _dbContext.icarapdetail.Add(icIssueBillD);
                        }

                    }
                }


                await _dbContext.SaveChangesAsync();



                return "OK";
            }
            catch
            {
                return "Error";
            }
        }

        private InventoryMoveBillH BuildICAssemblyFromFormData(int icMoveId, Dictionary<string, string> formData)
        {
            var inventoryAssembly = new InventoryMoveBillH();

            if (icMoveId != 0)
            {
                inventoryAssembly = _dbContext.icmove.FirstOrDefault(head => head.IcMoveId == icMoveId);
            }

            if (inventoryAssembly == null)
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
                            inventoryAssembly.IcRefNo = field.Value;
                            break;
                        case "cancelFlg":
                            inventoryAssembly.CancelFlg = ParseBool(field.Value);
                            break;
                        case "tranDte":
                            inventoryAssembly.TranDte = ParseDateTime(field.Value);
                            break;
                        case "refNo2":
                            inventoryAssembly.IcRefNo2 = field.Value;
                            break;
                        case "reasonId":
                            inventoryAssembly.ReasonId = ParseInt16(field.Value);
                            break;
                        case "remark":
                            inventoryAssembly.Remark = field.Value;
                            break;
                    }
                }
            }

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var userId = _dbContext.ms_user
                .Where(user => user.UserCde == userCde)
                .Select(user => user.UserId)
                .FirstOrDefault();

            inventoryAssembly.RevUserId = userId;
            inventoryAssembly.RevDteTime = DateTime.Now;
            inventoryAssembly.AsmFlg = true;

            return inventoryAssembly;
        }

        private InventoryMoveBillH UpdateICAssemblyFromFormData(string asmRefNo, string type, Dictionary<string, string> formData)
        {
            var inventoryAssembly = new InventoryMoveBillH();

            if (asmRefNo != null)
            {
                inventoryAssembly = _dbContext.icmove.FirstOrDefault(head => head.IcRefNo2 == asmRefNo && head.IcTranTypCde == type);
            }

            if (inventoryAssembly == null)
            {
                return new InventoryMoveBillH();
            }

            foreach (var field in formData)
            {
                if (field.Value != null)
                {
                    switch (field.Key)
                    {
                        case "cancelFlg":
                            inventoryAssembly.CancelFlg = ParseBool(field.Value);
                            break;
                        case "tranDte":
                            inventoryAssembly.TranDte = ParseDateTime(field.Value);
                            break;
                        case "refNo2":
                            inventoryAssembly.IcRefNo2 = field.Value;
                            break;
                        case "reasonId":
                            inventoryAssembly.ReasonId = ParseInt16(field.Value);
                            break;
                        case "remark":
                            inventoryAssembly.Remark = field.Value;
                            break;
                    }
                }
            }

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;
            var userId = _dbContext.ms_user
                .Where(user => user.UserCde == userCde)
                .Select(user => user.UserId)
                .FirstOrDefault();

            inventoryAssembly.RevUserId = userId;
            inventoryAssembly.RevDteTime = DateTime.Now;
            inventoryAssembly.AsmFlg = true;

            return inventoryAssembly;
        }

        private static InventoryBillD BuildInventoryBillD(List<string> rowData, int icMoveId, string refNo, short userId)
        {
            return new InventoryBillD
            {
                IcMoveId = icMoveId,
                IcRefNo = refNo,
                OrdNo = short.Parse(rowData[0]),
                FromLoc = rowData[1], // receive location
                ToLoc = rowData[2], // receive location
                ItemId = rowData[3],
                ItemDesc = rowData[4],
                UOM = rowData[5],
                UOMRate = decimal.Parse(rowData[6]),
                Qty = decimal.Parse(rowData[7]),
                UnitCost = decimal.Parse(rowData[8]),
                DiscAmt = decimal.Parse(rowData[9]),
                Price = decimal.Parse(rowData[8]),
                RevUserId = userId,
                RevDteTime = DateTime.Now
            };
        }

        public async Task<InventoryMoveBillH> FindICAssemblyH(int icMoveId)
        {
            var inventoryAssemblyH = await _dbContext.icmove.FirstOrDefaultAsync(head => head.IcMoveId == icMoveId);
            return inventoryAssemblyH ?? new InventoryMoveBillH();
        }

        public async Task<List<InventoryBillD>> FindICAssemblyDetails(int icMoveId)
        {
            var inventoryAssemblyDetailsList = await _dbContext.icarapdetail
                .Where(detail => detail.IcMoveId == icMoveId && detail.IcRefNo.Contains("REC"))
                .ToListAsync();

            return inventoryAssemblyDetailsList;
        }

        #endregion


        #region // Print methods //

        public async Task<IActionResult> PrintReview(string refNo)
        {
            var icMove = await _dbContext.icmove.Where(head => head.IcRefNo2 == refNo)
                .Select(head => new InventoryReport
                {
                    icrefno = head.IcRefNo,
                    reasonid = _dbContext.icreason.Where(r => r.ICReasonId == head.ReasonId).Select(r => r.ICReasonCde).FirstOrDefault() ?? "",
                    revuserid = head.RevUserId,
                    remark = head.Remark,
                    trandte = head.TranDte.ToString("dd-MM-yyyy")
                })
                .ToListAsync();

            var icmoveId = await _dbContext.icmove.Where(head => head.IcRefNo2 == refNo).Select(head => head.IcMoveId).FirstOrDefaultAsync();

            if (icMove == null)
            {
                return NotFound();
            }


            var detailList = await _dbContext.icarapdetail
                .Where(detail => detail.IcMoveId == icmoveId)
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
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\ICAssemblyReport.rdlc";

                report.ReportPath = path;
                report.DataSources.Add(new ReportDataSource("ICAssemblyDataSet", detailList));
                report.DataSources.Add(new ReportDataSource("ICAssemblyH", icMove));

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
            var stocks = await _dbContext.ms_stock.Where(stock => stock.FinishGoodFlg == true).ToListAsync();
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

