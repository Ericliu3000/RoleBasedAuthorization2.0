using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Sang.AspNetCore.RoleBasedAuthorization
{
    /// <summary>
    /// 模型和提供程序
    /// 在服务启动时从配置的Resource特性中获取权限类别信息
    /// https://docs.microsoft.com/zh-cn/aspnet/core/mvc/controllers/application-model?view=aspnetcore-6.0
    /// </summary>
    internal sealed class ResourceInfoModelProvider : IApplicationModelProvider
    {
        /// <summary>
        /// 执行的排序
        /// 首先 (Order=-1000)：
        /// DefaultApplicationModelProvider
        /// 然后(Order= -990)：
        /// AuthorizationApplicationModelProvider
        /// CorsApplicationModelProvider
        /// 接着是这个
        /// </summary>
        public int Order => -989;

        /// <summary>
        /// 基于其 Order 属性以倒序调用
        /// </summary>
        /// <param name="context"></param>
        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            //获取所有的控制器
            List<ResourceAttribute> attributeData = new List<ResourceAttribute>();
          

            foreach (var controllerModel in context.Result.Controllers)
            {
                //得到ResourceAttribute

                //Controller 的特性
                var resourceData = controllerModel.Attributes.OfType<ResourceAttribute>().ToArray();

               
                if (resourceData.Length > 0)
                {
                    attributeData.AddRange(resourceData);
                    foreach (var attribute in resourceData)
                    {
                        Log.WriteLog($"####resourceData:{attribute.Policy},{attribute.GetResource()}", Log.LogType.Information);
                    }
                }
                Log.WriteLog($"#### resourceData = controllerModel.Attributes:{controllerModel.ControllerName}", Log.LogType.Information);
                //Controller 中的每个方法的特性
                foreach (var actionModel in controllerModel.Actions)
                {
                    var actionResourceData = actionModel.Attributes.OfType<ResourceAttribute>().ToArray();
                    if (actionResourceData.Length > 0)
                    {
                        attributeData.AddRange(actionResourceData);

                        foreach (var attribute in actionResourceData)
                        {
                            Log.WriteLog($"####actionResourceData:Control:{controllerModel.ControllerName},Action:{actionModel.ActionName},{actionModel.ActionMethod},{attribute.Policy},{attribute.GetResource()},", Log.LogType.Information);
                        }
                    }
                   // Log.WriteLog($"####controllerModel.Actions:{actionModel.RouteValues},{actionModel.DisplayName}", Log.LogType.Information);
                }
            }
            // 整理信息集中存入全局
            foreach (var item in attributeData)
            {
                ResourceData.AddResource(item.GetResource(), item.Action);
                Log.WriteLog(text: $"####attributeData:{item.ToString()}",  Log.LogType.Information);
            }
        }

        /// <summary>
        /// 基于其 Order 属性以升序调用
        /// </summary>
        /// <param name="context"></param>
        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {

        }
    }

}