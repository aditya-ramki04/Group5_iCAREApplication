using iCareWebApplication.Data; // Import your data namespace where iCareContext is located
using iCareWebApplication.Data;
using Microsoft.EntityFrameworkCore; // Import EF Core namespace
using Pomelo.EntityFrameworkCore.MySql.Infrastructure; // Import MySQL specific infrastructure

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure MySQL Database Connection
builder.Services.AddDbContext<iCareContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)) // Specify your MySQL version
));

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
