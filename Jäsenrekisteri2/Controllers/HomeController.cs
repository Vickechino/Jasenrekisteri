using Jäsenrekisteri2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System;

namespace Jäsenrekisteri2.Controllers
{
    public class HomeController : Controller
    {
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();

        public ActionResult About() //About näkymän palautus
        {
            return View();
        }

        public ActionResult Index(string sortOrder) // Indexi näkymän/jäsenlistan palautus
        {

            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
            ViewBag.EmailSortParm = sortOrder == "email_desc" ? "email" : "email_desc";
            ViewBag.AdminSortParm = sortOrder == "admin_desc" ? "admin" : "admin_desc";
            ViewBag.emailVerifiedSortParm = sortOrder == "emailVerified_desc" ? "emailVerified" : "emailVerified_desc";
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
                case "emailVerified":
                    members = members.OrderBy(s => s.emailVerified);
                    break;
                case "emailVerified_desc":
                    members = members.OrderByDescending(s => s.emailVerified);
                    break;
                default:
                    members = members.OrderBy(s => s.fullname);
                    break;
            }
            try
            {
                return View(members.ToList());
            }
            catch { return View("About"); }
        }


        public async Task<ActionResult> EnterCode()//Sähköpostin varmistus koodin luonti, sähköpostittaminen ja varmistus näkymän palautus.
        {
            try
            {
                if ((Session["Username"] == null) || Session["emailVerified"].ToString() == "True") return RedirectToAction("Index");
                {
                    Login user = db.Logins.Find(Session["UserID"]);
                    Session["Email"] = user.email;
                    if (user.verificationEmailSent == null || DateTime.Now.AddMinutes(-5) > user.verificationEmailSent.Value)
                    {
                        System.Random random = new System.Random();
                        user.verificationEmailSent = System.DateTime.Now;
                        (Session["VerCode"]) = random.Next(100000, 2147483647);
                        db.Entry(user).CurrentValues.SetValues(user);
                        db.SaveChanges();
                        var body = "Hei, " + (Session["Name"]) + "!" + " Aktivointikoodisi on: "; //Viestin body
                        var message = new MailMessage();
                        message.To.Add(new MailAddress(db.Logins.Find(Session["UserID"]).email)); //Sähköpostin vastaanottaja
                        message.Subject = "RYHMA RY, AKTIVOINTKOODI";
                        message.Body = string.Format(body + (Session["VerCode"])); //Asetetaan viestin sisältö
                        message.IsBodyHtml = true;
                        using (var smtp = new SmtpClient())
                            await smtp.SendMailAsync(message);
                    }
                    return View();
                }
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }
        [HttpPost]  //Tästä alkaa sähköposti osoitteen varmistus
        public ActionResult VerifyEmail([Bind(Include = "VerificationCode")] Login LoginModel)
        {
            try
            {
                if (Session["emailVerified"].ToString() == "True") return RedirectToAction("Index"); /** Palautetaan index jos sähköposti osoite on jo vahvistettu **/
                Login user = db.Logins.Find(Session["UserID"]);

                if (Session["VerCode"].ToString() == LoginModel.verificationCode.ToString()) /*Verrataan koodia käyttäjän syötteeseen*//** Oikealla puolella on käyttäjän syöte **/
                {
                    string email = user.email.ToString();
                    var existingEntity = db.Logins.Find(Session["UserID"]);
                    existingEntity.emailVerified = true;
                    db.Entry(existingEntity).CurrentValues.SetValues(existingEntity);
                    db.SaveChanges();
                    Session["emailVerified"] = true;
                    ViewBag.VerifyCodeSuccess = "Sähköposti vahvistettu!";
                    return View("EnterCode", LoginModel);
                }
                ViewBag.VerifyCodeError = "Virheellinen koodi!";

                return View("EnterCode", LoginModel);
            }
            catch
            {
                ViewBag.VerifyCodeError = "Yritä uudelleen myöhemmin! Jos ongelma jatkuu ole yhteydessä tukeen.";
                return View("EnterCode", LoginModel);
            }
        }
    }
}