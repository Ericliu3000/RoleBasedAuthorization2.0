using ContosoUniversityRBAC.Areas.Admin;
using ContosoUniversityRBAC.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace ContosoUniversityRBAC.Views.Shared.Components.Navigation
{
    // ViewComponents/NavigationViewComponent.cs
    
    public class NavigationViewComponent  : ViewComponent
    {
        private readonly ScopeAuthorizationAndMenu _scopeauthorizationAndMenu;
       // private readonly RoleManager<MyRole> _roleManager;
       // private readonly UserManager<MyUser> _userManager;
       // private readonly SignInManager<MyUser> _signInManager;
        private readonly ILogger<NavigationViewComponent> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClaimsPrincipal _user;

        //private readonly IMenuService _menuService;
        //public NavigationViewComponent(IMenuService menuService) => _menuService = menuService;
        // private readonly AuthorizationAndMenu _authorizationAndMenu;
        public NavigationViewComponent(ScopeAuthorizationAndMenu scopeauthorizationAndMenu, 
              ILogger<NavigationViewComponent> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _user = httpContextAccessor.HttpContext?.User;
     
            _scopeauthorizationAndMenu = scopeauthorizationAndMenu;
            _logger = logger;
           // _userManager = userManager;
           // _roleManager = roleManager;
           // _signInManager = signInManager;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Menus> m = new List<Menus>();
        
            List<Menus> menus =   _scopeauthorizationAndMenu.ResourcesMenus;
             
           // menus = menus.Where( m=> m.Action=="Index" && m.Area !="Home").OrderBy(m=>m.Order).ToList();
           // _logger.LogInformation($"############IViewComponentResult");

            if (!(_user == null || !_user.Identity.IsAuthenticated))
            {
             //   menus = _scopeauthorizationAndMenu.GetMenusAuthorize(menus, _user.Claims);
            }
            //else

            //{
            //    foreach (var menu in menus)
            //    {
            //        menu.Isactive = true;
            //    }
            //}

                return View(menus);
        }
    }
}
