using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SongOfTheDay.Controllers
{
    public class LoginController : Controller
    {
        public string IsAdmin()
        {
            var whiteListAdmins = new List<string>()
            {
                "217.77.19.17",
                "::1",
                "78.60.109.99",
                "85.206.16.23",
                "78.56.100.33",
                "78.56.109.194"
            };

            return whiteListAdmins.Contains(Request.ServerVariables["REMOTE_ADDR"]) ? "1" : "0";
        }
    }
}