using GateFall.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// MySQL / MariaDB Ayarý
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    // 1. Email Onayý Ýstemiyoruz (Zaten yapmýþtýk)
    options.SignIn.RequireConfirmedAccount = false;

    // 2. Þifre Kurallarýný Gevþetiyoruz
    options.Password.RequireDigit = false; // Sayý zorunluluðu yok
    options.Password.RequireLowercase = false; // Küçük harf zorunluluðu yok
    options.Password.RequireUppercase = false; // Büyük harf zorunluluðu yok
    options.Password.RequireNonAlphanumeric = false; // Özel karakter (!, @ vs.) yok
    options.Password.RequiredLength = 3; // En az 3 karakter olsun yeter

    // 3. Email Benzersizliði (Opsiyonel: Email kullanmayacaðýmýz için kapatalým)
    options.User.RequireUniqueEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseSession();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();

app.Run();
