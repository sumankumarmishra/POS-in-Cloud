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
    public class YearlySalesController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public YearlySalesController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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

            return View(new YearlySalesSearch());
        }

        [HttpPost]
        public IActionResult View(short cmpyId, int year, int exYear)
        {
            var yearlySalesAction = 0;

            var mainList = new List<YearlySalesDataModel>();

            var yearlySales = _dbContext.spYearlySales01DbSet
                    .FromSqlRaw("EXEC sp_yearlySales @action={0}, @year = {1},@exyear = {2}", yearlySalesAction, year, exYear)
                    .AsEnumerable()
                    .ToList();

            if (cmpyId != 0) // if branchname is selected
            {
                var branchName = _dbContext.ms_company.Where(c => c.CmpyId == cmpyId).Select(c => c.CmpyNme).FirstOrDefault();

                var newYearlySales = yearlySales.Where(list => list.CmpyId == cmpyId).ToList();

                var totalAmt = newYearlySales.Sum(list => list.Amount);

                foreach (var item in newYearlySales)
                {
                    item.No = newYearlySales.IndexOf(item) + 1;
                    item.ProgressBar = GetProgressBar(item.Amount ?? 0, totalAmt ?? 0);
                }

                var headModel = new YearlySalesDataModel()
                {
                    YearlySales = newYearlySales,
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
                    var newYearlySales = yearlySales.Where(x => x.Branch == branch).ToList();
                    var totalAmt = newYearlySales.Sum(list => list.Amount);

                    foreach (var item in newYearlySales)
                    {
                        item.No = newYearlySales.IndexOf(item) + 1;
                        item.ProgressBar = GetProgressBar(item.Amount ?? 0, totalAmt ?? 0);
                    }

                    var headModel = new YearlySalesDataModel()
                    {
                        YearlySales = newYearlySales,
                        Branch = branch,
                        TotalAmount = totalAmt
                    };
                    mainList.Add(headModel);
                }
            }

            return PartialView("_YearlySalesViewPartial", mainList);
        }

        public IActionResult GetDonutChartData(short cmpyId, int year, int exYear)
        {
            var yearlySalesAction = 0;

            if (cmpyId == 0) // for all branches
            {

                var allBranches = _dbContext.ms_company.Select(cmpy => cmpy.CmpyNme).ToList();

                // Add chartData labels
                var newLabels = new List<string>();
                newLabels.AddRange(allBranches);

                // Add chartData datasets data
                var yearlySales = _dbContext.spYearlySales01DbSet
                    .FromSqlRaw("EXEC sp_yearlySales @action={0}, @year = {1},@exyear = {2}", yearlySalesAction, year, exYear)
                    .AsEnumerable()
                    .ToList();

                var newData = new List<int>();

                foreach (var branch in allBranches)
                {
                    var totalAmt = (int)(yearlySales.Where(x => x.Branch == branch).Sum(x => x.Amount) ?? 0);
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
                var yearlySales = _dbContext.spYearlySales01DbSet
                    .FromSqlRaw("EXEC sp_yearlySales @action={0}, @year = {1},@exyear = {2}", yearlySalesAction, year, exYear)
                    .AsEnumerable()
                    .Where(x => x.Branch == branchName)
                    .ToList();

                var totalAmt = (int)(yearlySales.Sum(x => x.Amount) ?? 0);
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

        public IActionResult GetBarChartData(short cmpyId, int year, int exYear)
        {
            var catgSalesAction = 1; // for all branches
            var categories = _dbContext.ms_stockcategory.Select(s => s.CategoryId).ToList();

            if (cmpyId != 0) // for specific branch
            {
                catgSalesAction = 2;
            }

            // Add chartData labels
            var newLabels = new List<string>();
            newLabels.AddRange(categories);

            // Add chartData datasets data
            var catgWiseSales = _dbContext.spYearlySales02DbSet
                .FromSqlRaw("EXEC sp_yearlySales @action={0}, @year = {1},@exyear = {2},@cmpyId = {3}", catgSalesAction, year, exYear, cmpyId)
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
