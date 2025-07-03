using ContosoUniversityRBAC;
using ContosoUniversityRBAC.Areas.Admin;

using ContosoUniversityRBAC.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// 配置Razor视图引擎，确保Area视图位置正确
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    //options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/MyAreas/{2}/Views/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/MyAreas/{2}/Views/Shared/{0}.cshtml");
    //options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});

 

// 注册过滤器和日志
builder.Services.AddScoped<LogModelStateFilter>();

builder.Logging.ClearProviders();
builder.Logging.AddLog4Net();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogModelStateFilter>();
});
builder.Services.AddSingleton<AuthorizationAndMenu>();
builder.Services.AddScoped<ScopeAuthorizationAndMenu>();


/*
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

*/
// 注册Razor Pages服务（关键修复点）
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();
 /*
    .AddRazorPagesOptions(options =>
 {
     // 配置Razor Pages路由
     options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/Identity/Account/Login");
     options.Conventions.AddAreaPageRoute("Identity", "/Account/Logout", "/Identity/Account/Logout");
 });
 */


// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ContosoUniversityRBAC.Data.MyDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddIdentity<MyUser, MyRole>(opt =>
{
    opt.Password.RequireDigit = false;
    opt.Password.RequireLowercase = false;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequireUppercase = false;
    opt.Password.RequiredLength = 6;

    opt.SignIn.RequireConfirmedAccount = false;


    opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
    opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
}
    )
    // .AddRoles<MyRole>()
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();
    //.AddUserManager<UserManager<MyUser>>()
    //.AddRoleManager<RoleManager<MyRole>>();
/*
IdentityBuilder iBuilder = new IdentityBuilder(typeof(MyUser), typeof(MyRole), builder.Services);

iBuilder.AddEntityFrameworkStores<MyDbContext>()
        .AddDefaultTokenProviders()
        .AddUserManager<UserManager<MyUser>>()
         .AddRoleManager<RoleManager<MyRole>>();
*/

 
builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();




var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseMigrationsEndPoint();
}
else
{
    /*
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
 */
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/plain";

            var error = context.Features.Get<IExceptionHandlerFeature>();
            if (error != null)
            {
                // 使用不同日志级别记录错误
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var logger1 = services.GetRequiredService<ILogger<Program>>();

                    logger1.LogError(
                    error.Error,
                    "未处理的异常: {RequestId}",
                    context.TraceIdentifier);
                }
                // await context.Response.WriteAsync("Internal server error. Please try again later.");
                context.Response.Redirect("/Home/Error");
            }
        });
    });
}
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MyDbContext>();
        RoleManager<MyRole> roleManager = (RoleManager<MyRole>)services.GetRequiredService(typeof(RoleManager<MyRole>));
        UserManager<MyUser> userManager = (UserManager<MyUser>)services.GetRequiredService(typeof(UserManager<MyUser>));
       // AuthorizationAndMenu authorizationAndMenu=(AuthorizationAndMenu) services.GetRequiredService(typeof (AuthorizationAndMenu));
        var logger1 = services.GetRequiredService<ILogger<Program>>();
        context.Database.EnsureCreated();

        
        DbInitializer.Initialize(context);
        
        await IdentityInitializer.Initialize(userManager, roleManager);

        //await IdentityInitializer.InitializeResource(roleManager, authorizationAndMenu);

        var logger2 = services.GetRequiredService<ILogger<Program>>();

        logger2.LogError("seeding the database succcessfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        var logger1 = services.GetRequiredService<ILogger<Program>>();

        logger1.LogError(ex, "An error occurred while seeding the database.");
    }
}


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


// 单独配置注册路由的权限
app.MapGet("/Identity/Account/Register", [Authorize(Roles = "RoleAdmin")] () => Results.Redirect("/users/create"));

// 配置路由
app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();