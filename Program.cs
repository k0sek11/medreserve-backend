using Medreserve.Features.Auth;
using Medreserve.Features.Patient;
using Medreserve.Features.Specialization;
using Medreserve.Features.Doctor;
using Medreserve.Features.Users;
using Medreserve.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<DatabaseContext>();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Medreserve API", Version = "v1" });
});

var connectionString = builder.Configuration.GetConnectionString("Default");


//Stare CORSY


// var allowedOrigins =
//     builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
//     ?? ["http://localhost:5000", "http://localhost:8000"];
//
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy(
//         "FrontendPolicy",
//         policy =>
//         {
//             policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
//         }
//     );
// });


//Corsy które mi działały na frontci
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000", 
                "http://127.0.0.1:5000",
                "http://localhost:5173", 
                "http://127.0.0.1:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthorization();

builder.Services.AddScoped<ISpecializationService, SpecializationService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();

builder
    .Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
    })
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;

    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

var app = builder.Build();

//To do zerkniecia role musza zawsze byc juz w bazie danych + migracje nie wiem czy dobre rozwiazanie
await app.ApplyDatabaseSetupAsync();

app.UseCors("FrontendPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
