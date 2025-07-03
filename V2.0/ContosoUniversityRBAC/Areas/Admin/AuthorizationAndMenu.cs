using ContosoUniversityRBAC.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
 
 
using Microsoft.AspNetCore.Mvc.Infrastructure;
 
using System.Security.Claims;
using System.Security.Policy;

namespace ContosoUniversityRBAC.Areas.Admin
{
    
    public class AuthorizationAndMenu
    {
        private readonly EndpointDataSource _endpointDataSource;
        private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;
        private readonly ILogger<AuthorizationAndMenu> _logger;
        private readonly IConfiguration _configuration; // 添加配置依赖
        public List<RouteAuthorizationInfo> routeAuthorizationInfos;
        
        public List<string> ResourcesRoles = new List<string>();
        public List<Menus> ResourcesMenus = new List<Menus>();
       // private string[] MenusOrder = { "Admin", "Contoso", "Courses", "Departmets", "Instructor", "Students", "Authorization", "Menus","Users","Departments" };
        private string[] MenusOrder = new string[0];
        private string MenusOrderString = "";
        public AuthorizationAndMenu(
            EndpointDataSource endpointDataSource,
            IActionDescriptorCollectionProvider actionDescriptorProvider,
            ILogger<AuthorizationAndMenu> logger,
            IConfiguration configuration)
        {
            _endpointDataSource = endpointDataSource;
            _actionDescriptorProvider = actionDescriptorProvider;
            _logger = logger;
            _configuration = configuration;


            MenusOrderString = _configuration.GetSection("AuthorizationConfig")
                                            .GetValue<string>("MenusOrder");
            MenusOrder = MenusOrderString.Split(",");
            routeAuthorizationInfos = GetAllEndpointAuthorizationInfo();
        }
        // 获取所有端点的授权信息
        public List<RouteAuthorizationInfo> GetAllEndpointAuthorizationInfo()
        {
            var result = new List<RouteAuthorizationInfo>();

            // 遍历所有端点数据源

            // 遍历每个数据源中的端点
            _logger.LogInformation($"#######GetAllEndpointAuthorizationInfo");
            foreach (var endpoint in _endpointDataSource.Endpoints)
            {
                // 仅处理路由端点（RouteEndpoint）
               
                if (endpoint is RouteEndpoint routeEndpoint)
                {
                    var info = new RouteAuthorizationInfo
                    {
                        RoutePattern = routeEndpoint.RoutePattern.RawText,
                        HttpMethods = string.Join(",", GetHttpMethods(routeEndpoint)),
                        AuthorizationPolicies = string.Join(",", GetAuthorizationPolicies(endpoint)),
                        AllowedRoles = string.Join(",", GetAllowedRoles(endpoint)),
                        RequiresAuthentication = IsAuthenticated(endpoint),
                        Order = routeEndpoint.Order, // 路由匹配优先级（数值越小越优先）
                        DisplayName = endpoint.DisplayName // 端点名称（如 "GET api/Values"）
                    };
                    AddResourcesRoles(info.AllowedRoles);
                    AddMenus(info);
                    result.Add(info);
                }
            }

            ResourcesRoles = ResourcesRoles.Distinct().Order().ToList();

            return result;
        }
        public void AddResourcesRoles(string Roles)
        {
            Roles = Roles.Trim();
            string[] roles = Roles.Split(',');
            foreach (string role in roles)
            {
                if (!string.IsNullOrEmpty(role))
                {
                    ResourcesRoles.Add(role);
                }
            }
        }
        public void AddMenus(RouteAuthorizationInfo info)
        {
           
            // 基础验证
            if (string.IsNullOrEmpty(info.DisplayName) || string.IsNullOrEmpty(info.RoutePattern))
                return;

            // 排除不需要的路由
            if (ShouldExcludeRoute(info))
                return;

            // 处理MVC和Razor Pages路由
            Menus menuItem = null;

            if (IsMvcRoute(info))
            {
                menuItem = ParseMvcRoute(info);
            }
            else
            {
                menuItem = ParseRazorPageRoute(info);
            }

            // 添加到菜单集合
            if (menuItem != null)
            {
                AssignMenuOrder(menuItem);
                ResourcesMenus.Add(menuItem);
            }
        }

        private bool ShouldExcludeRoute(RouteAuthorizationInfo info)
        {
            return info.RoutePattern.ToLower().Contains("identity") ||
                   info.DisplayName.ToLower().StartsWith("route:");
        }

        private bool IsMvcRoute(RouteAuthorizationInfo info)
        {
            return info.DisplayName.ToLower().Contains("controllers");
        }

