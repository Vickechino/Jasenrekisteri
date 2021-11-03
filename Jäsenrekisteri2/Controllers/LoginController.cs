using Jäsenrekisteri2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Jäsenrekisteri2.Controllers
{
    public class LoginController : Controller
    {
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();
        // GET: Login
        public ActionResult Index()
        {
            if (Session["username"] != null) { return RedirectToAction("Index", "Home"); } //Palautetaan login näkymä jos ei olla kirjauduttu
            else return View("Login");
        }
            public ActionResult Login() //Login näkymän palautus
        {
            if (Session["username"] != null) { return RedirectToAction("Index", "Home"); } //Palautetaan login näkymä jos ei olla kirjauduttu
            else return View();
        }
        
        //Käyttäjän sisäänkirjautuminen 
        [HttpPost]
        public ActionResult Authorize([Bind(Include = "username, password")] Login LoginModel) 
        {
            try
            {


                if (LoginModel.username == null)
                {
                    LoginModel.LoginMessage = "Käyttäjänimi ei voi olla tyhjä!";
                    return View("Login", LoginModel);
                }
                var bpassword = System.Text.Encoding.UTF8.GetBytes(LoginModel.password);
                var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword);
                LoginModel.password = Convert.ToBase64String(hash);
                var LoggedUser = db.Logins.SingleOrDefault(x => x.username == LoginModel.username && x.password == LoginModel.password);
                if (LoggedUser != null)
                {
                    Session["UserName"] = LoggedUser.username;
                    Session["Permission"] = LoggedUser.admin;
                    Session["UserID"] = LoggedUser.member_id;
                    Session["firstName"] = LoggedUser.firstname;
                    Session["lastName"] = LoggedUser.lastname;
                    LoggedUser.lastseen = DateTime.Now;
                    db.Entry(LoggedUser).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    LoginModel.LoginMessage = "Väärä käyttäjätunnus/salasana";
                    return View("Login", LoginModel);
                }
            }
            catch
            {
                LoginModel.LoginMessage = "Salasana ei voi olla tyhjä";
                return View("Login", LoginModel);
            }
        }
    }
}