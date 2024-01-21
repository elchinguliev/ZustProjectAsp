using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AspZustProject.Entities;
using AspZustProject.WebUI.HUBS;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var connection = builder.Configuration.GetConnectionString("myconn");
builder.Services.AddDbContext<CustomIdentityDbContext>(options =>
{
    options.UseSqlServer(connection, b => b.MigrationsAssembly("Zust.Entities"));
});


builder.Services.AddIdentity<CustomIdentityUser, CustomIdentityRole>()
    .AddEntityFrameworkStores<CustomIdentityDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IPasswordHasher<CustomIdentityUser>, PasswordHasher<CustomIdentityUser>>();

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSignalR();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("Default", "{controller=Account}/{action=Register}/{id?}");
    endpoints.MapHub<UserHub>("/userhub");
});

app.Run();
