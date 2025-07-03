using ContosoUniversityRBAC;
//using ContosoUniversityRBAC.Areas.Identity.Data;
using ContosoUniversityRBAC.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sang.AspNetCore.RoleBasedAuthorization;
var builder = WebApplication.CreateBuilder(args);

/*
 // ���������֤
builder.Services.AddOptions<MyOptions>()
    .Bind(builder.Configuration.GetSection("MyOptions"))
    .ValidateDataAnnotations();
*/
// ע�����������־
builder.Services.AddScoped<LogModelStateFilter>();

builder.Logging.ClearProviders();  
builder.Logging.AddLog4Net();
 
builder.Services.AddControllers(options =>
{
    options.Filters.Add<LogModelStateFilter>();
});

builder.Services.AddRazorPages(options => {
    // �ļ��м�����Ȩ
   // options.Conventions.AuthorizeFolder("/AdminPages", "RequireRoleAdmin");

    // ��ҳ����Ȩ
    options.Conventions.AuthorizePage("/Home/Privacy", "RequireRoleAdmin");

    // �������������ض�ҳ��
    //options.Conventions.AllowAnonymousToPage("/PublicPage");
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireRoleAdmin",
        policyBuilder => policyBuilder.RequireRole("RoleAdmin"));
});

/*
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Razor Pages ��Ȩ����
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizePage("/Identity/Account/Register", "require_role_admin");
});

// ��Ȩ��������
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("require_role_admin",
        policyBuilder => policyBuilder.RequireRole("RoleAdmin"));
});
*/
builder.Services.AddControllersWithViews( );

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ContosoUniversityRBAC.Data.MyDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();



builder.Services.AddDefaultIdentity<MyUser>(opt =>
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
    .AddRoles<MyRole>()
    .AddEntityFrameworkStores<MyDbContext>()
    .AddDefaultTokenProviders()
    .AddUserManager<UserManager<MyUser>>()
    .AddRoleManager<RoleManager<MyRole>>();
/*
IdentityBuilder iBuilder = new IdentityBuilder(typeof(MyUser), typeof(MyRole), builder.Services);

iBuilder.AddEntityFrameworkStores<MyDbContext>()
        .AddDefaultTokenProviders()
        .AddUserManager<UserManager<MyUser>>()
         .AddRoleManager<RoleManager<MyRole>>();
*/
//builder.Services.AddScoped<IClaimsTransformation, RoleClaimsTransformer>();
builder.Services.AddSangRoleBasedAuthorization();

builder.Services.AddDataProtection();
builder.Services.AddMemoryCache();
 



 
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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
                    context.TraceIdentifier );
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
        UserManager<MyUser>  userManager= (UserManager<MyUser>)services.GetRequiredService(typeof(UserManager<MyUser>));
        var logger1 = services.GetRequiredService<ILogger<Program>>();
        context.Database.EnsureCreated();
       
        await IdentityInitializer.Initialize(userManager, roleManager);
        DbInitializer.Initialize(context);
        var logger2 = services.GetRequiredService<ILogger<Program>>();

        logger2.LogError(  "An error occurred while seeding the database.");
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
app.UseAuthorization( );


// ��������ע��·�ɵ�Ȩ��

app.MapGet("/Identity/Account/Register", [Authorize(Roles = "RoleAdmin")] () => Results.Redirect("/users/create"));
 
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
