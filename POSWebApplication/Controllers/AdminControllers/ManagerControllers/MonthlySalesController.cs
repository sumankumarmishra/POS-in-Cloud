using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;


namespace POSWebApplication.Controllers.AdminControllers.ManagerControllers
{
    [Authorize]
    public class MonthlySalesController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public MonthlySalesController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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
            ViewData["Categories"] = new SelectList(_dbContext.ms_stockcategory.ToList(), "CategoryId", "CategoryId");

            return View(new MonthlySalesSearch());
        }

        [HttpPost]
        public IActionResult View(short cmpyId, string catgCde, int year)
        {
            var monthlySalesAction = 0;

            if (!catgCde.IsNullOrEmpty())
            {
                monthlySalesAction = 1;
            }

            var mainList = new List<MonthlySalesDataModel>();

            var monthlySales = _dbContext.spMonthlySales01DbSet
                    .FromSqlRaw("EXEC sp_monthlySales @action={0}, @year = {1},@catgcde = {2}", monthlySalesAction, year, catgCde)
                    .AsEnumerable()
                    .ToList();

            if (cmpyId != 0) // if branch name is selected
            {
                var branchName = _dbContext.ms_company.Where(c => c.CmpyId == cmpyId).Select(c => c.CmpyNme).FirstOrDefault();

                var newMonthlySales = monthlySales.Where(list => list.CmpyId == cmpyId).ToList();

                var totalAmt = newMonthlySales.Sum(list => list.Amount);

                foreach (var item in newMonthlySales)
                {
                    item.No = monthlySales.IndexOf(item) + 1;
                    item.Month = GetMonth(item.MonthNo ?? 0);
                    item.ProgressBar = GetProgressBar(item.Amount ?? 0, totalAmt ?? 0);
                }

                var headModel = new MonthlySalesDataModel()
                {
                    MonthlySales = newMonthlySales,
                    Branch = branchName,
                    TotalAmount = totalAmt
                };

                mainList.Add(headModel);
            }
            else // select all branches
            {
                var allBranches = _dbContext.ms_company.Select(cmpy => cmpy.CmpyNme).ToList();

                foreach (var branch in allBranches)
                {
                    var newMonthlySales = monthlySales.Where(x => x.Branch == branch).ToList();
                    var totalAmt = newMonthlySales.Sum(list => list.Amount);

                    foreach (var item in newMonthlySales)
                    {
                        item.No = monthlySales.IndexOf(item) + 1;
                        item.Month = GetMonth(item.MonthNo ?? 0);
                        item.ProgressBar = GetProgressBar(item.Amount ?? 0, totalAmt ?? 0);
                    }

                    var headModel = new MonthlySalesDataModel()
                    {
                        MonthlySales = newMonthlySales,
                        Branch = branch,
                        TotalAmount = totalAmt
                    };
                    mainList.Add(headModel);
                }
            }

            return PartialView("_MonthlySalesViewPartial", mainList);
        }

        public IActionResult GetDonutChartData(short cmpyId, string catgCde, int year)
        {
            var monthlySalesAction = 0;

            if (!catgCde.IsNullOrEmpty()) // for specific category only
            {
                monthlySalesAction = 1;
            }

            if (cmpyId == 0) // for all branches
            {

                var allBranches = _dbContext.ms_company.Select(cmpy => cmpy.CmpyNme).ToList();

                // Add chartData labels
                var newLabels = new List<string>();
                newLabels.AddRange(allBranches);

                // Add chartData datasets data
                var monthlySales = _dbContext.spMonthlySales01DbSet
                    .FromSqlRaw("EXEC sp_monthlySales @action={0}, @year = {1},@catgcde = {2}", monthlySalesAction, year, catgCde)
                    .AsEnumerable()
                    .ToList();

                var newData = new List<int>();

                foreach (var branch in allBranches)
                {
                    var totalAmt = (int)(monthlySales.Where(x => x.Branch == branch).Sum(x => x.Amount) ?? 0);
                    newData.Add(totalAmt);
                }

                var chartData = new
                {
                    labels = newLabels.ToArray(),
                    datasets = new[]
                        {
                        new
                        {
                            data = newData.ToArray(),
                            backgroundColor = new[] { "#f56954", "#00a65a" ,"#f39c12", "#00c0ef", "#3c8dbc", "#d2d6de","#9b59b6","#3498db","#e74c3c","#2ecc71","#1abc9c","#34495e" }
                        }
                    }
                };
                return Json(chartData);
            }
            else // for specific branch only
            {
                var branchName = _dbContext.ms_company.Where(c => c.CmpyId == cmpyId).Select(c => c.CmpyNme).FirstOrDefault();
                // Add chartData labels
                var newLabel = new List<string> { branchName ?? "Default" };

                // Add chartData datasets data
                var monthlySales = _dbContext.spMonthlySales01DbSet
                    .FromSqlRaw("EXEC sp_monthlySales @action={0}, @year = {1},@catgcde = {2}", monthlySalesAction, year, catgCde)
                    .AsEnumerable()
                    .Select(x => new MonthlySalesModel
                    {
                        Branch = x.Branch,
                        Amount = x.Amount
                    })
                    .Where(x => x.Branch == branchName)
                    .ToList();

                var totalAmt = (int)(monthlySales.Sum(x => x.Amount) ?? 0);
                var newData = new List<int> { totalAmt };

                var chartData = new
                {
                    labels = newLabel.ToArray(),
                    datasets = new[]
                        {
                        new
                        {
                            data = newData.ToArray(),
                            backgroundColor = new[] { "#f56954", "#00a65a" ,"#f39c12", "#00c0ef", "#3c8dbc", "#d2d6de","#9b59b6","#3498db","#e74c3c","#2ecc71","#1abc9c","#34495e" }
                        }
                    }
                };
                return Json(chartData);
            }
        }

        public IActionResult GetBarChartData(short cmpyId, string catgCde, int year)
        {
            // both null or empty
            var catgSalesAction = 2;
            var categories = _dbContext.ms_stockcategory.Select(c => c.CategoryId).ToList();

            if (cmpyId != 0 && catgCde.IsNullOrEmpty())
            {
                catgSalesAction = 3;

                var list = _dbContext.billh
                    .Where(h => h.CmpyId == cmpyId)
                    .Join(_dbContext.billd,
                    h => h.BillhId,
                    d => d.BillhId,
                    (h, d) => d.ItemID)
                    .Join(_dbContext.ms_stock,
                    x => x,
                    s => s.ItemId,
                    (x, s) => s.CatgCde)
                    .Distinct()
                    .ToList();
            }

            if (cmpyId == 0 && !catgCde.IsNullOrEmpty())
            {
                catgSalesAction = 4;
                categories = new List<string>()
                {
                    catgCde
                };
            }

            if (cmpyId != 0 && !catgCde.IsNullOrEmpty())
            {
                catgSalesAction = 5;
                categories = new List<string>()
                {
                    catgCde
                };
            }


            // Add chartData labels
            var newLabels = new List<string>();
            newLabels.AddRange(categories);

            // Add chartData datasets data
            var catgWiseSales = _dbContext.spMonthlySales02DbSet
                .FromSqlRaw("EXEC sp_monthlySales @action={0}, @year = {1},@catgcde = {2},@cmpyid = {3}", catgSalesAction, year, catgCde, cmpyId)
                .AsEnumerable()
                .ToList();

            var newData = new List<int>();

            foreach (var catg in catgWiseSales)
            {
                var totalAmt = (int)(catg.SaleAmount ?? 0);
                newData.Add(totalAmt);
            }

            var chartData = new
            {
                labels = newLabels.ToArray(),
                datasets = new[]
                    {
                        new
                        {
                            axis = "y",
                            label = "Categories",
                            backgroundColor = "rgba(60,141,188,0.9)",
                            borderColor = "rgba(60,141,188,0.8)",
                            pointRadius =  false,
                            pointColor = "#3b8bba",
                            pointStrokeColor = "rgba(60,141,188,1)",
                            pointHighlightFill = "#fff",
                            pointHighlightStroke = "rgba(60,141,188,1)",
                            data = newData.ToArray()
                        }
                    }
            };
            return Json(chartData);
        }

        #endregion


        #region // Other methods //

        protected static string GetMonth(int monthNo)
        {
            string[] months = { "January", "February", "March", "April", "May", "June", "July", "Auguest", "September", "October", "November", "December" };

            if (monthNo >= 1 && monthNo <= 12)
            {
                return months[monthNo - 1];
            }

            // Handle invalid month numbers
            return "InvalidMonth";
        }

        protected static int? GetProgressBar(decimal amount, decimal totalAmt)
        {
            var progessBar = (int)(amount / totalAmt * 100);
            return progessBar;
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
