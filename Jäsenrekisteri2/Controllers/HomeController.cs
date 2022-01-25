﻿using Jäsenrekisteri2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCEmail.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Jäsenrekisteri2.Controllers
{
    public class HomeController : Controller
    {
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();

        public ActionResult About() //About näkymän palautus
        {
            return View();
        }

        public ActionResult Index(string sortOrder) // Indexin/jäsenlistan näkymän palautus
        {

            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.EmailSortParm = sortOrder == "email_desc" ? "email" : "email_desc";
            ViewBag.AdminSortParm = sortOrder == "admin_desc" ? "admin" : "admin_desc";
            var members = from s in db.Logins
                          select s;
            switch (sortOrder)
            {
                case "name_desc":
                    members = members.OrderByDescending(s => s.fullname);
                    break;
                case "Date":
                    members = members.OrderBy(s => s.joinDate);
                    break;
                case "date_desc":
                    members = members.OrderByDescending(s => s.joinDate);
                    break;
                case "admin":
                    members = members.OrderBy(s => s.admin);
                    break;
                case "admin_desc":
                    members = members.OrderByDescending(s => s.admin);
                    break;
                case "email_desc":
                    members = members.OrderByDescending(s => s.email);
                    break;
                case "email":
                    members = members.OrderBy(s => s.email);
                    break;
                default:
                    members = members.OrderBy(s => s.fullname);
                    break;
            }
            try
            {
                foreach (var item in members)
                {
                    item.password = null;
                    item.username = null;
                }
                return View(members.ToList());

            }
            catch { return View("About"); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Verify(EmailFormModel model)
        {
            if (ModelState.IsValid)
            {
                var code = db.Logins.Find(Session["UserID"]).verificationCode;
                var body = "Your account activation code is: ";
                var message = new MailMessage();
                message.To.Add(new MailAddress("victor.alm@student.careeria.fi"));
                message.From = new MailAddress("victor.alm@student.careeria.fi");
                message.Subject = "Your account activation code";
                message.Body = string.Format(body + code); //Asetetaan viestin sisältö
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "victor.alm@student.careeria.fi",  // replace with valid value
                        Password = "IGJ-qgv-124"  // replace with valid value
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp-mail.outlook.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(message);
                    return RedirectToAction("Sent");
                }
            }
            return View(model);
        }
        public ActionResult Sent()
        {
            return View();
        }
        public ActionResult Verify()
        {
            return View();
        }
        public ActionResult EnterCode()
        {
            return View();
        }
        [HttpPost]
        public ActionResult VerifyEmail([Bind(Include = "VerificationCode")] Login LoginModel)
        {
            try
            {
                var theCode = db.Logins.Find(Session["UserID"]).verificationCode;
                if (theCode == LoginModel.verificationCode)
                {
                    var existingEntity = db.Logins.Find(Session["UserID"]);
                    existingEntity.emailVerified = true;
                    db.Entry(existingEntity).CurrentValues.SetValues(existingEntity);
                    db.SaveChanges();
                    ViewBag.VerifyCodeSuccess = "Sähköposti vahvistettu!";
                    return View("EnterCode", LoginModel);
                }
            }
            catch
            {
                ViewBag.VerifyCodeError = "Sähköposti vahvistettu!";
                return View("VerifyCode, Home", LoginModel);
            }
                ViewBag.VerifyCodeError = "Virhe!";
                return View("VerifyCode, Home", LoginModel);
        }
    }
}