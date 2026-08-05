using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.RegularExpressions;
var builder = WebApplication.CreateBuilder(args);


// ==========================================
// Explicit Configuration Loading
// ==========================================
builder.Configuration
    .AddJsonFile(
        "appsettings.json",
        optional: false,
        reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{builder.Environment.EnvironmentName}.json",
        optional: true,
        reloadOnChange: true);


// ==========================================
// Database Connection Factory
// ==========================================
builder.Services.AddSingleton<IDbConnectionFactory, SqlHelper>();


// ==========================================
// MVC + API
// ==========================================
builder.Services.AddControllers();
builder.Services.AddControllersWithViews();


// ==========================================
// HttpContext
// ==========================================
builder.Services.AddHttpContextAccessor();


// ==========================================
// Swagger + JWT
// ==========================================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // JWT Definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    // Apply JWT Globally
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


// ==========================================
// Feedback Token Settings
// ==========================================
builder.Services
    .AddOptions<DoctorMobileApp.Models.PatientFeedback.FeedbackTokenSettings>()
    .Bind(builder.Configuration.GetSection("FeedbackTokenSettings"))
    .Validate(
        x => x.TokenExpiryHours > 0,
        "Token expiry hours must be greater than zero.")
    .ValidateOnStart();


// ==========================================
// Database
// ==========================================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


// ==========================================
// Auto Register Services & Repositories
// ==========================================
builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.Where(t =>
        t.Name.EndsWith("Service") ||
        t.Name.EndsWith("Repository")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());


// ==========================================
// JWT Authentication
// ==========================================
builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])
        ),

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var expClaim = context.Principal?.FindFirst("exp")?.Value;

            if (!string.IsNullOrEmpty(expClaim))
            {
                var expUtc = DateTimeOffset
                    .FromUnixTimeSeconds(long.Parse(expClaim))
                    .UtcDateTime;

                TimeZoneInfo indiaZone =
                    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                DateTime expIndia =
                    TimeZoneInfo.ConvertTimeFromUtc(expUtc, indiaZone);

                DateTime nowIndia =
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaZone);

                var remainingMinutes =
                    (expIndia - nowIndia).TotalMinutes;

                int warningMinutes = Convert.ToInt32(
                    builder.Configuration["Jwt:WarningMinutes"]);

                if (remainingMinutes <= warningMinutes &&
                    remainingMinutes > 0)
                {
                    context.Response.Headers.Append(
                        "X-Token-Expiring",
                        "true");

                    context.Response.Headers.Append(
                        "X-Token-Remaining-Minutes",
                        Math.Ceiling(remainingMinutes).ToString());

                    context.Response.Headers.Append(
                        "X-Token-Expiry-IST",
                        expIndia.ToString("dd-MM-yyyy hh:mm:ss tt"));
                }

                if (remainingMinutes <= 0)
                {
                    context.Fail("Token expired");
                }
            }

            return Task.CompletedTask;
        },

        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException ex)
            {
                TimeZoneInfo indiaZone =
                    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                DateTime expIndia =
                    TimeZoneInfo.ConvertTimeFromUtc(
                        ex.Expires.ToUniversalTime(),
                        indiaZone);

                context.Response.Headers.Append(
                    "Token-Expired-Time-IST",
                    expIndia.ToString("dd-MM-yyyy hh:mm:ss tt"));
            }

            return Task.CompletedTask;
        }
    };
});


// ==========================================
// Authorization
// ==========================================
builder.Services.AddAuthorization();


// ==========================================
// Build App
// ==========================================
var app = builder.Build();


// ==========================================
// Print Database Details
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    Console.WriteLine("========================================");
    Console.WriteLine("Current Database      : " + db.Database.GetDbConnection().Database);
    Console.WriteLine("Current SQL Server    : " + db.Database.GetDbConnection().DataSource);
    Console.WriteLine("Connection String     : " + db.Database.GetConnectionString());
    Console.WriteLine("========================================");
}

// ==========================================
// Exception Handling
// ==========================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// ==========================================
// Swagger
// ==========================================
// Uncomment below if you want Swagger only in Development
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
*/

// Always Enabled
app.UseSwagger();
app.UseSwaggerUI();
//}
app.UseStaticFiles();
app.UseHttpsRedirection();

var skillIconPathPattern = new Regex(
    @"^/(?<hospitalCode>[A-Za-z0-9]+)/MobileApp/DoctorSkillset/(?<fileName>[A-Za-z0-9_-]+\.(jpg|jpeg|png|gif))$",
    RegexOptions.IgnoreCase | RegexOptions.Compiled);

app.Use(async (context, next) =>
{
    // Only touch requests whose path matches the strict allow-list above.
    var match = skillIconPathPattern.Match(context.Request.Path.Value ?? string.Empty);
    if (HttpMethods.IsGet(context.Request.Method) && match.Success)
    {
        // Rebuild the physical path from the validated groups only - never from raw input.
        var physicalPath = Path.Combine(
            @"D:\", match.Groups["hospitalCode"].Value, "MobileApp", "DoctorSkillset", match.Groups["fileName"].Value);
        if (!File.Exists(physicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        // Detect content-type from the extension (image/jpeg, image/png, etc.) and stream the file.
        new FileExtensionContentTypeProvider().TryGetContentType(physicalPath, out var contentType);
        context.Response.ContentType = contentType ?? "application/octet-stream";
        await context.Response.SendFileAsync(physicalPath);
        return;
    }
    // Not an icon request - hand off to the rest of the pipeline (Swagger, auth, controllers).
    await next();
});

// ==========================================
// Middleware
// ==========================================
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// ==========================================
// API Controllers
// ==========================================
app.MapControllers();

// ==========================================
// MVC Route
// ==========================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();