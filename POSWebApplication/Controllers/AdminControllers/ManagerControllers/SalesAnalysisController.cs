using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSWebApplication.Models;
using POSinCloud.Services;

using POSWebApplication.Data;

namespace POSWebApplication.Controllers.AdminControllers.ManagerControllers
{
    [Authorize]
    public class SalesAnalysisController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public SalesAnalysisController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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

            return View(new SalesAnalysisSearch());
        }

        [HttpPost]
        public IActionResult Search(short cmpyId, DateTime fromDate, int days, string mode, int topBottom)
        {
            var saleAnalysisAction = 0;

            if (cmpyId == 0 && mode == "Top")
            {
                saleAnalysisAction = 0;
            }
            if (cmpyId != 0 && mode == "Top")
            {
                saleAnalysisAction = 1;
            }
            if (cmpyId == 0 && mode == "Bottom")
            {
                saleAnalysisAction = 2;
            }
            if (cmpyId != 0 && mode == "Bottom")
            {
                saleAnalysisAction = 3;
            }

            var saleAnalysisList = _dbContext.spSalesAnalysisDbSet
                    .FromSqlRaw("EXEC sp_saleAnalysis @action={0},@bizdte = {1},@days = {2},@cmpyId = {3} ", saleAnalysisAction, fromDate.Date, days, cmpyId)
                    .AsEnumerable()
                    .Take(topBottom)
                    .ToList();

            foreach (var item in saleAnalysisList)
            {
                item.No = saleAnalysisList.IndexOf(item) + 1;
            }
            return PartialView("_SaleAnalysisSearchPartial", saleAnalysisList);
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
