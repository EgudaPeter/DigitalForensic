using DevExpress.Data.Linq;
using DevExpress.Web.Mvc;
using Digital.Forensic.BusinessObjectLayer;
using Digital.Forensic.Common;
using Digital.Forensic.Models.Models;
using Digital.Forensic.Repository.Interfaces;
using Digital_Forensic.Areas.Admin.Views.WorkSchedule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Digital_Forensic.Areas.Regular.Controllers
{
    public class YourScheduleController : Controller
    {
        static string errorMessage = "Operation failed due to an internal system error. Contact system administrator.";
        static string PUBLICKEY = string.Empty; static string ENCRYPTEDPATH = string.Empty;
        static string TEMPPATH = string.Empty; static string FILEEXTENSION = string.Empty;
        static string FILENAME = string.Empty; static string FILEDATAENC = string.Empty;
        static string FILEDATA = string.Empty;

        IUserRepo userRepo;
        IWorkScheduleRepo scheduleRepo;
        IYourRepo yourRepo;
        public YourScheduleController(IUserRepo _userRepo, IWorkScheduleRepo _scheduleRepo, IYourRepo _yourRepo)
        {
            userRepo = _userRepo; scheduleRepo = _scheduleRepo;
            yourRepo = _yourRepo;
        }

        // GET: Regular/YourSchedule
        public ActionResult GetSchedule()
        {
            return View();
        }

        [ValidateInput(false)]
        public ActionResult YourScheduleGridViewPartial(string key)
        {
            return PartialView("_YourScheduleGridViewPartial", GetEtntityDataSource(key));
        }

        private EntityServerModeSource GetEtntityDataSource(string key)
        {
            EntityServerModeSource esms = new EntityServerModeSource();
            esms.KeyExpression = "WID";
            if (key != null)
            {
                PUBLICKEY = key;
                var model = scheduleRepo.GetAllSchedules().Where(x => x.PublicKey == key);
                esms.QueryableSource = model;
                return esms;
            }
            esms.QueryableSource = null;
            return esms;
        }

        public ActionResult WorkPage()
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
            return PartialView("_CheckBoxListPartial_YourSchedule");
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
        public JsonResult SelectSchedule(string ID)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                if (ID != null)
                {
                    int wid = int.Parse(ID);
                    var widRecord = scheduleRepo.GetAllSchedules().Where(x => x.WID == wid).FirstOrDefault();
                    if (widRecord != null)
                    {
                        PUBLICKEY = widRecord.PublicKey;
                        return Json(new
                        {
                            success = true
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
        public JsonResult CheckIfDownloadIsPossible(string key)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var widRecord = scheduleRepo.GetAllSchedules().Where(x => x.PublicKey == key).FirstOrDefault();
                if (widRecord != null)
                {
                    var DE = scheduleRepo.GetDE(widRecord.WID);
                    if (DE != null)
                    {
                        var hashedValue = Cryptography.EncryptString(DE.FileData);
                        if (hashedValue == DE.FileHashedValue)
                        {
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
                                infoMessage = "File has been tampered with!"
                            });
                        }
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
        public JsonResult DownloadFiles(string key)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                var widRecord = scheduleRepo.GetAllSchedules().Where(x => x.PublicKey == key).FirstOrDefault();
                if (widRecord != null)
                {
                    var DE = scheduleRepo.GetDE(widRecord.WID);
                    var DCoC = scheduleRepo.GetDCoC(widRecord.WID);
                    if (DE != null && DCoC != null)
                    {
                        var dir = @"C:/DigitalForensics";
                        Directory.CreateDirectory(dir);
                        var fileNameDE = DE.FileName;
                        var fileNameDCoC = DCoC.DCoCFileName;
                        fileNameDE = fileNameDE.Replace("enc", string.Empty);
                        fileNameDCoC = fileNameDCoC.Replace("enc", string.Empty);
                        var pathDE = $"{dir}/{fileNameDE}{DE.FileExtension}";
                        var pathDCoC = $"{dir}/{fileNameDCoC}{DCoC.DCoCFileExtension}";
                        FileInfo fi_DE = new FileInfo(pathDE);
                        if (fi_DE.Exists)
                        {
                            fi_DE.Delete();
                        }
                        FileInfo fi_DCoC = new FileInfo(pathDCoC);
                        if (fi_DCoC.Exists)
                        {
                            fi_DCoC.Delete();
                        }
                        using (FileStream fs_DE = fi_DE.Create())
                        {
                            Byte[] txt = new UTF8Encoding(true).GetBytes(DE.FileData);
                            fs_DE.Write(txt, 0, txt.Length);
                        }
                        using (FileStream fs_DCoC = fi_DCoC.Create())
                        {
                            Byte[] txt = new UTF8Encoding(true).GetBytes(DCoC.DCoCFileData);
                            fs_DCoC.Write(txt, 0, txt.Length);
                        }
                        return Json(new
                        {
                            success = true,
                            infoMessage = "Download successful. A folder named <b>DigitalForensics</b> has been created on your drive.<br> Your files have been downloaded to that folder."
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
                return RedirectToAction("WorkPage", "YourSchedule", new { Area = "Regular" });
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
                    var record = new FilesModel()
                    {
                        WID = widRecord.WID,
                        FileData = FILEDATA,
                        FileDataEnc = FILEDATAENC,
                        FileName = FILENAME,
                        FileExtension = FILEEXTENSION
                    };
                    var response = yourRepo.AddDE(record);
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
                    var response = yourRepo.AddDCoC(record);
                    if (response == true)
                    {
                        return Json(new
                        {
                            success = true,
                            infoMessage = "DCoC pushed successfully. Click OK to notify the next actor"
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
        public JsonResult ReturnFile(string previousActor,string presentActor)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
               if(previousActor != null && presentActor != null)
                {
                    if(previousActor == "admin")
                    {
                        var user1 = userRepo.FindUser("PIE");
                        var user2 = userRepo.FindUser(presentActor);
                        SendNotificationEmail(user1, user2);
                        return Json(new
                        {
                            success = true,
                            infoMessage = "A notification email has been sent to previous actor concerning the situation"
                        });
                    }
                    else
                    {
                        var user1 = userRepo.FindUser(previousActor);
                        var user2 = userRepo.FindUser(presentActor);
                        SendNotificationEmail(user1, user2);
                        return Json(new
                        {
                            success = true,
                            infoMessage = "A notification email has been sent to previous actor concerning the situation"
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
        public JsonResult NotifyNextActor(string nextActor)
        {
            var innerEx = string.Empty;
            var exMessage = string.Empty;
            try
            {
                if(nextActor != null)
                {
                    var user = userRepo.FindUser(nextActor);
                    if(user != null)
                    {
                        SendNotificationEmailToNextActor(user);
                        return Json(new
                        {
                            success = true,
                            infoMessage = "A notification has been sent to the next actor."
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

        private void SendNotificationEmail(User user1, User user2)
        {
            var fromEmail = new MailAddress("digitalforensics307@gmail.com", "Digital Forensics");
            var toEmail = new MailAddress(user1.Email);
            var fromEmailPassword = "digital307forensics";
            string subject = "Digital Evidence is Corrupted";
            string body = $"Hello {user1.Surname} {user1.Firstname},";
            body += $"<br /><br />The DE file you sent to {user2.Surname} {user2.Firstname} has been rejected.";
            body += $"<br /><br />The cause being that the file has been tampered with. Endeavour to check the file again or call the actor and enquire more about the situation.";
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

        private void SendNotificationEmailToNextActor(User user)
        {
            var fromEmail = new MailAddress("digitalforensics307@gmail.com", "Digital Forensics");
            var toEmail = new MailAddress(user.Email);
            var fromEmailPassword = "digital307forensics";
            string subject = "You are next";
            string body = $"Hello {user.Surname} {user.Firstname},";
            body += $"<br /><br />The DE and DCoC file is available for you to view and comment on respectively. ";
            body += $"<br /><br />Happy working.";
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