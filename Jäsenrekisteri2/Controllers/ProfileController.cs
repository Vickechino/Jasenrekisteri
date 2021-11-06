using Jäsenrekisteri2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        public ActionResult Edit([Bind(Include = "password, email, firstname, lastname, member_id, lastseen, joinDate, username")] Login editee)
        {
            if (ModelState.IsValid && Session["Username"].ToString() == db.Logins.Find(editee.member_id).username)
            {
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
                    editee.username = db.Logins.Find(editee.member_id).username;
                    editee.admin = db.Logins.Find(editee.member_id).admin;
                    editee.joinDate = db.Logins.Find(editee.member_id).joinDate;
                    var existingEntity = db.Logins.Find(editee.member_id);
                    db.Entry(existingEntity).CurrentValues.SetValues(editee);
                    db.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
                catch
                {
                    ViewBag.CreateUserError = "Error 2 käyttäjääsi muokattaessa, tarkista tiedot";
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