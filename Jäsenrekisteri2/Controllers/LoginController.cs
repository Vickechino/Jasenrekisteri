using Jäsenrekisteri2.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Jäsenrekisteri2.Controllers
{
    public class LoginController : Controller
    {
        private bool deleted;
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
        
        //Käyttäjän tunnistus ALKAA TÄSTÄ
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
                else if (LoginModel.password == null) 
                {
                    LoginModel.LoginMessage = "Salasana ei voi olla tyhjä!";
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
            catch (Exception e)
            {
                LoginModel.LoginMessage = e.ToString();
                return View("Login", LoginModel);
            }
        }
        //Käyttäjän tunnistus LOPPUU TÄHÄN

        //Käyttäjän Uloskirjautuminen
        public ActionResult LogOut() 
        {
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
        
        //Käyttäjän poistonäkymän palautus
        public ActionResult Delete(int? id) 
        {
            if (id == null) return RedirectToAction("Index"); ;
            Login chosenLogin = db.Logins.Find(id);
            if (chosenLogin == null) return RedirectToAction("Index", "Home");
            if (Session["Username"] != null && Session["Permission"].Equals(1))
            {
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
                    ViewBag.ActionSuccess = "Käyttäjän: " + user.username + " poisto onnistui!";
                    return View();
                }
                catch
                {
                    if (deleted == true) { return RedirectToAction("Index", "Home"); }
                    ViewBag.DeleteUserError = "Delete failed!";
                    deleted = true;
                    return View();
                }

            }
            else return RedirectToAction("Index");
        }

        //Käyttäjän Luonti näkymän palatus // CREATE
        public ActionResult Create() 
        {
            if (Session["UserName"] != null && Session["Permission"].Equals(1))
            {
                return View();
            }
            else return RedirectToAction("Index", "Home");
        }
        //Käyttäjän luominen
        [HttpPost]  
        public ActionResult Create([Bind(Include = "username, password, email, firstname, lastname, admin, joinDate, fullname, verificationCode")] Login newUser)
        {
            if (ModelState.IsValid && Session["UserName"] != null && Session["Permission"].ToString() == "1")
            {
                var userNameAlreadyExists = db.Logins.Any(x => x.username == newUser.username); //Katsotaan löytyykö samalla nimellä käyttäjä
                if (userNameAlreadyExists)
                {
                    ViewBag.CreateUserError = "Käyttäjänimi on VARATTU";
                    return View();
                }
                try
                {
                    System.Random random = new System.Random();
                    newUser.verificationCode = random.Next(100000, 2147483647);
                    newUser.joinDate = DateTime.Now;
                    var bpassword = System.Text.Encoding.UTF8.GetBytes(newUser.password);
                    var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword);
                    newUser.password = Convert.ToBase64String(hash);
                    newUser.fullname = newUser.firstname + " " + newUser.lastname;
                    db.Logins.Add(newUser);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch
                {
                    ViewBag.CreateUserError = "Tapahtui virhe! Tarkista syöttämäsi tiedot.";
                    return View();
                }

            }
            return View(User);

        }
        
        //Käyttäjän muokkausnäkymän palautus
        public ActionResult Edit(int? id) 
        {
            if (id == null) { return RedirectToAction("Index"); }
            try
            {
                if (Session["Username"] == null) return RedirectToAction("Login", "Home");
                Login user = db.Logins.Find(id);
                if (user == null) RedirectToAction("Index", "Home");
                if (Session["Username"] != null && Session["Permission"].Equals(1))
                {
                    user.password = "";
                    return View(user);
                }
                else return RedirectToAction("Index");
            }
            catch (Exception)
            {
                return RedirectToAction("Index");
            }
            finally { db.Dispose(); }
        }
        [HttpPost] //Käyttäjän muokkaus
        [ValidateAntiForgeryToken] //Katso https://go.microsoft.com/fwlink/?LinkId=317598
        public ActionResult Edit([Bind(Include = "username, password, email, firstname, lastname, admin, member_id, lastseen, joinDate, fullname")] Login editee)
        {
            if (ModelState.IsValid && Session["Permission"].Equals(1))
            {
                var userNameAlreadyExists = db.Logins.Any(x => x.username == editee.username); //Katsotaan löytyykö samalla nimellä käyttäjää
                if (userNameAlreadyExists && db.Logins.Find(editee.member_id).username.ToLower() != editee.username.ToLower())
                {
                    ViewBag.CreateUserError = "Käyttäjänimi varattu!";
                    return View();
                }
                try
                {
                    if (editee.password == null)
                    {
                        editee.password = db.Logins.Find(editee.member_id).password; //Salasana kenttä on tyhjä, haetaan nykyinen tietokannasta, eikä hashata.
                    }
                    else
                    {
                        var bpassword = System.Text.Encoding.UTF8.GetBytes(editee.password);
                        var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword); //Muussa tapauksessa syötetty salasana hashataan ennen tiedon talletusta.
                        editee.password = Convert.ToBase64String(hash);
                    }
                    var existingEntity = db.Logins.Find(editee.member_id);
                    editee.fullname = editee.firstname + " " + editee.lastname;
                    db.Entry(existingEntity).CurrentValues.SetValues(editee);
                    db.SaveChanges();
                    ViewBag.ActionSuccess = "Käyttäjän: " + editee.username + " muokkaus onnistui!";
                    return View();
                }
                catch
                {
                    ViewBag.CreateUserError = "Virhe käyttäjää muokattaessa, tarkista syötetyt tiedot!";
                    return View();
                }
                finally
                {
                    db.Dispose();
                }
            }
            return View(User);
        }
    }
}             /*  DEVELOPED BY VICKECHINO ALL RIGHTS RESERVED FFS!*/