using DevExpress.Data.Linq;
using Digital.Forensic.Common;
using Digital.Forensic.Models.Models;
using Digital.Forensic.Repository.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Digital_Forensic.Areas.Admin.Views.WorkSchedule;
using System.Net.Mail;
using System.Net;
using Digital.Forensic.BusinessObjectLayer;

namespace Digital_Forensic.Areas.Admin.Controllers
{
    [Authorize(Roles = "AD")]
    public class WorkScheduleController : Controller
    {

        static string errorMessage = "Operation failed due to an internal system error. Contact system administrator.";
        static string PUBLICKEY = string.Empty; static string ENCRYPTEDPATH = string.Empty;
        static string TEMPPATH = string.Empty; static string FILEEXTENSION = string.Empty;
        static string FILENAME = string.Empty; static string FILEDATAENC = string.Empty;
        static string FILEDATA = string.Empty;

        IUserRepo userRepo;
        IWorkScheduleRepo scheduleRepo;
        public WorkScheduleController(IUserRepo _userRepo, IWorkScheduleRepo _scheduleRepo)
        {
            userRepo = _userRepo; scheduleRepo = _scheduleRepo;
        }

        // GET: Admin/WorkSchedule
        public ActionResult Schedule()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult WorkScheduleGridViewPartial()
        {
            return PartialView("_WorkScheduleGridViewPartial", GetEntityDataSource());
        }

        private EntityServerModeSource GetEntityDataSource()
        {
            EntityServerModeSource esms = new EntityServerModeSource();
            esms.KeyExpression = "UID";
            var model = userRepo.GetAllUsers().Where(x => x.UserRole == "RE");
            esms.QueryableSource = model;
            return esms;
        }

        public ActionResult EncryptPage()
        {
            if (TEMPPATH != string.Empty)
            {
                string fileExtension = FILEEXTENSION = Path.GetExtension(TEMPPATH);
                FILENAME = Path.GetFileNameWithoutExtension(TEMPPATH);
                if (fileExtension == ".txt" || fileExtension == ".pdf")
                {
                    string text = FILEDATAENC = $"{System.IO.File.ReadAllText(TEMPPATH)}";
                    ViewBag.EncryptedContents = text;
                    ViewBag.EncrptedPath = ENCRYPTEDPATH;
                    TEMPPATH = string.Empty;
                    ENCRYPTEDPATH = string.Empty;
                }
            }
            return View();
        }

        public ActionResult CheckBoxListPartial()
        {
            List<string> Actors = new List<string>();
            var wid = scheduleRepo.GetAllSchedules().Where(x => x.PublicKey == PUBLICKEY).FirstOrDefault().WID;
            var actors = scheduleRepo.GetAllActors().Where(x => x.WID == wid);
            foreach (var actor in actors)
            {
                Actors.Add(actor.Username);
            }
            Session["actors"] = Actors;
            return PartialView("_CheckBoxListPartial");
        }

