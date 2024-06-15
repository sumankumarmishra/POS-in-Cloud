using Microsoft.AspNetCore.Mvc;
using POSWebApplication.Models;
using POSWebApplication.Data;
using POSinCloud.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace POSWebApplication.Controllers.AdminControllers.ManagerControllers
{
    public class SaleDashboardController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public SaleDashboardController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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

        public IActionResult Index()
        {
            SetLayOutData();

            ViewData["BranchNames"] = new SelectList(_dbContext.ms_company.ToList(), "CmpyId", "CmpyNme");

            return View(new SaleDashboardDataModel());
        }

        [HttpPost]
        public IActionResult Search(short cmpyId, DateTime fromDate, DateTime toDate)
        {
            SetLayOutData();

            var cmpygrpnme = _dbContext.ms_company.Where(cmpy => cmpy.CmpyId == cmpyId).Select(cmpy => cmpy.CmpyNme).FirstOrDefault();
            var saleDashboardAction = 0;
            var currDashboardAction = 1;


            var saleDashboardSearchModel = new SaleDashboardSearchModel
            {
                CmpyId = cmpyId,
                FromDate = fromDate,
                ToDate = toDate,
            };

            // SaleDashboard List

            var saleDashboardList = _dbContext.spSaleDashboard00DbSet
                        .FromSqlRaw("EXEC sp_saledashboard @action={0}, @fromdate = {1}, @todate = {2}", saleDashboardAction, fromDate, toDate)
                        .AsEnumerable()
                        .ToList();

            if (cmpyId != 0) // if branch name is selected
            {
                saleDashboardList = saleDashboardList.Where(list => list.CmpyId == cmpyId).ToList();
                currDashboardAction = 2;
            }

            foreach (var item in saleDashboardList)
            {
                item.No = saleDashboardList.IndexOf(item) + 1;
            }

            // CurrencySaleDashboardList

            var saleCurrList = _dbContext.spSaleDashboard0102DbSet
                        .FromSqlRaw("exec sp_saledashboard @action={0},@cmpyid={1}, @fromdate = {2}, @todate = {3}", currDashboardAction, cmpyId, fromDate, toDate)
                        .AsEnumerable()
                        .ToList();

            foreach (var curr in saleCurrList)
            {
                curr.No = saleCurrList.IndexOf(curr) + 1;
            }

            var saleDashboardDataList = new SaleDashboardDataModel()
            {
                SaleDashboardSearch = saleDashboardSearchModel,
                SaleDashboardList = saleDashboardList,
                SaleCurrList = saleCurrList,
                TotalAmt = saleDashboardList.Sum(list => list.SaleAmount)
            };

            return PartialView("_SaleDashboardSearchPartial", saleDashboardDataList);
        }

        public IActionResult DateWiseSales(short cmpyId, string name, DateTime fromDate, DateTime toDate)
        {
            SetLayOutData();

            var dateWiseSaleAction = 3;

            var dateWiseSaleList = _dbContext.spSaleDashboard03DbSet
                        .FromSqlRaw("EXEC sp_saledashboard @action={0}, @cmpyid = {1}, @fromdate = {2}, @todate = {3}", dateWiseSaleAction, cmpyId, fromDate, toDate)
                        .AsEnumerable()
                        .ToList();

            foreach (var item in dateWiseSaleList)
            {
                item.No = dateWiseSaleList.IndexOf(item) + 1;
            }

            ViewData["BranchName"] = name;
            ViewData["CmpyId"] = cmpyId;

            return View(dateWiseSaleList);
        }

        public IActionResult ItemWiseSales(short cmpyId, string name, DateTime date)
        {
            SetLayOutData();

            var itemWiseSaleAction = 4;

            var itemWiseSaleList = _dbContext.spSaleDashboard04DbSet
                        .FromSqlRaw("EXEC sp_saledashboard @action={0}, @cmpyId = {1}, @thisdate = {2}", itemWiseSaleAction, cmpyId, date)
                        .AsEnumerable()
                        .ToList();

            foreach (var item in itemWiseSaleList)
            {
                item.No = itemWiseSaleList.IndexOf(item) + 1;
            }

            ViewData["BranchName"] = name;

            ViewData["Date"] = date.ToString("dd MMM yyyy");

            return View(itemWiseSaleList);
        }

        #endregion


        #region // Global methods (Important)//

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
