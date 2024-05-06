using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace POSWebApplication.Controllers.AdminControllers.UsersControllers
{
    [Authorize]
    public class UserPOSController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;
        public UserPOSController(POSWebAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            SetLayOutData();

            try
            {
                /*IEnumerable<UserPOS> resultList = from t1 in _dbContext.ms_userpos
                             join t2 in _dbContext.ms_user on t1.UserId equals t2.UserId
                             select new UserPOS
                             {
                                 POSid = t1.POSid,
                                 UserCode = t2.UserCde,
                                 UserPOSid = t1.UserPOSid
                             };*/

                IEnumerable<UserPOS> resultList = _dbContext.ms_userpos
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
    }
}
