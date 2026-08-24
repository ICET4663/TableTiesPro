using Microsoft.AspNetCore.Authentication.Cookies; // Needed for CookieAuthenticationDefaults
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TableTies.Data;
using TableTies.Models;
using TableTies.Services;
using Microsoft.Extensions.DependencyInjection; // Needed for ConfigureApplicationCookie extension method

var builder = WebApplication.CreateBuilder(args);

// ========== Configuration ==========
var configuration = builder.Configuration;
var connectionString = configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Missing database connection string");

// ========== Database ==========
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)); // Using SQLite as configured

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity services completely removed to fix runtime errors
// We'll use a simple approach without Identity framework

// ========== Email Service Configuration ==========
// Configure email service options from appsettings.json
builder.Services.Configure<EmailServiceOptions>(configuration.GetSection("EmailService"));

// Register the real email service implementation (for future use)
builder.Services.AddScoped<IEmailService, EmailService>();

// Use dummy email sender for now to avoid SMTP authentication issues
builder.Services.AddTransient<IEmailSender, DummyEmailSender>();

// ========== Razor Pages & API ==========
builder.Services.AddRazorPages(); // Add support for Razor Pages

// Add support for API controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        // Configure JSON serialization to ignore cycles in object graphs (useful for related entities)
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Completely disable authentication for unrestricted access
// builder.Services.AddAuthentication();

// ========== Swagger ==========
// Add services for API exploration and Swagger generation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configure Swagger document information
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TableTies API", Version = "v1" });

    // Configure JWT Bearer security definition for Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer <token>')",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer" // The scheme name must match the one used in AddJwtBearer
    });

    // Configure security requirement for Swagger UI (allows using the Authorize button)
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Reference the security definition defined above
                }
            },
            Array.Empty<string>() // Specify scopes (empty for this case)
        }
    });
});

// ========== Custom Services ==========
// Register your custom application services
builder.Services.AddScoped<IBookingService, BookingServiceImplementation>(); // Assuming IBookingService and its implementation
builder.Services.AddScoped<IRestaurantService, RestaurantService>(); // Assuming IRestaurantService and its implementation
builder.Services.AddScoped<IConsultantService, ConsultantService>(); // Ensure Consultant Service is registered
builder.Services.AddHttpContextAccessor(); // Needed to access HttpContext in services if required

// ========== App Build & Middleware ==========
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // In development, enable Swagger UI and developer exception page
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage(); // Shows detailed error information
}
else
{
    // In production, use a standard exception handler and HSTS
    app.UseExceptionHandler("/Error"); // Redirects to an error page on exceptions
    app.UseHsts(); // Enforces secure connections (HTTPS)
}

app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS
app.UseStaticFiles(); // Serves static files (HTML, CSS, JS, images)

app.UseRouting(); // Configures endpoint routing

// Authentication and authorization middleware disabled since Identity services removed
// app.UseAuthentication(); 
// app.UseAuthorization(); 

// Map controllers and Razor Pages endpoints
app.MapControllers(); // Maps routes for API controllers
app.MapRazorPages(); // Maps routes for Razor Pages

// Run the application
app.Run();