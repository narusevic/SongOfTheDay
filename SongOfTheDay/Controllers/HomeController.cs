using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace SongOfTheDay.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var user = User.Identity.GetUserId();

            return View();
        }

        [Authorize]
        public ActionResult Admin()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
    }
}