        private Menus ParseMvcRoute(RouteAuthorizationInfo info)
        {
            var displayName = info.DisplayName.ToLower();
            /*
            // 提取控制器名称
            int controllerStartIndex = displayName.IndexOf("controllers.") + 12;
            int controllerEndIndex = displayName.IndexOf("controller.", controllerStartIndex);

            //if (controllerEndIndex < controllerStartIndex)
            //    return null;

            string controller = info.DisplayName.Substring(controllerStartIndex, controllerEndIndex - controllerStartIndex <0 ? 0 : controllerEndIndex - controllerStartIndex);

            // 提取区域名称
            int areaStartIndex = displayName.IndexOf("areas.");
            string area = areaStartIndex >= 0
                ? info.DisplayName.Substring(areaStartIndex + 6, controllerStartIndex - areaStartIndex - 13)
                : "Home";

            // 提取动作名称
            string actionPrefix = "controller.";
            int actionStartIndex = controllerEndIndex + actionPrefix.Length;
            string actionIndicator = $"({info.DisplayName.Substring(0, 2)}";
            int actionEndIndex = displayName.IndexOf(actionIndicator, actionStartIndex);

            //if (actionEndIndex < actionStartIndex)
            //    return null;

            string action = info.DisplayName.Substring(actionStartIndex, actionEndIndex - actionStartIndex <0 ? 0 : actionEndIndex - actionStartIndex).Trim();

            // 构建URL
            string url = area == "Home"
                ? $"/{controller}/{action}"
                : $"/{area}/{controller}/{action}";
            */
            int controllerStartIndex = info.DisplayName.ToLower().IndexOf("controllers.");
            int controllerEndIndex = info.DisplayName.ToLower().IndexOf("controller.");

            int areaStartIndex = info.DisplayName.ToLower().IndexOf("areas.");

            string actionIndicator = info.DisplayName.ToLower().Substring(0, 2);
            int actionEndIndex = info.DisplayName.ToLower().IndexOf($"({actionIndicator}");

            string controller = info.DisplayName.Substring(controllerStartIndex + 12, controllerEndIndex - controllerStartIndex - 12 > 0 ? controllerEndIndex - controllerStartIndex - 12 : 0);
            string area = areaStartIndex > 0 ? info.DisplayName.Substring(areaStartIndex + 6, controllerStartIndex - areaStartIndex - 7 > 0 ? controllerStartIndex - areaStartIndex - 7 : 0) : "Home";
            
            string action = info.DisplayName.Substring(controllerEndIndex + 11, actionEndIndex - controllerEndIndex - 11 > 0 ? actionEndIndex - controllerEndIndex - 11 : 0);
            action = action.Trim();
            string url = area == "Home" ? $"/{controller}/{action}" : $"/{area}/{controller}/{action}";
            //_logger.LogInformation($"{displayName},{action},{area},{controller}");
            
            return new Menus
            {
                Action = action,
                Url = url,
                Area = area,
                Controller = controller,
                DisplayName = info.DisplayName,
                Name = url,
                AllowedRoles = info.AllowedRoles,
                Isactive = true
            };
        }

        private Menus ParseRazorPageRoute(RouteAuthorizationInfo info)
        {
            string area = "Razor";
            string url = $"/{info.RoutePattern}";

            string[] segments = info.DisplayName.Split('/');
            if (segments.Length == 0)
                return null;

            string action = segments[^1].Trim();
            string controller = segments.Length > 1
                ? string.Join(".", segments.Take(segments.Length - 1)).Substring(1)
                : "";

            return new Menus
            {
                Action = action,
                Url = url,
                Area = area,
                Controller = controller,
                DisplayName = info.DisplayName,
                Name = url,
                AllowedRoles = info.AllowedRoles,
                Isactive = true
            };
        }

        private void AssignMenuOrder(Menus menuItem)
        {
            menuItem.Order = Array.IndexOf(MenusOrder, menuItem.Controller);

            if (menuItem.Order == -1)
            {
                menuItem.Order = 0;
                _logger.LogCritical($"####Can not find Controller #{menuItem.Controller}# in the menusorder ");
            }
        }

