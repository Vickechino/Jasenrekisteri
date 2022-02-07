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
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();
        int i = 0;
        //public ActionResult Index() // Käyttäjäkokemuksen parantamikseksi vastataan myös tähän pyyntöön
        //{
        //    return RedirectToAction("Login"); // Ohjataan login näkymän palautukseen
        //}
            public ActionResult Login() // Login näkymän palautus
        {
            if (Session["username"] != null) { return RedirectToAction("Index", "Home"); } //* Palautetaan Index jos ollaan jo kirjauduttu
            else return View(); // Muutoin palautetaan login näkymä
        }
        
        // ** Käyttäjän varmistus/sisäänkirjautuminen ALKAA TÄSTÄ ** /
        [HttpPost]
        public ActionResult Authorize([Bind(Include = "username, password")] Login LoginModel) 
        {
            try
            {
                if (LoginModel.username == null) // Tarkistetaan onko käyttäjänimi kenttä tyhjä
                {
                    ViewBag.Error = "Käyttäjänimi ei voi olla tyhjä!";
                    return View("Login", LoginModel);
                }
                else if (LoginModel.password == null) // Tarkistetaan onko salasana kenttä tyhjä
                {
                    ViewBag.Error = "Salasana ei voi olla tyhjä!";
                    return View("Login", LoginModel);
                }
                var bpassword = System.Text.Encoding.UTF8.GetBytes(LoginModel.password);
                var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword); //Sotketaan käyttäjän syöte vertailua varten
                LoginModel.password = Convert.ToBase64String(hash); //Päivitetään modelin sisältö^
                var LoggedUser = db.Logins.SingleOrDefault(x => x.username == LoginModel.username && x.password == LoginModel.password); //Etsitään tiedoilla käyttäjä
                if (LoggedUser != null) //Jos käyttäjä löytyy (ei ole = null)
                {
                    Session["Username"] = LoggedUser.username;
                    Session["Name"] = LoggedUser.firstname;
                    Session["Permission"] = LoggedUser.admin;
                    Session["UserID"] = LoggedUser.member_id;
                    Session["emailVerified"] = LoggedUser.emailVerified.ToString();
                    LoggedUser.lastseen = DateTime.Now;
                    db.Entry(LoggedUser).State = EntityState.Modified;
                    db.SaveChanges();
                    if (LoggedUser.emailVerified == null || Session["emailVerified"].ToString() == "False") // ** Jos käyttäjä ei ole vahvistanut sähköpostiosoitetta...  ** /
                    { 
                        return RedirectToAction("EnterCode", "Home");  // ** Ohjataan sähköpostin vahvistus näkymään ** /
                    }
                    else
                    return RedirectToAction("Index", "Home");
                }
                else //Syötetyillä tiedoilla ei löytyntyt käyttäjää: palautetaan virheviesti
                {
                    ViewBag.Error = "Virheellinen käyttäjätunnus/salasana";
                    return View("Login", LoginModel);
                }
            }
            catch
            {
                return View("Login");
            }
        }//** Käyttäjän varmistus/sisäänkirjautuminen LOPPUU **


        //Käyttäjän Uloskirjautuminen
        public ActionResult LogOut() 
        {
            if (Session != null)
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
        // ** Käyttäjän poiston koodi alkaa ** //
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["Username"] != null && Session["Permission"].Equals(1)) // Tarkistetaan oikeuksien riittävyys
            {
                try // Yritetään poistaa rivi tietokannasta
                {
                    Login user = db.Logins.Find(id);
                    db.Logins.Remove(user);
                    db.SaveChanges();
                    if (Session["Username"].ToString() == user.username) { return RedirectToAction("Logout", "Login"); }
                    ViewBag.ActionSuccess = "Käyttäjän: " + user.username + " poisto onnistui!";
                    return View();
                }
                catch // Poisto epäonnistui: palautetaan virheviesti / ohjataan etusivulle
                {
                    if (i < 1)
                    {
                    return RedirectToAction("Index", "Home"); 
                    }
                    ViewBag.DeleteUserError = "Delete failed!";
                    i++;
                    return View();
                }
            }
            else return RedirectToAction("Index");
        }// ** Käyttäjän poiston koodi  loppuu** //

        // ** Käyttäjän Luonti näkymän palatus ** //
        public ActionResult Create()
        {
            if (Session["UserName"] != null && Session["Permission"].Equals(1))
            {
                return View();
            }
            else return RedirectToAction("Index", "Home");
        }
        // ** Käyttäjän Luonti näkymän palatus LOPPUU** //

        [HttpPost]  // ** Käyttäjän luonnin koodi ALKAA ** //
        public ActionResult Create([Bind(Include = "username, password, email, firstname, lastname, admin, joinDate, fullname, emailVerified, confirmPassword")] Login newUser)
        {
            if (ModelState.IsValid && Session["UserName"] != null && Session["Permission"].ToString() == "1") // ** Tarkistetaan Modelin sekä käyttäjän oikeuksien kelpoisuus ** /
            {
                if (newUser.password != newUser.confirmPassword) // Tarkistetaan täsmäävätkö salasanat
                {
                    ViewBag.CreateUserError = "Salasanat eivät täsmää!";
                    return View();
                }
                var userNameAlreadyExists = db.Logins.Any(x => x.username == newUser.username); // ** Bool -> löytyykö samalla(syötetyllä) nimellä käyttäjä  ** /
                if (userNameAlreadyExists) // Jos käyttäjänimi on varattu
                {
                    ViewBag.CreateUserError = "Käyttäjänimi on VARATTU";
                    return View();
                }
                try // Yritetään luoda tietokantaan uusi rivi/käyttäjä
                {
                    newUser.emailVerified = false;
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

        } // ** Käyttäjän luonnin koodi LOPPUU TÄHÄN ** //

        //** Käyttäjän muokkausnäkymän palautus ALKAA
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
        } //** Käyttäjän muokkausnäkymän palautus LOPPUU ** /

        // ** Käyttäjän muokkaus ** /
        [HttpPost] 
        [ValidateAntiForgeryToken] //Katso https://go.microsoft.com/fwlink/?LinkId=317598
        public ActionResult Edit([Bind(Include = "username, password, email, firstname, lastname, admin, member_id, lastseen, joinDate, fullname, confirmPassword")] Login editee)
        {
            if (ModelState.IsValid && Session["Permission"].Equals(1))
            {
                if (editee.password != editee.confirmPassword) //Tarkistetaan salasanan vaihto sarakkeiden identikaalisuus
                {
                    ViewBag.EditUserError = "Salasanat eivät täsmää!";
                    return View();
                }
                var userNameAlreadyExists = db.Logins.Any(x => x.username == editee.username); // Tarkistetaan löytyykö samalla nimellä käyttäjää ja asetetaan bool arvo sen mukaisesti
                if (userNameAlreadyExists && db.Logins.Find(editee.member_id).username.ToLower() != editee.username.ToLower()) /*// Jos samalla nimella löytyy käyttäjä palautetaan viesti */
                {
                    ViewBag.CreateUserError = "Käyttäjänimi varattu!";
                    return View();
                }
                try
                {
                    if (editee.password == null)
                    {
                        editee.password = db.Logins.Find(editee.member_id).password; //Salasana kenttä on tyhjä, haetaan nykyinen tietokannasta, eikä hashata.
                        editee.confirmPassword = editee.password;
                    }
                    else
                    {
                        var bpassword = System.Text.Encoding.UTF8.GetBytes(editee.password);
                        var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword); //Muussa tapauksessa syötetty salasana hashataan ennen tiedon talletusta.
                        editee.password = Convert.ToBase64String(hash);
                    }
                    var existingEntity = db.Logins.Find(editee.member_id);
                    editee.fullname = editee.firstname + " " + editee.lastname;

                    if (editee.email.ToLower() != existingEntity.email.ToLower()) // Jos kättäjän syöte ei vastaa nykyistä sähköposti-osoittetta
                        editee.emailVerified = false; // astetetaan emailVerified Falseksi...
                    else editee.emailVerified = existingEntity.emailVerified; // Muutoin käytetään vanhaa arvoa

                    db.Entry(existingEntity).CurrentValues.SetValues(editee); // Asetetaan arvot existingEntityyn
                    db.SaveChanges(); /* Säästetään muutokset tietokantaan */
                    ViewBag.ActionSuccess = "Käyttäjän: " + editee.username + " muokkaus onnistui!";
                    return View();
                }
                catch
                {
                    ViewBag.EditUserError = "Virhe käyttäjää muokattaessa, tarkista syötetyt tiedot!";
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
} /********************************* DEVELOPED BY VICKE ALL RIGHTS RESERVED *************************************/