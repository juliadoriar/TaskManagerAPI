using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Data;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Services;
using SoapCore;
using System.ServiceModel;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configuration setup
        var configuration = builder.Configuration;

        // JWT Verification

        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new ArgumentNullException("Jwt:Secret", "JWT Secret key is not configured in appsettings.json");
        }

        // Get the connection string from appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("ConnectionStrings:DefaultConnection", "Database connection string is not configured in appsettings.json");
        }

        // Add services to the container.
        builder.Services.AddScoped<TaskService>(provider =>
            new TaskService(connectionString, provider.GetRequiredService<EmailService>())
        );
        builder.Services.AddControllers();
        builder.Services.AddSingleton<Database>();
        builder.Services.AddSingleton<EmailService>();
        builder.Services.AddSingleton<IUserService>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            return new UserService(configuration);
        });

        // Swagger setup
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        // JWT Authentication setup
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Secret"]);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        var app = builder.Build();

        // Adiciona o middleware SoapCore usando conversão explícita para evitar ambiguidade
        ((IApplicationBuilder)app).UseSoapEndpoint<IUserService>("/UserService.svc", new BasicHttpBinding(), SoapSerializer.XmlSerializer);

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication(); // Added for JWT support
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
