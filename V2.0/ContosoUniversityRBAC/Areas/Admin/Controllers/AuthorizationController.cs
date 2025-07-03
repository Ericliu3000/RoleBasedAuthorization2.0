using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContosoUniversityRBAC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthorizationController : Controller
    {
        private readonly AuthorizationAndMenu _authorizationAndMenu;
        public AuthorizationController(AuthorizationAndMenu authorizationAndMenu) 
        {
            _authorizationAndMenu = authorizationAndMenu;
        }
        // GET: AuthorizationController
        [Authorize]
        public ActionResult Index()
        {
            return View(_authorizationAndMenu.routeAuthorizationInfos);
        }

    }
}
