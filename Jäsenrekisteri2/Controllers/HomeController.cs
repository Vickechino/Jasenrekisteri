using Jäsenrekisteri2.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Jäsenrekisteri2.Controllers
{
    public class HomeController : Controller
    {
        JäsenrekisteriEntities db = new JäsenrekisteriEntities();

        public ActionResult Index()
        {

            List<Login> model = db.Logins.ToList();
            foreach (var item in model)
            {
                item.fullname = item.firstname + " " + item.lastname;
                item.password = null;
                //item.email = null;
                item.username = null;
                //item.admin = null;
            }
            return View(model);

        }
    }
}