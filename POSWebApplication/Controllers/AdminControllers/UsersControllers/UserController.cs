using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using POSinCloud.Services;
using POSWebApplication.Data;
using POSWebApplication.Models;

namespace POSWebApplication.Controllers.AdminControllers.UsersControllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly POSWebAppDbContext _dbContext;

        public UserController(DatabaseServices dbServices, IHttpContextAccessor accessor)
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

                foreach (var user in usersList)
                {
                    user.MnuGrp = _dbContext.ms_usermenugrp.Where(gp => gp.MnuGrpId == user.MnuGrpId).Select(gp => gp.MnuGrpNme).FirstOrDefault() ?? "";
                }

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
                var posList = _dbContext.ms_autonumber.Where(an => an.PosDefLoc != null && an.PosDefLoc != "").Select(pos => pos.PosId).ToList();

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
                    user.Pwd ??= "User@123";
                    user.CmpyId = await _dbContext.ms_autonumber
                        .Where(pos => pos.PosId == user.POSid)
                        .Select(pos => pos.CmpyId)
                        .FirstOrDefaultAsync();
                    user.CreateDtetime = DateTime.Now;
                    await _dbContext.ms_user.AddAsync(user);
                    await _dbContext.SaveChangesAsync();

                    if (user.POSid != null)
                    {
                        var userPos = new UserPOS
                        {
                            UserId = user.UserId,
                            POSid = user.POSid
                        };

                        await _dbContext.ms_userpos.AddAsync(userPos);
                    }

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

        public IActionResult ChangePassword(int id)
        {
            SetLayOutData();

            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            var customUser = new CustomUser()
            {
                UserId = user.UserId,
                UserCde = user.UserCde,
                UserNme = user.UserNme,
                MnuGrp = _dbContext.ms_usermenugrp.Where(gp => gp.MnuGrpId == user.MnuGrpId).Select(gp => gp.MnuGrpNme).FirstOrDefault() ?? ""
            };

            return View(customUser);
        }

        [HttpPost]
        public IActionResult ChangePassword(CustomUser customUser)
        {
            SetLayOutData();

            var user = _dbContext.ms_user.FirstOrDefault(u => u.UserId == customUser.UserId);

            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (customUser.Pwd == user.Pwd)
                {
                    if (customUser.NewPwd == customUser.ConfirmPwd)
                    {
                        user.Pwd = customUser.NewPwd;
                        _dbContext.ms_user.Update(user);
                        _dbContext.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        @ViewBag.AlertMessage = "The new password and confirmation password do not match.";
                    }
                }
                else
                {
                    @ViewBag.AlertMessage = "The password does not match.";
                }

            }
            return View(customUser);
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
                if (dbUser != null)
                {
                    dbUser.UserCde = user.UserCde;
                    dbUser.UserNme = user.UserNme;
                    dbUser.MnuGrpId = user.MnuGrpId;
                    dbUser.CmpyId = await _dbContext.ms_autonumber
                        .Where(pos => pos.PosId == user.POSid)
                        .Select(pos => pos.CmpyId)
                        .FirstOrDefaultAsync();

                    _dbContext.ms_user.Update(dbUser);

                    var userPos = await _dbContext.ms_userpos.FirstOrDefaultAsync(u => u.UserId == user.UserId);

                    if (userPos != null)
                    {
                        if (user.POSid != null)
                        {
                            userPos.POSid = user.POSid;
                            _dbContext.ms_userpos.Update(userPos);
                        }
                        else
                        {
                            _dbContext.ms_userpos.Remove(userPos);
                        }
                    }
                    else
                    {
                        if (user.POSid != null)
                        {
                            var newUserPOS = new UserPOS
                            {
                                UserId = user.UserId,
                                POSid = user.POSid
                            };

                            await _dbContext.ms_userpos.AddAsync(newUserPOS);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    TempData["info message"] = "User is successfully updated!";
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
