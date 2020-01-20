using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using puck.core.Abstract;
using puck.core.Controllers;

namespace puckweb.Controllers
{
    public class HomeController : BaseController
    {
        private readonly I_Log _logger;

        public HomeController(I_Log logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return base.Puck();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return base.ErrorPage();
        }
    }
}
