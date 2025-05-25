using CAPSI.Sante.API.Hubs;
using CAPSI.Sante.API.Middleware;
using FluentValidation;
using FluentValidation.AspNetCore;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog.Events;
using Serilog;
using System.Data;
using System.Text;
using System.Threading.RateLimiting;
using CAPSI.Sante.API.Configuration;
using Microsoft.Data.SqlClient;
using CAPSI.Sante.Domain.Models.Firestore;
using CAPSI.Sante.Application.Validators;
using CAPSI.Sante.Infrastructure.SqlServer.Repositories;
using CAPSI.Sante.Infrastructure.PostgreSQL.Repositories;
using CAPSI.Sante.Infrastructure.Firebase.Services;
using CAPSI.Sante.Application.Data;
using CAPSI.Sante.Infrastructure.PostgreSQL.Data;
using CAPSI.Sante.Infrastructure.SqlServer.Data;
using CAPSI.Sante.Application.Services.Firebase;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using CAPSI.Sante.Application.Services.SQLserver.Interfaces;
using CAPSI.Sante.Application.Services.SQLserver;
using CAPSI.Sante.Application.Services.Firebase.Interfaces;
using CAPSI.Sante.Application.Services.PostegreSQL;
using CAPSI.Sante.Application.Repositories.SQLserver.Interfaces;
using CAPSI.Sante.Application.Repositories.PostegreSQL.Interfaces;
using Microsoft.AspNetCore.Http.Features;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/capsi-sante-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

builder.Host.UseSerilog();

// Services de base
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configuration de Swagger avec configuration pour éviter les conflits de noms

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CAPSI.Sante API",
        Version = "v1",
        Description = "API RESTful pour la gestion des rendez-vous médicaux",
        Contact = new OpenApiContact
        {
            Name = "Support Team",
            Email = "support@capsisante.com"
        }
    });

    // Cette configuration devrait résoudre tous les problèmes de types génériques
    c.CustomSchemaIds(type => SwaggerSchemaHelper.GetCustomSchemaId(type));

    // Ajouter la sécurité JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
  
    // Ajout d'une méthode pour désactiver les validations de schéma
    c.SchemaFilter<SchemaFilter>();
});
 
// Création directe des connexions sans utiliser l'interface IDatabaseConnection
builder.Services.AddScoped(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("SqlServerConnection");
    return new SqlServerConnection(connectionString);
});

builder.Services.AddScoped(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("PostgreSQLConnection");
    return new PostgresConnection(connectionString);
});

// Enregistrement des repositories avec leurs connexions concrètes
builder.Services.AddScoped<IPatientRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<PatientRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new PatientRepository(sqlConnection, logger);
});

builder.Services.AddScoped<IMedecinRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<MedecinRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new MedecinRepository(sqlConnection, logger);
});

builder.Services.AddScoped<IAnalyticsRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AnalyticsRepository>>();
    var postgresConnection = sp.GetRequiredService<PostgresConnection>();
    return new AnalyticsRepository(postgresConnection, logger);
});

builder.Services.AddScoped<IServiceMedicalRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<ServiceMedicalRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new ServiceMedicalRepository(sqlConnection, logger);
});

builder.Services.AddScoped<IUserRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<UserRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new UserRepository(sqlConnection, logger);
});

builder.Services.AddScoped<IDossierMedicalRepository>(sp => {
    var logger = sp.GetRequiredService<ILogger<DossierMedicalRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new DossierMedicalRepository(sqlConnection, logger);
});

builder.Services.AddScoped<IAntecedentMedicalRepository>(sp => {
    var logger = sp.GetRequiredService<ILogger<AntecedentMedicalRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new AntecedentMedicalRepository(sqlConnection, logger);
});

builder.Services.AddScoped<IPrescriptionRepository>(sp => {
    var logger = sp.GetRequiredService<ILogger<PrescriptionRepository>>();
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    return new PrescriptionRepository(sqlConnection, logger);
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateDossierMedicalDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAntecedentMedicalDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreatePrescriptionDtoValidator>();

// Enregistrement du DatabaseSelector pour le code futur
builder.Services.AddScoped<IDatabaseSelector>(sp =>
{
    // Créer le sélecteur avec les services récupérés directement
    var sqlConnection = sp.GetRequiredService<SqlServerConnection>();
    var postgresConnection = sp.GetRequiredService<PostgresConnection>();

    return new DatabaseSelector(
        sqlConnection as IDatabaseConnection,
        postgresConnection as IDatabaseConnection
    );
});

// Ajouter SignalR pour les communications en temps réel
builder.Services.AddSignalR();

// Configuration de Firestore (Données temps réel)
builder.Services.Configure<FirestoreSettings>(builder.Configuration.GetSection("Firestore"));

// Enregistrement du FirestoreDb avant les services qui en dépendent
builder.Services.AddSingleton(sp => {
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new FirestoreDbBuilder
    {
        ProjectId = configuration["Firestore:ProjectId"],
        CredentialsPath = configuration["Firestore:CredentialsPath"]
    }.Build();
});

// Enregistrement des services Firestore
builder.Services.AddSingleton<IFirestoreRendezVousService, FirestoreRendezVous>();
builder.Services.AddSingleton<DisponibiliteService>();
builder.Services.AddSingleton<MessageService>();
builder.Services.AddSingleton<FirestoreNotificationService>();

// Services métier
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IMedecinService>(sp => {
    var analyticsRepository = sp.GetRequiredService<IAnalyticsRepository>();
    var firestoreRendezVousService = sp.GetRequiredService<IFirestoreRendezVousService>();
    var logger = sp.GetRequiredService<ILogger<MedecinService>>();
    var medecinRepository = sp.GetRequiredService<IMedecinRepository>();
    var firestoreDb = sp.GetRequiredService<FirestoreDb>();
    var serviceMedicalRepository = sp.GetRequiredService<IServiceMedicalRepository>();
    // Supprimez cette ligne
    // var sqlConnection = sp.GetRequiredService<SqlServerConnection>();

    return new MedecinService(
        analyticsRepository,
        firestoreRendezVousService,
        logger,
        medecinRepository,
        firestoreDb,
        serviceMedicalRepository
    // Supprimez ce paramètre
    // sqlConnection
    );
});

builder.Services.AddScoped<IServiceMedicalService, ServiceMedicalService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPatientService, PatientService>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

// Services d'authentification
builder.Services.AddScoped<IAuthService, AuthService>();

// Configuration de FluentValidation
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

// JWT Authentication
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
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder =>
        {
            builder.WithOrigins("http://localhost:3000") // Frontend URL
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            error = "Trop de requêtes. Veuillez réessayer plus tard."
        }, token);
    };
});

builder.Services.Configure<FormOptions>(options =>
{
    // Augmenter la limite pour les fichiers upload (par défaut est 128 MB)
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});


// IMPORTANT: Build l'application APRÈS toutes les configurations de services
var app = builder.Build();

app.UseStaticFiles(); // Pour servir les fichiers statiques depuis wwwroot

// Ajouter le mappage du hub SignalR
app.MapHub<NotificationsHub>("/hubs/notifications");

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseCors("AllowSpecificOrigins");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

app.Run();

Log.CloseAndFlush();

