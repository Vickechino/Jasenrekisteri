using Jäsenrekisteri2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Jäsenrekisteri2.Controllers
{
    public class HomeController : Controller
    {
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();

        public ActionResult Index(string sortOrder)
        {

            ViewBag.NameSortParm = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
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
                    members = members.OrderByDescending(s => s.admin);
                    break;
                default:
                    members = members.OrderBy(s => s.email);
                    break;
            }
            foreach (var item in members)
            {
                //item.fullname = item.firstname + " " + item.lastname;
                item.password = null;
                item.username = null;
            }
            return View(members.ToList());
        }
        //        ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder)? "name_desc" : "";
        //   ViewBag.DateSortParm = sortOrder == "Date" ? "date_desc" : "Date";
        //   var students = from s in db.Students
        //                  select s;
        //   switch (sortOrder)
        //   {
        //      case "name_desc":
        //         students = students.OrderByDescending(s => s.LastName);
        //         break;
        //      case "Date":
        //         students = students.OrderBy(s => s.EnrollmentDate);
        //         break;
        //      case "date_desc":
        //         students = students.OrderByDescending(s => s.EnrollmentDate);
        //         break;
        //      default:
        //         students = students.OrderBy(s => s.LastName);
        //         break;
        //   }
        //   return View(students.ToList());
        //}
    }
}