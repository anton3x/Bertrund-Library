using Definitivo.Data;
using Definitivo.Hubs;
using Definitivo.Models;
using Definitivo.Models.BackgroundServices.Definitivo.BackgroundServices;
using Definitivo.Models.Configuration;
using Definitivo.Models.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<Perfil>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddSignalR();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@#*/|";
    options.User.RequireUniqueEmail = true;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "BertrundAuth"; // Nome do cookie
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Tempo de expiração do cookie
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true; // Renova o cookie a cada acesso
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Requer HTTPS
});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromSeconds(5);
});


builder.Services.AddControllersWithViews();

builder.Services.AddResponseCaching(options =>
{
    options.SizeLimit = 200;
    options.MaximumBodySize = 100;
});

builder.Services.AddTransient<DbInitializer>();

builder.Services.Configure<EmprestimoCleanupConfig>(builder.Configuration.GetSection("EmprestimoCleanup"));
builder.Services.AddHostedService<EmprestimoCleanupService>();

builder.Services.AddSingleton<ChatService>();
builder.Services.AddScoped<BibliotecaService>();

builder.Services.AddScoped<ProcessarReservasService>();

builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("EmailConfiguration"));
builder.Services.AddTransient<IEmailSender, EmailService>(); 

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Auth:Google:ClientId"];
        options.ClientSecret = builder.Configuration["Auth:Google:ClientSecret"];
    });


var app = builder.Build();

using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;

var initializer = services.GetRequiredService<DbInitializer>();

initializer.Run();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(

    new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=" + 20;
            ctx.Context.Response.Headers[HeaderNames.LastModified] = ctx.File.LastModified.ToString();
        }
    }

);

app.UseResponseCaching();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatHub>("/chatHub");

// No Program.cs ou Startup.cs
app.MapControllerRoute(
    name: "perfilReservas",
    pattern: "Perfil/Reservas",
    defaults: new { controller = "Home", action = "Perfil", sectionActivated = "reservas-tab" }
);

app.MapControllerRoute(
    name: "perfilEmprestimos",
    pattern: "Perfil/Emprestimos",
    defaults: new { controller = "Home", action = "Perfil", sectionActivated = "emprestimos-tab" }
);

app.MapControllerRoute(
    name: "perfilHistorico",
    pattern: "Perfil/Historico",
    defaults: new { controller = "Home", action = "Perfil", sectionActivated = "historico-tab" }
);


app.MapControllerRoute(
    name: "faqRoute",
    pattern: "FAQ",
    defaults: new { action = "FAQ", controller = "Home" }
);

app.MapControllerRoute(
    name: "politicasPrivacidadeRoute",
    pattern: "PoliticasDePrivacidade",
    defaults: new { action = "politicasPrivacidade", controller = "Home" }
);

app.MapControllerRoute(
    name: "aboutUsRoute",
    pattern: "SobreNos",
    defaults: new { action = "AboutUs", controller = "Home" }
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
