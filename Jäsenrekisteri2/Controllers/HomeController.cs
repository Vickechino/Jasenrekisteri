using Jäsenrekisteri2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

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
            ViewBag.EmailSortParm = sortOrder ==  "email_desc" ? "email" : "email_desc";
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
    }
}