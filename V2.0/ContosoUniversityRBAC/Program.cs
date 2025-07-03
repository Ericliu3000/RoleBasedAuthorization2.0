using ContosoUniversityRBAC;
using ContosoUniversityRBAC.Areas.Admin;

using ContosoUniversityRBAC.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
// ����Razor��ͼ���棬ȷ��Area��ͼλ����ȷ
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
    //options.AreaViewLocationFormats.Clear();
    options.AreaViewLocationFormats.Add("/MyAreas/{2}/Views/{1}/{0}.cshtml");
    options.AreaViewLocationFormats.Add("/MyAreas/{2}/Views/Shared/{0}.cshtml");
    //options.AreaViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
});

 

// ע�����������־
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
// ע��Razor Pages���񣨹ؼ��޸��㣩
builder.Services.AddRazorPages();

builder.Services.AddControllersWithViews();
 /*
    .AddRazorPagesOptions(options =>
 {
     // ����Razor Pages·��
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
                // ʹ�ò�ͬ��־�����¼����
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var logger1 = services.GetRequiredService<ILogger<Program>>();

                    logger1.LogError(
                    error.Error,
                    "δ������쳣: {RequestId}",
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


// ��������ע��·�ɵ�Ȩ��
app.MapGet("/Identity/Account/Register", [Authorize(Roles = "RoleAdmin")] () => Results.Redirect("/users/create"));

// ����·��
app.MapControllerRoute(
    name: "MyArea",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();