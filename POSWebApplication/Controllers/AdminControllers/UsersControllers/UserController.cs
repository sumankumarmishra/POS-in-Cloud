using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.UsersControllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly DatabaseSettings _databaseSettings;
        private readonly POSWebAppDbContext _dbContext;

        public UserController(DatabaseSettings databaseSettings)
        {
            _databaseSettings = databaseSettings;
            var optionsBuilder = new DbContextOptionsBuilder<POSWebAppDbContext>().UseSqlServer(_databaseSettings.ConnectionString);
            _dbContext = new POSWebAppDbContext(optionsBuilder.Options);
        }


        #region // Main methods //

        public async Task<IActionResult> Index()
        {
            SetLayOutData();

            if (TempData["info message"] != null)
            {
                ViewBag.InfoMessage = TempData["info message"];
            }
            if (TempData["warning message"] != null)
            {
                ViewBag.WarningMessage = TempData["warning message"];
            }

            try
            {
                var usersList = await _dbContext.ms_user.ToListAsync();
                return View(usersList);
            }
            catch (Exception ex)
            {
                TempData["alert message"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult Create()
        {
            SetLayOutData();

            if (ViewData["User Role"]?.ToString() == "accessLvl2")
            {
                var menuGroupList = _dbContext.ms_usermenugrp.ToList();
                var posList = _dbContext.ms_autonumber.Select(pos => pos.PosId).ToList();

                var userModelList = new UserModelList
                {
                    User = new User(),
                    UserMenuGroupList = menuGroupList,
                    POSList = posList
                };

                return View(userModelList);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                if (user.Pwd == user.ConfirmPwd)
                {
                    short? cmpyId = await _dbContext.ms_autonumber
                        .Where(pos => pos.PosId == user.POSid)
                        .Select(pos => pos.CmpyId)
                        .FirstOrDefaultAsync();

                    if (cmpyId != null)
                    {
                        user.CmpyId = cmpyId.Value;
                    }

                    await _dbContext.ms_user.AddAsync(user);
                    await _dbContext.SaveChangesAsync();


                    var userPos = new UserPOS
                    {
                        UserId = user.UserId,
                        POSid = user.POSid
                    };

                    await _dbContext.ms_userpos.AddAsync(userPos);
                    await _dbContext.SaveChangesAsync();
                    TempData["info message"] = "User is successfully created!";
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.AlertMessage = "Password and confirm password are not the same";

            }
            else
            {
                ViewBag.AlertMessage = "Required fields must be filled";
            }

            var posList = await _dbContext.ms_autonumber.Select(pos => pos.PosId).ToListAsync();

            var userModelList = new UserModelList
            {
                User = user,
                UserMenuGroupList = await _dbContext.ms_usermenugrp.ToListAsync(),
                POSList = posList
            };

            return View(userModelList);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            SetLayOutData();

            if (ViewData["User Role"]?.ToString() == "accessLvl2")
            {
                var user = await _dbContext.ms_user.FirstOrDefaultAsync(u => u.UserId == id);

                if (user != null)
                {
                    var posId = _dbContext.ms_userpos.FirstOrDefault(u => u.UserId == user.UserId)?.POSid;
                    user.POSid = posId ?? user.POSid;
                }

                var posList = _dbContext.ms_autonumber.Select(pos => pos.PosId).ToList();

                var userModelList = new UserModelList
                {
                    User = user ?? new User(),
                    UserMenuGroupList = await _dbContext.ms_usermenugrp.ToListAsync(),
                    POSList = posList
                };

                return View(userModelList);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            SetLayOutData();

            if (ModelState.IsValid)
            {
                var dbUser = await _dbContext.ms_user.FindAsync(user.UserId);
                if (user.Pwd == user.ConfirmPwd)
                {

                    if (dbUser != null)
                    {
                        dbUser.UserCde = user.UserCde;
                        dbUser.UserNme = user.UserNme;
                        dbUser.Pwd = user.Pwd;
                        dbUser.MnuGrpId = user.MnuGrpId;
                        dbUser.CreateDtetime = user.CreateDtetime;
                        dbUser.CmpyId = user.CmpyId;

                        short? cmpyId = await _dbContext.ms_autonumber.Where(pos => pos.PosId == user.POSid)
                            .Select(pos => pos.CmpyId)
                            .FirstOrDefaultAsync();

                        if (cmpyId != null)
                        {
                            dbUser.CmpyId = cmpyId.Value;
                        }

                        _dbContext.ms_user.Update(dbUser);

                        var userPos = await _dbContext.ms_userpos.FirstOrDefaultAsync(u => u.UserId == user.UserId);

                        if (userPos != null)
                        {
                            userPos.POSid = user.POSid;
                            _dbContext.ms_userpos.Update(userPos);
                        }

                        await _dbContext.SaveChangesAsync();
                        TempData["info message"] = "User is successfully updated!";
                        return RedirectToAction(nameof(Index));
                    }
                }

                ViewBag.AlertMessage = "Password and confirm password are not the same";
            }
            else
            {
                ViewBag.AlertMessage = "Required fields must be filled";
            }

            var posList = await _dbContext.ms_autonumber.Select(pos => pos.PosId).ToListAsync();

            var userModelList = new UserModelList
            {
                User = user,
                UserMenuGroupList = await _dbContext.ms_usermenugrp.ToListAsync(),
                POSList = posList
            };

            return View(userModelList);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            SetLayOutData();

            if (ViewData["User Role"]?.ToString() == "accessLvl2")
            {
                var user = await _dbContext.ms_user.FirstOrDefaultAsync(u => u.UserId == id);

                if (user != null)
                {
                    var posId = _dbContext.ms_userpos.FirstOrDefault(u => u.UserId == user.UserId)?.POSid;
                    user.POSid = posId ?? user.POSid;
                }

                var userModelList = new UserModelList
                {
                    User = user ?? new User(),
                    UserMenuGroupList = await _dbContext.ms_usermenugrp.ToListAsync()
                };

                return View(userModelList);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(User user)
        {
            SetLayOutData();

            var dbUser = _dbContext.ms_user.FirstOrDefault(u => u.UserId == user.UserId);
            var posUser = _dbContext.ms_userpos.FirstOrDefault(u => u.UserId == user.UserId);

            if (posUser != null && dbUser != null)
            {
                _dbContext.ms_userpos.Remove(posUser);
                _dbContext.ms_user.Remove(dbUser);
            }

            await _dbContext.SaveChangesAsync();
            TempData["info message"] = "User is successfully deleted!";
            return RedirectToAction(nameof(Index));
        }

        #endregion


        #region // JS methods //
        public async Task<IActionResult> AddUserPartial()
        {
            var userMenuGroupList = await _dbContext.ms_usermenugrp.ToListAsync();

            var userModelList = new UserModelList()
            {
                UserMenuGroupList = userMenuGroupList
            };

            return PartialView("_AddUserPartial", userModelList);
        }

        public async Task<IActionResult> EditUserPartial(short userId)
        {
            var user = await _dbContext.ms_user.FindAsync(userId);
            var userMenuGroupList = await _dbContext.ms_usermenugrp.ToListAsync();

            var userModelList = new UserModelList
            {
                UserMenuGroupList = userMenuGroupList,
                User = user

            };

            return PartialView("_EditUserPartial", userModelList);
        }

        public async Task<IActionResult> DeleteUserPartial(short userId)
        {
            var user = await _dbContext.ms_user.FindAsync(userId);

            var userModelList = new UserModelList
            {
                User = user
            };

            return PartialView("_DeleteUserPartial", userModelList);
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
