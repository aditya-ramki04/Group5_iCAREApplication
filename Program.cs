using iCareWebApplication.Data;  // Import the namespace where iCareContext is located (data access layer)
using Microsoft.EntityFrameworkCore;  // Import EF Core namespace to work with databases
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;  // Import MySQL-specific EF Core infrastructure

// Set up the builder for the web application
var builder = WebApplication.CreateBuilder(args);

// Add services to the container (dependency injection container)
builder.Services.AddControllersWithViews();  // Add services to support MVC with views (controllers + views)

// Configure MySQL Database Connection
builder.Services.AddDbContext<iCareContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnection"),  // Get the connection string from configuration
    new MySqlServerVersion(new Version(8, 0, 21)) // Specify the version of MySQL being used
));

// Add session services (session handling)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);  // Set session timeout to 30 minutes
    options.Cookie.HttpOnly = true;  // Make session cookie HTTP-only for security
    options.Cookie.IsEssential = true;  // Mark cookie as essential for the application to function
});

// Build the app (creates an instance of the web application)
var app = builder.Build();

// Configure the HTTP request pipeline (how the app should handle HTTP requests)
if (!app.Environment.IsDevelopment())  // If the app is not in development environment
{
    app.UseExceptionHandler("/Home/Error");  // Use a generic error handler route
    app.UseHsts();  // Enforce HTTP Strict Transport Security (HSTS) for secure connections
}

app.UseHttpsRedirection();  // Redirect HTTP requests to HTTPS
app.UseStaticFiles();  // Enable serving static files like CSS, JavaScript, and images

app.UseRouting();  // Enable routing (matching URLs to actions)

app.UseSession();  // Enable session state to store user data between requests

app.UseAuthorization();  // Enable authorization middleware to handle authentication and authorization

// Map default controller route pattern
app.MapControllerRoute(
    name: "default",  // The name of the route
    pattern: "{controller=Home}/{action=Index}/{id?}");  // Route pattern for the URL structure: Controller/Action/Optional ID

// Uncommented part is an alternative route mapping for account-related actions (Login/Register)
// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller=Account}/{action=LoginOrRegister}/{id?}");

// Run the application (starts listening for HTTP requests)
app.Run();