        public List<Menus> GetMenusAuthorize(List<Menus> menus, IEnumerable<Claim> claims)
        {
            // 获取用户角色集合（用于快速查找）
            _logger.LogInformation($"##### GetMenusAuthorize ");

            var userRoles = claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToHashSet();
            List <Menus> NewMenus=menus.Select(m=>(Menus)m.Clone()).ToList();

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
        // 获取端点支持的HTTP方法

        private List<string> GetHttpMethods(RouteEndpoint endpoint)
        {
            return endpoint.Metadata
                .OfType<HttpMethodMetadata>()
                .SelectMany(m => m.HttpMethods)
                .Distinct()
                .ToList();
        }

        // 获取端点的授权策略
        private List<string> GetAuthorizationPolicies(Endpoint endpoint)
        {
            return endpoint.Metadata
                .OfType<AuthorizeAttribute>()
                .Where(a => !string.IsNullOrEmpty(a.Policy))
                .Select(a => a.Policy)
                .Distinct()
                .ToList();
        }

        // 获取端点允许的角色
        private List<string> GetAllowedRoles(Endpoint endpoint)
        {
            return endpoint.Metadata
                .OfType<AuthorizeAttribute>()
                .Where(a => !string.IsNullOrEmpty(a.Roles))
                .SelectMany(a => a.Roles.Split(','))
                .Select(r => r.Trim())
                .Distinct()
                .ToList();
        }

        // 判断端点是否需要认证
        private bool IsAuthenticated(Endpoint endpoint)
        {
            return endpoint.Metadata
                .OfType<AuthorizeAttribute>()
                .Any() ||
                endpoint.Metadata
                .OfType<IAuthorizeData>()
                .Any();
        }
        /*
        public bool Authorize( string Area,string Controller,string Action ,
            IEnumerable<System.Security.Claims.Claim> myClaims)
        {
            bool ret = false;
           
            //_logger.LogInformation($"{Area},{Controller},{Action}, ");
            if (myClaims != null && ResourcesMenus != null)
            {
                List<string?> MenusRight = ResourcesMenus.Where(m => m.Area == Area && m.Controller == Controller && m.Action == Action)
                                                     .Select(m => m.AllowedRoles).ToList();

              
                if (MenusRight != null && MenusRight.Count > 0)
                {
                    string[] Right = MenusRight[0].Split(",");
                    foreach(string s in Right)
                    {
                        
                      if (
                                myClaims.Where( c=> c.Type== ClaimTypes.Role && c.Value==s)
                                    .ToList().Count>0 )
                       {
                            ret = true;
                            break;
                        }
                    }

                }
                else
                {
                    ret = true;
                }


                  
            }
            else
            {
                ret = true;
            }
            return ret;
        }
        public Dictionary<string,bool> GetAuthorizeAction( string Area, string Controller ,IEnumerable<System.Security.Claims.Claim> myClaims)
        {
         
             bool _Index = Authorize(Area,Controller, "Index", myClaims);
             bool _Create = Authorize(Area, Controller, "Create",myClaims);
             bool _Edit = Authorize(Area, Controller, "Edit", myClaims);
             bool _Detail = Authorize(Area, Controller, "Detail", myClaims);
             bool _Delete = Authorize(Area, Controller, "Delete", myClaims);
            Dictionary<string, bool> dictionaryAuthorize = new Dictionary<string, bool>();
            dictionaryAuthorize.Add("Create", _Create);
            dictionaryAuthorize.Add("Edit", _Edit);
            dictionaryAuthorize.Add("Delete", _Delete);
            dictionaryAuthorize.Add("Detail", _Detail);
            dictionaryAuthorize.Add("Index", _Index);
            return dictionaryAuthorize;
            }
        */
        public bool Authorize(string area, string controller, string action, IEnumerable<Claim> myClaims)
        {
            // 空值检查
            if (myClaims == null || ResourcesMenus == null)
                return true;

            // 获取菜单项的允许角色
            var menuItem = ResourcesMenus
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
            return myClaims
                .Any(c => c.Type == ClaimTypes.Role && allowedRoles.Contains(c.Value));
        }

        public Dictionary<string, bool> GetAuthorizeAction(string area, string controller, IEnumerable<Claim> myClaims)
        {
            // 定义需要检查的动作列表
            //var actionsToCheck = new[] { "Index", "Create", "Edit", "Detail", "Delete" };
             
            var actionsToCheck=ResourcesMenus.Select(m=> m.Action).Distinct().ToArray();
            _logger.LogInformation($"#####GetAuthorizeAction，{string.Join(",",actionsToCheck)}");
            // 使用ToDictionary方法一次性构建结果字典
            return actionsToCheck.ToDictionary(
                action => action,
                action => Authorize(area, controller, action, myClaims)
            );
        }
        public Dictionary<string, bool> GetAuthorizeController( IEnumerable<System.Security.Claims.Claim> myClaims)
        {
            Dictionary<string, bool> dictionaryAuthorize = new Dictionary<string, bool>();
            foreach( var M in ResourcesMenus.Where( c=>c.Action=="Index").ToList())
            {
                string Controller = M.Controller;
                if (!string.IsNullOrEmpty(Controller)) 
                {
                    bool b = Authorize(M.Area, M.Controller, "Index", myClaims);
                    dictionaryAuthorize.Add(Controller, b);
                }
            }
            return dictionaryAuthorize;

        }
}


}

