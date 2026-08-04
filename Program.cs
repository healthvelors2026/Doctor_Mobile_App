using DoctorMobileApp.CommonClass;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDbConnectionFactory, SqlHelper>();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
// ✅ Swagger + JWT
builder.Services.AddSwaggerGen(options =>
{
    // 🔐 1. Define Bearer Auth
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   // ✅ THIS LINE
        Scheme = "bearer",                // ✅ THIS LINE (must be lowercase)
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token. Example: 12345abcdef"
    });
    // 🔐 2. Apply globally
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
            new string[] {}
        }
    });
});

// ✅ JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                // JWT Expiry UTC
                var expUtc = DateTimeOffset
                    .FromUnixTimeSeconds(long.Parse(expClaim))
                    .UtcDateTime;

                // Convert UTC -> Indian Time
                TimeZoneInfo indiaZone =
                    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                DateTime expIndia =
                    TimeZoneInfo.ConvertTimeFromUtc(expUtc, indiaZone);

                DateTime nowIndia =
                    TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaZone);

                var remainingMinutes =
                    (expIndia - nowIndia).TotalMinutes;

                // Dynamic warning minutes from appsettings
                int warningMinutes = Convert.ToInt32(
                    builder.Configuration["Jwt:WarningMinutes"]
                );

                if (remainingMinutes <= warningMinutes &&
                    remainingMinutes > 0)
                {
                    context.Response.Headers.Append(
                        "X-Token-Expiring",
                        "true"
                    );

                    context.Response.Headers.Append(
                        "X-Token-Remaining-Minutes",
                        Math.Ceiling(remainingMinutes).ToString()
                    );

                    context.Response.Headers.Append(
                        "X-Token-Expiry-IST",
                        expIndia.ToString("dd-MM-yyyy hh:mm:ss tt")
                    );
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
                // Convert Expiry to IST
                TimeZoneInfo indiaZone =
                    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                DateTime expIndia =
                    TimeZoneInfo.ConvertTimeFromUtc(
                        ex.Expires.ToUniversalTime(),
                        indiaZone
                    );

                context.Response.Headers.Append(
                    "Token-Expired-Time-IST",
                    expIndia.ToString("dd-MM-yyyy hh:mm:ss tt")
                );
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
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












app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();