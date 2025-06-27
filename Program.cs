using MyMvcApp.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddAuthorization(); // ✅ Добави това
builder.Services.AddHttpContextAccessor();


// Добавяне на услуги за сесии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Време на изтичане на сесията
    options.Cookie.HttpOnly = true; // Сесийните бисквитки са достъпни само чрез HTTP
    options.Cookie.IsEssential = true; // Сесийните бисквитки са необходими за функционалността
});

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql("server=localhost;database=myappdb;user=root;password=",
        new MySqlServerVersion(new Version(8, 0, 34)))); // или съответната версия

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

app.UseRouting();

app.UseSession(); // ✅ Активиране на сесиите
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();