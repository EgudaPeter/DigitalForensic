using Digital.Forensic.Common;
using Digital.Forensic.Models.Models;
using Digital.Forensic.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Digital_Forensic.Controllers
{
    public class AuthenticateController : Controller
    {
        static string errorMessage = "Operation failed due to an internal system error. Contact system administrator.";
        static string USERNAME = string.Empty;

        IUserRepo userRepo;
        public AuthenticateController(IUserRepo _userRepo)
        {
            userRepo = _userRepo;
        }


        // GET: Authenticate
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Authenticate", new { Area = "" });
        }

        public ActionResult ActivateAccount()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ActivateAccountViaUsername()
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                if(USERNAME != string.Empty)
                {
                    var result = userRepo.ActivateUserAccount(USERNAME);
                    if(result == true)
                    {
                        return Json(new
                        {
                            success = true
                        });
                    }
                    return Json(new
                    {
                        success = false,
                        infoMessage = "Operation was not successful. Try again later."
                    });
                }
            }
            catch (Exception ex)
            {
                innerEx = ex.InnerException != null ? ex.InnerException.InnerException != null ? $" {ex.InnerException.Message} {ex.InnerException.InnerException.Message}" : $" {ex.InnerException.Message}" : string.Empty;
                exMessage = ex.Message;
            }
            return Json(new
            {
                success = -1,
                errorMessage = $"{errorMessage}<br/> <b>Error Detail</b><br/> Error Message: <b>{exMessage}</b> </br> Inner Exception Message:<b> {innerEx}</b>"
            });
        }

        [HttpPost]
        public JsonResult doLogin(LoginModel Data)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var encryptedPassword = Data.Password != null ? Cryptography.EncryptString(Data.Password) : null;
                if (encryptedPassword == null || Data.Username == null)
                {
                    return Json(new
                    {
                        success = false,
                        infoMessage = "Invalid login credentials!"
                    });
                }
                if (Membership.ValidateUser(Data.Username, encryptedPassword))
                {
                    FormsAuthentication.SetAuthCookie(Data.Username, Data.RememeberMe);
                    //userRepo.LogUserLogin(Data.Username, encryptedPassword, DateTime.Now);
                    var user = userRepo.FindUser(Data.Username);
                    if(user != null)
                    {
                        if(user.UserRole == "AD")
                        {
                            return Json(new { success = "admin" });
                        }
                        if (user.UserRole == "RE")
                        {
                            return Json(new { success = "regular" });
                        }
                    }
                }
                return Json(new
                {
                    success = false,
                    infoMessage = "Invalid login credentials!"
                });
            }
            catch (Exception ex)
            {
                innerEx = ex.InnerException != null ? ex.InnerException.InnerException != null ? $" {ex.InnerException.Message} {ex.InnerException.InnerException.Message}" : $" {ex.InnerException.Message}" : string.Empty;
                exMessage = ex.Message;
            }
            return Json(new
            {
                success = -1,
                errorMessage = $"{errorMessage}<br/> <b>Error Detail</b><br/> Error Message: <b>{exMessage}</b> </br> Inner Exception Message:<b> {innerEx}</b>"
            });
        }

        [HttpPost]
        public JsonResult CheckIfUserAccountHasBeenActivated(LoginModel User)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var user = userRepo.FindUser(User.Username);
                if (user != null)
                {
                    if (user.UserRole == "AD")
                    {
                        return Json(new
                        {
                            success = true
                        });
                    }
                    if(user.isActivated == false)
                    {
                        USERNAME = user.Username;
                        return Json(new
                        {
                            success = "NOT ACTIVATED"
                        });
                    }
                    if(user.HasChangedPassword == false)
                    {
                        return Json(new
                        {
                            success = "HAS NOT CHANGED PASSWORD"
                        });
                    }
                    return Json(new
                    {
                        success = true
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        infoMessage = "Invalid login details"
                    });
                }

            }
            catch (Exception ex)
            {
                innerEx = ex.InnerException != null ? ex.InnerException.InnerException != null ? $" {ex.InnerException.Message} {ex.InnerException.InnerException.Message}" : $" {ex.InnerException.Message}" : string.Empty;
                exMessage = ex.Message;
            }
            return Json(new
            {
                success = -1,
                errorMessage = $"{errorMessage}<br/> <b>Error Detail</b><br/> Error Message: <b>{exMessage}</b> </br> Inner Exception Message:<b> {innerEx}</b>"
            });
        }

       public ActionResult ChangePasswordView()
        {
            return View();
        }

        [HttpPost]
        public JsonResult ChangePassword(string oldPassword, string newPassword)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var encryptedOldPassword = Cryptography.EncryptString(oldPassword);
                var encryptedNewPassword = Cryptography.EncryptString(newPassword);
                var result = userRepo.ChangePassword(encryptedOldPassword, encryptedNewPassword);
                if(result == true)
                {
                    return Json(new
                    {
                        success = true,
                        infoMessage = "Password successfully changed. Login is now possible."
                    });
                }
                return Json(new
                {
                    success = false,
                    infoMessage = "Operation failed. Try again or contact system administrator"
                });
            }
            catch (Exception ex)
            {
                innerEx = ex.InnerException != null ? ex.InnerException.InnerException != null ? $" {ex.InnerException.Message} {ex.InnerException.InnerException.Message}" : $" {ex.InnerException.Message}" : string.Empty;
                exMessage = ex.Message;
            }
            return Json(new
            {
                success = -1,
                errorMessage = $"{errorMessage}<br/> <b>Error Detail</b><br/> Error Message: <b>{exMessage}</b> </br> Inner Exception Message:<b> {innerEx}</b>"
            });
        }
    }
}