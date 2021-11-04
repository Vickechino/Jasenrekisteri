using Jäsenrekisteri2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            if (Session["username"] != null) { return RedirectToAction("Index", "Home"); } //Palautetaan login näkymä ellei olla jo kirjauduttu
            else return View("Login");
        }
            public ActionResult Login() //Login näkymän palautus
        {
            if (Session["username"] != null) { return RedirectToAction("Index", "Home"); } //Palautetaan login näkymä ellei olla jo kirjauduttu
            else return View();
        }
        
        //Käyttäjän Sisäänkirjautuminen ALKAA TÄSTÄ
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
                    Session["Username"] = LoggedUser.username;
                    Session["Permission"] = LoggedUser.admin;
                    Session["UserID"] = LoggedUser.member_id;
                    Session["firstname"] = LoggedUser.firstname;
                    Session["lastname"] = LoggedUser.lastname;
                    LoggedUser.lastseen = DateTime.Now;
                    db.Entry(LoggedUser).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    LoginModel.LoginMessage = "Virheellinen käyttäjätunnus/salasana";
                    return View("Login", LoginModel);
                }
            }
            catch
            {
                LoginModel.LoginMessage = "Salasana ei voi olla tyhjä";
                return View("Login", LoginModel);
            }
        }
        //Käyttäjän Sisäänkirjautuminen LOPPUU TÄHÄN

        //Käyttäjän Uloskirjautuminen
        public ActionResult LogOut() 
        {
            Session.Abandon();
            ViewBag.LoggedStatus = "Out";
            return RedirectToAction("Index", "Home");
        }
        
        //Käyttäjän poistonäkymän palautus
        public ActionResult Delete(int? id) 
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Login chosenLogin = db.Logins.Find(id);
            if (chosenLogin == null) return RedirectToAction("Index", "Home");
            if (Session["Username"] != null && Session["Permission"].Equals(1))
            {
                chosenLogin.fullname = chosenLogin.firstname + " " + chosenLogin.lastname;
                return View(chosenLogin);
            }
            else return RedirectToAction("Index");
        }
        [HttpPost, ActionName("Delete")] //Käyttäjän poisto
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["Username"] != null && Session["Permission"].Equals(1))
            {
                try
                {
                    Login user = db.Logins.Find(id);

                    db.Logins.Remove(user);
                    db.SaveChanges();
                    if (Session["Username"].ToString() == user.username) { return RedirectToAction("Logout", "Login"); }
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.DeleteUserError = "Delete failed!";
                    return View();
                }

            }
            else return RedirectToAction("Index");
        }
    }
}