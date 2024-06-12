using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSWebApplication.Data;
using POSWebApplication.Models;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Reporting.NETCore;
using POSinCloud.Services;


namespace POSWebApplication.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(DatabaseServices dbServices, IHttpContextAccessor accessor, IWebHostEnvironment webHostEnvironment)
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
            if (!HttpContext.User.Claims.Any() || _dbContext == null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["alert message"] = "Session Expired!";
                return RedirectToAction("Index", "LogIn");
            }

            SetLayOutData();

            if (TempData["alert message"] != null)
            {
                ViewBag.AlertMessage = TempData["alert message"];
            }

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

            var dbUser = await _dbContext.ms_user.FirstOrDefaultAsync(user => user.UserCde == userCde);

            var posId = await _dbContext.ms_userpos
                .Where(userPOS => userPOS.UserId == dbUser.UserId)
                .Select(userPOS => userPOS.POSid)
                .FirstOrDefaultAsync();

            var autoNumber = await _dbContext.ms_autonumber.FirstOrDefaultAsync(pos => pos.PosId == posId);

            autoNumber.BizDteString = ChangeDateFormat(autoNumber.BizDte);

            // Main methods
            var billHList = await _dbContext.billh
                .Where(billH => billH.Status == 'P' && billH.BizDte.Date == autoNumber.BizDte.Date && billH.POSId == posId)
                .Join(_dbContext.billp,
                billH => billH.BillhId,
                billP => billP.BillhID,
                (billH, billP) => new Billh
                {
                    BillhId = billH.BillhId,
                    BizDte = billH.BizDte,
                    BillNo = billH.BillNo,
                    LocCde = billH.LocCde,
                    ShiftNo = billH.ShiftNo,
                    GuestNme = billH.GuestNme,
                    BillDiscount = billH.BillDiscount,
                    Remark = billH.Remark,
                    Status = billH.Status,
                    POSId = billH.POSId,
                    UserId = billH.UserId,
                    SaleType = billP.CurrTyp,
                    RevDteTime = billH.RevDteTime
                })
                .ToListAsync();

            foreach (var billH in billHList)
            {
                var sale = await _dbContext.billp
                    .FirstOrDefaultAsync(billp => billp.BillhID == billH.BillhId && billp.CurrTyp == billH.SaleType);

                if (sale != null)
                {
                    billH.TotalAmount = sale.LocalAmt - sale.ChangeAmt;
                }
                billH.Action = "Sales";
                billH.User = userCde;
            }

            //var saleTotal = _dbContext.billp.Sum(billp => billp.LocalAmt - billp.ChangeAmt);

            var home = new Home()
            {
                AutoNumber = autoNumber,
                BillHList = billHList,
                SalesTotal = GetTotal('P', autoNumber.BizDte, posId),
                ReturnTotal = GetTotal('R', autoNumber.BizDte, posId),
                VoidTotal = GetTotal('V', autoNumber.BizDte, posId)
            };

            return View(home);
        }

        #endregion


        #region // Search methods //

        [HttpPost]
        public async Task<IActionResult> Search(string fromDate, string toDate, string shiftNo)
        {
            SetLayOutData();

            var userCde = HttpContext.User.Claims.FirstOrDefault()?.Value;

            var dbUser = await _dbContext.ms_user.FirstOrDefaultAsync(user => user.UserCde == userCde);

            var posId = await _dbContext.ms_userpos
                .Where(userPOS => userPOS.UserId == dbUser.UserId)
                .Select(userPOS => userPOS.POSid)
                .FirstOrDefaultAsync();

            var autoNumber = await _dbContext.ms_autonumber.FirstOrDefaultAsync(pos => pos.PosId == posId);

            autoNumber.BizDteString = ChangeDateFormat(autoNumber.BizDte);

            var query = _dbContext.billh.AsQueryable(); // Start with the base query

            if (!shiftNo.IsNullOrEmpty() && int.TryParse(shiftNo, out var shiftNumber))
            {
                query = query.Where(h => h.ShiftNo == shiftNumber);
            }

            if (!fromDate.IsNullOrEmpty() && DateTime.TryParse(fromDate, out var fromDateValue))
            {
                query = query.Where(h => h.BizDte.Date >= fromDateValue.Date);
            }

            if (!toDate.IsNullOrEmpty() && DateTime.TryParse(toDate, out var toDateValue))
            {
                query = query.Where(h => h.BizDte.Date <= toDateValue.Date);
            }

            if (fromDate.IsNullOrEmpty() && toDate.IsNullOrEmpty())
            {
                query = query.Where(h => h.BizDte.Date == autoNumber.BizDte.Date);
            }

            var billHList = await query
                .Where(billH => billH.Status == 'P' && billH.POSId == posId)
                .Join(_dbContext.billp,
                billH => billH.BillhId,
                billP => billP.BillhID,
                (billH, billP) => new Billh
                {
                    BillhId = billH.BillhId,
                    BizDte = billH.BizDte,
                    BillNo = billH.BillNo,
                    LocCde = billH.LocCde,
                    ShiftNo = billH.ShiftNo,
                    GuestNme = billH.GuestNme,
                    BillDiscount = billH.BillDiscount,
                    Remark = billH.Remark,
                    Status = billH.Status,
                    POSId = billH.POSId,
                    UserId = billH.UserId,
                    SaleType = billP.CurrTyp
                })
                .ToListAsync();


            foreach (var billH in billHList)
            {
                var sale = await _dbContext.billp.FirstOrDefaultAsync(billp => billp.BillhID == billH.BillhId && billp.CurrTyp == billH.SaleType);
                if (sale != null)
                {
                    billH.TotalAmount = sale.LocalAmt - sale.ChangeAmt;
                }
                billH.Action = "Sales";
                billH.User = userCde;
            }

            var home = new Home()
            {
                AutoNumber = autoNumber,
                BillHList = billHList
            };

            return PartialView("_HomePartial", home);
        }

        #endregion


        #region // Edit methods //

        [HttpPost]
        public IActionResult Edit(Home home)
        {
            if (home.BillH.VoidFlg) // check bill is void or not
            {
                VoidBill(home);
                return RedirectToAction("Index");
            }

            var editedBillH = home.BillH;
            var dbBillH = _dbContext.billh.FirstOrDefault(h => h.BillNo == editedBillH.BillNo);

            if (dbBillH != null)
            {
                dbBillH.LocCde = editedBillH.LocCde;
                dbBillH.Remark = editedBillH.Remark;

                _dbContext.billh.Update(dbBillH);
            }

            _dbContext.SaveChanges();
            return RedirectToAction("Index");

        }

        public void VoidBill(Home home)
        {
            var billH = home.BillH;

            var dbBillH = _dbContext.billh.FirstOrDefault(h => h.BillNo == billH.BillNo);

            if (dbBillH != null) // void billH
            {
                dbBillH.LocCde = billH.LocCde;
                dbBillH.Remark = billH.Remark;
                dbBillH.Status = 'V';
                dbBillH.RevDteTime = DateTime.Now;

                _dbContext.billh.Update(dbBillH);

                var dbBillDList = _dbContext.billd
                    .Where(bill => bill.BillhId == dbBillH.BillhId)
                    .ToList(); // void billD

                foreach (var dbBillD in dbBillDList)
                {
                    dbBillD.VoidFlg = true;
                    dbBillD.VoidDteTime = DateTime.Now;
                    _dbContext.billd.Update(dbBillD);
                }
            }

            _dbContext.SaveChanges();
        }

        public IActionResult GetBillDList(int billHId)
        {
            var billDList = _dbContext.billd.Where(d => d.BillhId == billHId).ToList();
            var count = 1;

            foreach (var billD in billDList)
            {
                billD.No = count;
                count++;
            }

            var billH = _dbContext.billh.FirstOrDefault(h => h.BillhId == billHId);

            var locations = _dbContext.ms_location.ToList();

            var saleTypes = _dbContext.ms_currency.ToList();

            var home = new Home()
            {
                BillDList = billDList,
                BillH = billH,
                Locations = locations
            };

            return PartialView("_BillDPartial", home);
        }

        #endregion


        #region // Other spin-off methods //

        public async Task<IActionResult> ShiftEnd(string posID)
        {
            var pos = await _dbContext.ms_autonumber.FirstOrDefaultAsync(autoNumber => autoNumber.PosId == posID);
            if (pos != null)
            {
                pos.CurShift += 1;

                _dbContext.Update(pos);
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DayEnd(string posID)
        {
            var pos = await _dbContext.ms_autonumber.FirstOrDefaultAsync(autoNumber => autoNumber.PosId == posID);
            if (pos != null)
            {
                pos.CurShift = 1;
                DateTime UpdatedDate = pos.BizDte.AddDays(1);
                pos.BizDte = UpdatedDate;
                _dbContext.Update(pos);
                await _dbContext.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public decimal GetTotal(char status, DateTime bizDte, string posId)
        {
            var total = _dbContext.billh
                .Where(h => h.Status == status && h.BizDte.Date == bizDte.Date && h.POSId == posId)
                .SelectMany(h => _dbContext.billp.Where(p => p.BillhID == h.BillhId))
                .Sum(p => p.LocalAmt - p.ChangeAmt);

            return total;
        }


        #endregion


        #region // Print methods //

        public async Task<IActionResult> BillPrint(string billNo)
        {
            var billH = await _dbContext.billh.Where(b => b.BillNo == billNo).FirstOrDefaultAsync();

            if (billH == null)
            {
                return NotFound();
            }

            var userNme = await _dbContext.ms_user.Where(u => u.UserId == billH.UserId).Select(u => u.UserNme).FirstOrDefaultAsync();
            var currTyp = await _dbContext.billp.Where(b => b.BillhID == billH.BillhId).Select(b => b.CurrTyp).FirstOrDefaultAsync();

            var billHList = await _dbContext.billh
                .Where(bill => bill.BillNo == billNo)
                .Select(billD => new BillReport
                {
                    posid = billD.POSId,
                    billno = billD.BillNo,
                    userid = userNme,
                    bizdte = billD.BizDte.ToString("dd-MM-yyyy"),
                    shiftno = billD.ShiftNo,
                    billtypcde = currTyp,
                    billdiscount = billD.BillDiscount,
                    remark = billD.Remark
                })
                .ToListAsync();


            // change to match with rdlc dataset
            var detailsList = await _dbContext.billd
                .Where(bill => bill.BillhId == billH.BillhId)
                .Select(billD => new BillReport
                {
                    itemid = billD.ItemID,
                    qty = billD.Qty,
                    discamt = billD.DiscAmt,
                    price = billD.Price
                })
                .ToListAsync();

            foreach (var detail in detailsList)
            {
                detail.itemid = GetItemDescByItemId(detail.itemid);
            }

            try
            {
                var report = new Microsoft.Reporting.NETCore.LocalReport();
                var path = $"{this._webHostEnvironment.WebRootPath}\\Report\\BillPrint.rdlc";

                report.ReportPath = path;
                report.DataSources.Add(new ReportDataSource("BillDetails", detailsList));
                report.DataSources.Add(new ReportDataSource("BillH", billHList));

                byte[] htmlBytes = report.Render("HTML4.0");

                return File(htmlBytes, "text/html");
            }
            catch
            {
                return BadRequest("An error occurred while generating the report. Please try again later.");
            }
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

        private static string ChangeDateFormat(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);
            return dateOnly.ToString("dd-MM-yyyy");
        }

        private string GetItemDescByItemId(string itemId)
        {
            var itemDesc = _dbContext.ms_stock
                .Where(stk => stk.ItemId == itemId)
                .Select(stk => stk.ItemDesc)
                .FirstOrDefault();

            return itemDesc ?? "";
        }

        #endregion


        #region // Unnecessary methods //

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #endregion
    }
}