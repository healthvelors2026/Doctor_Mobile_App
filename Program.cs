using DoctorMobileApp.CommonClass;
using DoctorMobileApp.WebService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IDbConnectionFactory, SqlHelper>();
builder.Services.AddHttpClient<ITokenDisplayBroadcastService, TokenDisplayBroadcastService>();
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,   
        Scheme = "bearer",                
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token. Example: 12345abcdef"
    });
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var expClaim = context.Principal?.FindFirst("exp")?.Value;

            if (!string.IsNullOrEmpty(expClaim))
            {
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim)).UtcDateTime;
                // Convert UTC -> Indian Time
                TimeZoneInfo indiaZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime expIndia = TimeZoneInfo.ConvertTimeFromUtc(expUtc, indiaZone);
                DateTime nowIndia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaZone);
                var remainingMinutes = (expIndia - nowIndia).TotalMinutes;

                int warningMinutes = Convert.ToInt32(builder.Configuration["Jwt:WarningMinutes"]);
                if (remainingMinutes <= warningMinutes && remainingMinutes > 0)
                {
                    context.Response.Headers.Append("X-Token-Expiring","true");
                    context.Response.Headers.Append("X-Token-Remaining-Minutes",Math.Ceiling(remainingMinutes).ToString());
                    context.Response.Headers.Append("X-Token-Expiry-IST",expIndia.ToString("dd-MM-yyyy hh:mm:ss tt"));
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
                TimeZoneInfo indiaZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime expIndia = TimeZoneInfo.ConvertTimeFromUtc(ex.Expires.ToUniversalTime(),indiaZone);
                
                context.Response.Headers.Append("Token-Expired-Time-IST",expIndia.ToString("dd-MM-yyyy hh:mm:ss tt")
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

var skillIconPathPattern = new Regex(@"^/(?<hospitalCode>[A-Za-z0-9]+)/MobileApp/DoctorSkillset/(?<fileName>[A-Za-z0-9_-]+\.(jpg|jpeg|png|gif))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
var kioskBannerPathPattern = new Regex(@"^/(?<hospitalCode>[A-Za-z0-9]+)/Kiosk/KioskBanners/(?<fileName>[A-Za-z0-9_-]+\.(jpg|jpeg|png|gif))$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

app.Use(async (context, next) =>
{
    // For Skill Set Image in Kiosk
    var match = skillIconPathPattern.Match(context.Request.Path.Value ?? string.Empty);
    if (HttpMethods.IsGet(context.Request.Method) && match.Success)
    {
        var physicalPath = Path.Combine(@"D:\", match.Groups["hospitalCode"].Value, "MobileApp", "DoctorSkillset", match.Groups["fileName"].Value);

        if (!File.Exists(physicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        new FileExtensionContentTypeProvider().TryGetContentType(physicalPath, out var contentType);
        context.Response.ContentType = contentType ?? "application/octet-stream";
        await context.Response.SendFileAsync(physicalPath);
        return;
    }
    // For Kiosk Banner 
    var bannerMatch = kioskBannerPathPattern.Match(context.Request.Path.Value ?? string.Empty);
    if (HttpMethods.IsGet(context.Request.Method) && bannerMatch.Success)
        {
        var physicalPath = Path.Combine(@"D:\", bannerMatch.Groups["hospitalCode"].Value, "Kiosk", "KioskBanners", bannerMatch.Groups["fileName"].Value);

        if (!File.Exists(physicalPath))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        new FileExtensionContentTypeProvider().TryGetContentType(physicalPath,out var contentType);

        context.Response.ContentType = contentType ?? "application/octet-stream";

        await context.Response.SendFileAsync(physicalPath);
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();