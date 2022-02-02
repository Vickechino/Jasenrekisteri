﻿using Jäsenrekisteri2.Models;
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
                    ViewBag.Error = "Käyttäjänimi ei voi olla tyhjä!";
                    return View("Login", LoginModel);
                }
                else if (LoginModel.password == null) 
                {
                    ViewBag.Error = "Salasana ei voi olla tyhjä!";
                    return View("Login", LoginModel);
                }
                var bpassword = System.Text.Encoding.UTF8.GetBytes(LoginModel.password);
                var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword);
                LoginModel.password = Convert.ToBase64String(hash);
                var LoggedUser = db.Logins.SingleOrDefault(x => x.username == LoginModel.username && x.password == LoginModel.password);
                if (LoggedUser != null)
                {
                    Session["Username"] = LoggedUser.username;
                    Session["Name"] = LoggedUser.firstname;
                    Session["Permission"] = LoggedUser.admin;
                    Session["UserID"] = LoggedUser.member_id;
                    Session["emailVerified"] = LoggedUser.emailVerified.ToString();
                    LoggedUser.lastseen = DateTime.Now;
                    db.Entry(LoggedUser).State = EntityState.Modified;
                    db.SaveChanges();
                    if (LoggedUser.emailVerified == null || Session["emailVerified"].ToString() == "False")
                    { return RedirectToAction("EnterCode", "Home"); }
                    else
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ViewBag.Error = "Virheellinen käyttäjätunnus/salasana";
                    return View("Login", LoginModel);
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.ToString();
                return View("Login", LoginModel);
            }
        }//** Käyttäjän tunnistus LOPPUU **


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
        }

        //Käyttäjän Luonti näkymän palatus
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
        public ActionResult Create([Bind(Include = "username, password, email, firstname, lastname, admin, joinDate, fullname, emailVerified")] Login newUser)
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
                catch (Exception e)
                {
                    ViewBag.CreateUserError = "Tapahtui virhe! Tarkista syöttämäsi tiedot." + e.ToString();
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
        public ActionResult Edit([Bind(Include = "username, password, email, firstname, lastname, admin, member_id, lastseen, joinDate, fullname, confirmPassword")] Login editee)
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
                        if (editee.password != editee.confirmPassword)
                        {
                            ViewBag.EditUserError = "Salasanat eivät täsmää!";
                            return View();
                        }
                        var bpassword = System.Text.Encoding.UTF8.GetBytes(editee.password);
                        var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword); //Muussa tapauksessa syötetty salasana hashataan ennen tiedon talletusta.
                        editee.password = Convert.ToBase64String(hash);
                    }
                    var existingEntity = db.Logins.Find(editee.member_id);
                    editee.fullname = editee.firstname + " " + editee.lastname;
                    var currentVerificationState = db.Logins.Find(editee.member_id).emailVerified;
                    var currentEmail = db.Logins.Find(editee.member_id).email;
                    if (editee.email != currentEmail) editee.emailVerified = false;
                    else editee.emailVerified = true;
                    editee.emailVerified = currentVerificationState;
                    if (editee.email.ToString().ToLower() != db.Logins.Find(editee.member_id).email.ToString().ToLower())
                        editee.emailVerified = false;  //Asetetaan sähköpostin vahvistus booleani falseksi jos osoite vaihdetaan
                    else editee.emailVerified = db.Logins.Find(editee.member_id).emailVerified; //Muutoin haetaan & käytetään vanhaa arvoa
                    db.Entry(existingEntity).CurrentValues.SetValues(editee);
                    db.SaveChanges();
                    ViewBag.ActionSuccess = "Käyttäjän: " + editee.username + " muokkaus onnistui!";
                    return View();
                }
                catch (Exception e)
                {
                    ViewBag.EditUserError = "Virhe käyttäjää muokattaessa, tarkista syötetyt tiedot!" + e.ToString();
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