        [HttpPost]
        public JsonResult GetPublicKey()
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                return Json(new
                {
                    success = true,
                    publicKey = PUBLICKEY
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
        public JsonResult GeneratePublicKey()
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var publicKey = Utilities.GenerateKey(10);
                return Json(new
                {
                    success = true,
                    publicKey = publicKey
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
        public JsonResult BeginTask(WorkScheduleModel Data, string selectedKeys)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                Data.CreatedDate = DateTime.Now;
                Data.GeneratedBy = User.Identity.Name;
                int wid = scheduleRepo.AddSchedule(Data);
                if (wid > 0)
                {
                    PUBLICKEY = Data.PublicKey;
                    string[] userIDS = selectedKeys.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (scheduleRepo.SaveActors(wid, userIDS))
                    {
                        foreach(var item in userIDS)
                        {
                            int userID = int.Parse(item);
                            var user = userRepo.GetAllUsers().Where(x => x.UID == userID).FirstOrDefault();
                            if(user != null)
                            {
                                SendNotificationEmail(Data, user);
                            }
                        }
                        return Json(new
                        {
                            success = true,
                            infoMessage = $"Schedule successfully started and notifications have been sent to actors for this schedule."
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                if(PUBLICKEY != string.Empty)
                {
                   scheduleRepo.RollTransaction(PUBLICKEY);
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
        public ActionResult Upload(HttpPostedFileBase file, string PublicKey)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            Evidence evidence = new Evidence();
            try
            {
                if (file.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(Server.MapPath("~/App_Data/Uploads"), fileName);
                    file.SaveAs(path);
                    evidence.Key = PublicKey;
                    evidence.Value = path;
                    FILEDATA = $"{System.IO.File.ReadAllText(path)}";
                    TEMPPATH = EncryptFile(evidence.Value.ToString(), evidence.Key);
                    ENCRYPTEDPATH = Algorithm.Encrypt(TEMPPATH, evidence.Key);
                }
                return RedirectToAction("EncryptPage", "WorkSchedule", new { Area = "Admin" });
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
        public JsonResult PushDE(string publicKey)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var widRecord = scheduleRepo.GetAllSchedules().Where(x => x.PublicKey == publicKey).FirstOrDefault();
                if (widRecord != null)
                {
                    var hashedValue = Cryptography.EncryptString(FILEDATA);
                    var record = new FilesModel()
                    {
                        WID = widRecord.WID,
                        FileData = FILEDATA,
                        FileDataEnc = FILEDATAENC,
                        FileName = FILENAME,
                        FileExtension = FILEEXTENSION,
                        FileHashedValue = hashedValue
                    };
                    var response = scheduleRepo.AddDE(record);
                    if (response)
                    {
                        return Json(new
                        {
                            success = true,
                            infoMessage = "DE pushed successfully"
                        });
                    }
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
        public JsonResult PushDCoC(string publicKey)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var widRecord = scheduleRepo.GetAllSchedules().Where(x => x.PublicKey == publicKey).FirstOrDefault();
                if (widRecord != null)
                {
                    var record = new FilesModel()
                    {
                        WID = widRecord.WID,
                        FileData = FILEDATA,
                        FileDataEnc = FILEDATAENC,
                        FileName = FILENAME,
                        FileExtension = FILEEXTENSION
                    };
                    var response = scheduleRepo.AddDCoC(record);
                    if (response == true)
                    {
                        return Json(new
                        {
                            success = true,
                            infoMessage = "DCoC pushed successfully"
                        });
                    }
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

        private string EncryptFile(string path, string key)
        {
            //Get the Input File Name and Extension.
            string fileName = Path.GetFileNameWithoutExtension(path);
            string fileExtension = Path.GetExtension(path);
            string combinedPath = $"{fileName}_enc{fileExtension}";

            //Build the File Path for the original (input) and the encrypted (output) file.
            string output = Path.Combine(Server.MapPath("~/App_Data/Uploads"), combinedPath);

            //Save the Input File, Encrypt it and save the encrypted file in output path.
            Algorithm.EncryptContentsInPath(path, output, key);

            //Delete the original (input) and the encrypted (output) file.
            //File.Delete(path);
            //File.Delete(output);

            //Response.End();
            return output;
        }

        private string DecryptFile(string path, string key)
        {
            //Get the Input File Name and Extension
            string fileName = Path.GetFileNameWithoutExtension(path).Replace("_enc", string.Empty);
            string fileExtension = Path.GetExtension(path);

            //Build the File Path for the original (input) and the decrypted (output) file
            string output = Server.MapPath("~/App_Data/Uploads") + fileName + fileExtension;

            //Save the Input File, Decrypt it and save the decrypted file in output path.
            Algorithm.DecryptContentsInPath(path, output, key);

            //Delete the original (input) and the decrypted (output) file.
            //File.Delete(path);
            //File.Delete(output);

            return output;
        }

        private void SendNotificationEmail(WorkScheduleModel Data, User user)
        {
            var fromEmail = new MailAddress("digitalforensics307@gmail.com", "Digital Forensics");
            var toEmail = new MailAddress(user.Email);
            var fromEmailPassword = "digital307forensics";
            string subject = "Participant in a Schedule";
            string body = $"Hello {user.Surname} {user.Firstname},";
            body += $"<br /><br />You have been chosen to partcipate in a schedule.";
            body += $"<br /><br />This is the key <b>{Data.PublicKey}</b> to have access to the schedule.";
            body += "<br /><br />Please do not reply this message";
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