using ContosoUniversityRBAC.Data;
using ContosoUniversityRBAC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sang.AspNetCore.RoleBasedAuthorization;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ContosoUniversityRBAC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<MyRole> _roleManager;
        private readonly UserManager<MyUser> _userManager;
        private readonly MyDbContext _context;


        public HomeController(MyDbContext context,ILogger<HomeController> logger, RoleManager<MyRole> roleManager, UserManager<MyUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            //IdentityInitializer a = new IdentityInitializer(_context, _roleManager, _userManager);
            //await IdentityInitializer.InitializeResource(_roleManager);
            foreach (var claimType in GetStandardClaimTypes())
            {

                _logger.LogInformation($"##### Key:{claimType.Key}，Value：{claimType.Value}");
                
            }
            MyTest m= new MyTest();
            // 获取所有 Claims
            m.Claims = User.Claims;

            // 从 Claims 中筛选出角色
            m.Roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return View(m);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

       

 
        private IDictionary<string, string> GetStandardClaimTypes()
        {
            // Usiamo la reflection per ottenere l'elenco dei valori
            // presenti nella classe ClaimTypes.
            // Verranno mostrati nella view in un menu a tendina,
            // per facilitare la selezione
            // 我们使用反射来获取“ClaimTypes”类中所包含的值的列表。
            // 这些值将在视图中以下拉菜单的形式显示出来，
            // 以便于选择操作。
            return typeof(ClaimTypes)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .OrderBy(field => field.Name)
                .ToDictionary(static field => field.GetValue(null) as string, field => field.Name);
        }
    }
}
