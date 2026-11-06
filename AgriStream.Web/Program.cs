using AgriStream.Web.Data;
using AgriStream.Web.Models;
//using AgriStream.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;






var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews(); //routing setup

builder.Services.AddDbContext<FarmDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlDb")));

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddAuthorization();


builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

//builder.Services.AddSingleton<BlobService>();
//builder.Services.AddSingleton<TelemetryService>();


builder.Services.AddHttpClient("Functions", client =>
{
    var baseUrl = builder.Configuration["FunctionBaseUrl"] ?? "http://localhost:7071/";
    client.BaseAddress = new Uri(baseUrl);
    client.Timeout = TimeSpan.FromSeconds(15);
});


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 20 * 1024 * 1024;
});


builder.Services.AddApplicationInsightsTelemetry();





var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
