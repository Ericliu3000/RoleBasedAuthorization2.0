using ContosoUniversityRBAC.Areas.Admin.Models;
using ContosoUniversityRBAC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversityRBAC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenusController : Controller
    {

        private readonly AuthorizationAndMenu _authorizationAndMenu;
        public MenusController(AuthorizationAndMenu authorizationAndMenu)
        {
            _authorizationAndMenu = authorizationAndMenu;
        }

        // GET: Admin/Enrollments
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View( _authorizationAndMenu.ResourcesMenus );
        }

       
    }
}
