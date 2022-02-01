﻿using Jäsenrekisteri2.Models;
using System;
using System.Web.Mvc;

namespace Jäsenrekisteri2.Controllers
{
    public class ProfileController : Controller
    {
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();
        // OMIEN TIETOJEN EDIT NÄKYMÄN PALAUTUS
        public ActionResult Edit()
        {
            if (Session["Username"] == null) return RedirectToAction("Login", "Login");
            Login currentUser = db.Logins.Find(Session["UserID"]);
            if (currentUser == null) RedirectToAction("Index", "Home");
            if (currentUser.member_id.ToString() == Session["UserID"].ToString())
            {
                currentUser.password = "";
                return View(currentUser);
            }
            return RedirectToAction("Login", "Login");
        }
        [HttpPost] //Oman käyttäjän muokkaus
        [ValidateAntiForgeryToken] //Katso https://go.microsoft.com/fwlink/?LinkId=317598
        public ActionResult Edit([Bind(Include = "currentPassword, confirmPassword, password, email, firstname, lastname, member_id, lastseen, joinDate, username, fullname")] Login editee)
        {
            if (ModelState.IsValid && Session["Username"].ToString() == db.Logins.Find(editee.member_id).username)
            {
                try
                {
                    var bpassword = System.Text.Encoding.UTF8.GetBytes(editee.currentPassword);
                    var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bpassword);
                    var theString = Convert.ToBase64String(hash);
                    if (theString != db.Logins.Find(Session["UserID"]).password) 
                    {
                        ViewBag.EditProfileError = "Väärä salasana!";
                        return View();
                    }
                        if (editee.password != editee.confirmPassword)
                        {
                            ViewBag.EditProfileError = "Salasanat eivät täsmää!";
                            return View();
                        }
                    var newpassword = System.Text.Encoding.UTF8.GetBytes(editee.confirmPassword);
                    var newhash = System.Security.Cryptography.MD5.Create().ComputeHash(newpassword);
                    editee.password = Convert.ToBase64String(newhash);
                    if (editee.email != db.Logins.Find(editee.member_id).email) editee.emailVerified = false; //Asetetaan sähköpostin vahvistus booleani falseksi jos osoite vaihdetaan
                    editee.username = db.Logins.Find(editee.member_id).username;
                    editee.admin = db.Logins.Find(editee.member_id).admin;
                    editee.joinDate = db.Logins.Find(editee.member_id).joinDate;
                    var existingEntity = db.Logins.Find(editee.member_id);
                    editee.fullname = editee.firstname + " " + editee.lastname;
                    db.Entry(existingEntity).CurrentValues.SetValues(editee);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    ViewBag.EditProfileError = "Error käyttäjääsi muokattaessa, tarkista tiedot";
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
}