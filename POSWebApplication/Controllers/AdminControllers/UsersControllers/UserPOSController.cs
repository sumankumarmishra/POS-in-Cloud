using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.UsersControllers
{
    [Authorize]
    public class UserPOSController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;
        public UserPOSController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            _dbContext = new POSWebAppDbContext(optionsBuilder.Options);
        }


        #region // Main methods //

        public IActionResult Index()
        {
            SetLayOutData();

            try
            {
                var resultList = _dbContext.ms_userpos
                            .Join(_dbContext.ms_user,
                                t1 => t1.UserId,
                                t2 => t2.UserId,
                                (t1, t2) => new UserPOS
                                {
                                    POSid = t1.POSid,
                                    UserCode = t2.UserCde,
                                    UserPOSid = t1.UserPOSid
                                });

                return View(resultList);
            }
            catch (Exception ex)
            {
                ViewBag.AlertMessage = ex.Message;
                return RedirectToAction("Index", "Home");
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

                var POS = _dbContext.ms_userpos.FirstOrDefault(pos => pos.UserId == user.UserId);

                var bizDte = _dbContext.ms_autonumber
                    .Where(auto => auto.PosId == POS.POSid)
                    .Select(auto => auto.BizDte)
                    .FirstOrDefault();

                ViewData["Business Date"] = bizDte.ToString("dd-MM-yyyy");
                ViewData["Database"] = _databaseSettings.DbName;
            }
        }

        #endregion
    }
}
