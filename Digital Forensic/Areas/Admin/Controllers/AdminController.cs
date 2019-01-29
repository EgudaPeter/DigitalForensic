using DevExpress.Data.Linq;
using DevExpress.Web.Mvc;
using Digital.Forensic.Common;
using Digital.Forensic.Models.Models;
using Digital.Forensic.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace Digital_Forensic.Areas.Admin.Controllers
{
    [Authorize(Roles ="AD")]
    public class AdminController : Controller
    {
        static string errorMessage = "Operation failed due to an internal system error. Contact system administrator.";
        static int TEMPCREATEDID = -1; static string PASSWORD = string.Empty; static string UPDATE_PASSWORD = string.Empty;
        static int TEMPUPDATEDID = -1;

        IUserRepo userRepo;
        public AdminController(IUserRepo _userRepo)
        {
            userRepo = _userRepo;
        }

        // GET: Admin/Admin
        public ActionResult Admin()
        {
            return View();
        }
        
        public ActionResult CreateUsers()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult UsersGridViewPartial()
        {
            if(TEMPCREATEDID > -1)
            {
                ViewBag.Key = TEMPCREATEDID;
                TEMPCREATEDID = -1;
            }

            if (TEMPUPDATEDID > -1)
            {
                ViewBag.Key = TEMPUPDATEDID;
                TEMPUPDATEDID = -1;
            }

            return PartialView("_UsersGridViewPartial", GetEntityDataSource());
        }

        private EntityServerModeSource GetEntityDataSource()
        {
            EntityServerModeSource esms = new EntityServerModeSource();
            esms.KeyExpression = "UID";
            var model = userRepo.GetAllUsers().Where(x => x.UserRole == "RE");
            esms.QueryableSource = model;
            return esms;
        }

        [HttpPost]
        public JsonResult ValidateEmail(string Email)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var result = userRepo.CheckIfEmailIsUnique(Email);
                var result2 = Utilities.IsValidEmail(Email);
                if (result2 == true)
                {
                    if (result == true)
                    {
                        return Json(new
                        {
                            success = true
                        });
                    }
                    return Json(new
                    {
                        success = false,
                        infoMessage = $"Entered email <b>({Email})</b> exists!"
                    });
                }
                return Json(new
                {
                    success = false,
                    infoMessage = $"Entered email <b>({Email})</b> is not in the correct format!"
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
        public JsonResult ValidateUsername(string _userName)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var result = userRepo.CheckIfUserNameIsUnique(_userName);
                if (result == true)
                {
                    return Json(new
                    {
                        success = true
                    });
                }
                return Json(new
                {
                    success = false,
                    infoMessage = $"Entered Username <b>({_userName})</b> exists!"
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
        public JsonResult GetUserToUpdate(string ID)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                int userID = int.Parse(ID);
                var user = userRepo.GetAllUsers().Where(x => x.UID == userID).FirstOrDefault();
                if(user != null)
                {
                    var password = Utilities.GenerateRandomDigitCode(6);
                    return Json(new
                    {
                        success = true,
                        id = user.UID,
                        surName = user.Surname,
                        firstName = user.Firstname,
                        userName = user.Username,
                        email = user.Email,
                        password = password
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
        public JsonResult AddUser(UserModel Data)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                Data.isActivated = false;
                Data.HasChangedPassword = false;
                Data.UserRole = "RE";
                Data.CreatedDate = DateTime.Now;
                PASSWORD = Data.Password;
                Data.Password = Cryptography.EncryptString(Data.Password);
                Data.CapturedBy = User.Identity.Name;
                Data.ActivationCode = Guid.NewGuid();
                TEMPCREATEDID = userRepo.AddUser(Data);
                SendActivationEmail(Data);
                return Json(new
                {
                    success = true,
                    infoMessage = "User created successfully!"
                });
            }
            catch (Exception ex)
            {
                if(TEMPCREATEDID > -1)
                {
                    userRepo.RollBackTransaction(TEMPCREATEDID);
                }
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
        public JsonResult UpdateUser(UserModel Data)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                Data.isActivated = false;
                Data.HasChangedPassword = false;
                UPDATE_PASSWORD = Data.Password;
                Data.Password = Cryptography.EncryptString(Data.Password);
                Data.CapturedBy = User.Identity.Name;
                Data.ActivationCode = Guid.NewGuid();
                TEMPUPDATEDID = userRepo.UpdateUser(Data);
                SendActivationEmailForUpdate(Data);
                return Json(new
                {
                    success = true,
                    infoMessage = "User updated successfully!"
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
        public JsonResult DeleteUser(string selectedKey)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                int userId = int.Parse(selectedKey);
                userRepo.DeleteASingleUser(userId);
                return Json(new
                {
                    success = true,
                    infoMessage = $"User successfully deleted!"
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
        public JsonResult DeleteUsers(string selectedKeys)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                string[] userIDS = selectedKeys.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                userRepo.DeleteAGroupOfUsers(userIDS);
                return Json(new
                {
                    success = true,
                    infoMessage = $"User(s) successfully deleted!"
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
        public JsonResult GenerateRandomPassword()
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var password = Utilities.GenerateRandomDigitCode(6);
                return Json(new
                {
                    success = true,
                    password = password
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

        [HttpGet]
        public ActionResult VerifyAccount(string ID)
        {
            if (userRepo.VerifyAccount(ID) == true)
            {
                TempData["Msg"] = "Account verification successful";
            }
            else
            {
                TempData["Msg"] = "Oops! Account verification failed! Contact your system administrator!";
            }
            return View();
        }

        private void SendActivationEmail(UserModel user)
        {
            var verifyUrl = "/Admin/Admin/VerifyAccount/" + user.ActivationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("digitalforensics307@gmail.com", "Digital Forensics");
            var toEmail = new MailAddress(user.Email);
            var fromEmailPassword = "digital307forensics";
            string subject = "Account Activation";
            string body = $"Hello {user.Surname} {user.Firstname},";
            body += $"<br /><br />These are your login credentials <b>{user.Username}</b> and <b>{PASSWORD}</b>.";
            body += "<br /><br />Before you can successfully login into your account, please click the following link to activate your account";
            body += "<br /><br /><a href ='" + link + "'>Click here to activate your account.</a>";
            body += "<br /><br />Please do not reply this message and ignore email if account is already activated!";
            body += "<hr />";
            body += "<br /><br />Thanks. Digital Forensics.";

            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            })
            {
                smtp.Send(message);
            }
        }

        private void SendActivationEmailForUpdate(UserModel user)
        {
            var verifyUrl = "/Admin/Admin/VerifyAccount/" + user.ActivationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);
            var fromEmail = new MailAddress("digitalforensics307@gmail.com", "Digital Forensics");
            var toEmail = new MailAddress(user.Email);
            var fromEmailPassword = "digital307forensics";
            string subject = "Account Activation";
            string body = $"Hello {user.Surname} {user.Firstname},";
            body += $"<br /><br />These are your new login credentials <b>{user.Username}</b> and <b>{UPDATE_PASSWORD}</b> because your account has been modified by the administrator.";
            body += "<br /><br />Please click the following link to activate your account";
            body += "<br /><br /><a href ='" + link + "'>Click here to activate your account.</a>";
            body += "<br /><br />Please do not reply this message and ignore email if account is already activated!";
            body += "<hr />";
            body += "<br /><br />Thanks. Digital Forensics.";

            var smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            })
            {
                smtp.Send(message);
            }
        }
    }
}