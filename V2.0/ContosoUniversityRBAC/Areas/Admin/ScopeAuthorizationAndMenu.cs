using ContosoUniversityRBAC.Areas.Admin.Models;
using System.Security.Claims;

namespace ContosoUniversityRBAC.Areas.Admin
{
    public class ScopeAuthorizationAndMenu
    {
        private readonly ILogger<ScopeAuthorizationAndMenu> _logger;
        private readonly AuthorizationAndMenu  _authorizationAndMenu;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ClaimsPrincipal _user;
        public List<Menus> ResourcesMenus = new List<Menus>();
        public Dictionary<string, bool> ResourcesMenusAction;
        public ScopeAuthorizationAndMenu(ILogger<ScopeAuthorizationAndMenu> logger, AuthorizationAndMenu authorizationAndMenu, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _user = httpContextAccessor.HttpContext?.User;
            _logger = logger;
            _authorizationAndMenu = authorizationAndMenu;
            string username = _user == null ? "None" : _user.Identity.Name;
            _logger.LogInformation($"#####ScopeAuthorizationAndMenu user={username}");
            
            ResourcesMenus = !(_user == null || !_user.Identity.IsAuthenticated) ? GetMenusAuthorize( _authorizationAndMenu.ResourcesMenus, _user.Claims ) : GetMenusAuthorize(_authorizationAndMenu.ResourcesMenus);

        }
        public List<Menus> GetMenusAuthorize(List<Menus> menus, IEnumerable<Claim> claims)
        {
            string username = _user == null ? "None" : _user.Identity.Name;
            _logger.LogInformation($"##### ScopeAuthorizationAndMenu @@ GetMenusAuthorize user={username}");

            //menus = menus.Where(m => m.Action == "Index" && m.Area != "Home").OrderBy(m => m.Order).ToList();
            // 获取用户角色集合（用于快速查找）
          

            var userRoles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToHashSet();
            List<Menus> NewMenus = menus
                                .Where(m => m.Action == "Index" && m.Area != "Home")
                                .OrderBy(m => m.Order)
                                .Select(m => (Menus)m.Clone())
                                .ToList();

            foreach (var menu in NewMenus)
            {
                // 如果菜单没有设置角色限制，或用户拥有任一允许的角色，则激活
                menu.Isactive = string.IsNullOrEmpty(menu.AllowedRoles) ||
                    menu.AllowedRoles
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Any(role => userRoles.Contains(role));
            }

            return NewMenus;

        }
        public List<Menus> GetMenusAuthorize(List<Menus> menus)
        {



            return menus
                    .Where(m => m.Action == "Index" && m.Area != "Home")
                    .OrderBy(m => m.Order)
                    .Select(m => (Menus)m.Clone()).ToList();

            

        }
        public Dictionary<string, bool> GetAuthorizeAction(string area, string controller )
        {
            // 定义需要检查的动作列表
            //var actionsToCheck = new[] { "Index", "Create", "Edit", "Detail", "Delete" };

            var actionsToCheck = _authorizationAndMenu.ResourcesMenus.Select(m => m.Action).Distinct().ToArray();
              string username = _user == null ? "None" : _user.Identity.Name;
            _logger.LogInformation($"##### ScopeAuthorizationAndMenu  GetAuthorizeAction，user={username},{string.Join(",", actionsToCheck)}");
            // 使用ToDictionary方法一次性构建结果字典
            ResourcesMenusAction= actionsToCheck.ToDictionary(
                action => action,
                action => Authorize(area, controller, action, _user.Claims)
            );
            return ResourcesMenusAction;
        }
        
        public bool Authorize(string area, string controller, string action, IEnumerable<Claim> myClaims)
        {
            // 空值检查
            if (myClaims == null || _authorizationAndMenu.ResourcesMenus == null)
                return true;

            // 获取菜单项的允许角色
          
            var menuItem = _authorizationAndMenu.ResourcesMenus
                .FirstOrDefault(m =>
                    string.Equals(m.Area, area, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.Controller, controller, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(m.Action, action, StringComparison.OrdinalIgnoreCase));

            // 如果菜单项不存在或没有设置角色限制，默认授权通过
            if (menuItem == null || string.IsNullOrEmpty(menuItem.AllowedRoles))
                return true;

            // 提取允许的角色
            var allowedRoles = menuItem.AllowedRoles
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();

            // 检查用户是否拥有任何一个允许的角色
            bool ret= myClaims
                .Any(c => c.Type == ClaimTypes.Role && allowedRoles.Contains(c.Value));
            _logger.LogInformation($"Authorize action: {action},{ret.ToString()},{menuItem.AllowedRoles.ToString()}，{_user.Identity.Name}");
            return ret;
        }

    }
}
