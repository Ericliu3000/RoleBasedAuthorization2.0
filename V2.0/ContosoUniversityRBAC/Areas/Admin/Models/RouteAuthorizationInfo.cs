namespace ContosoUniversityRBAC.Areas.Admin.Models
{
    public class RouteAuthorizationInfo
    {
        
            public string? RoutePattern { get; init; } // 路由模板
            public int Order { get; init; } // 匹配优先级
            public string? DisplayName { get; init; } // 端点名称

            public  string? HttpMethods { get; set; }   
            public  string? AuthorizationPolicies { get; set; } 
            public  string? AllowedRoles { get; set; }  
            public bool RequiresAuthentication { get; set; }
         
    }
}
