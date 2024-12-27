using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TaskManagerAPI.Data;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Services;
using SoapCore;
using System.ServiceModel;
using TaskManagerAPI.NotificationService;


public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configura��o das defini��es de configura��o (appsettings.json)
        var configuration = builder.Configuration;

        // Verifica��o da chave secreta do JWT
        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new ArgumentNullException("Jwt:Secret", "A chave secreta do JWT n�o est� configurada em appsettings.json");
        }

        // Obter a string de conex�o do banco de dados a partir das configura��es
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("ConnectionStrings:DefaultConnection",
                "A string de conex�o com o banco de dados n�o est� configurada em appsettings.json");
        }

        // Adiciona os servi�os ao cont�iner de depend�ncias
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

        // Configura��o do Swagger para documenta��o da API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configura��o da autentica��o JWT
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
                // Valida��o da chave de assinatura do JWT
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,  // N�o valida o emissor
                ValidateAudience = false // N�o valida o p�blico
            };
        });

        var app = builder.Build();

        // Adiciona o middleware do SoapCore para expor o servi�o SOAP
        ((IApplicationBuilder)app).UseSoapEndpoint<IUserService>("/UserService.svc", new BasicHttpBinding(), SoapSerializer.XmlSerializer);

        // Configura��o do pipeline de requisi��es HTTP
        if (app.Environment.IsDevelopment())
        {
            // Ativa o Swagger UI apenas em ambiente de desenvolvimento
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Redirecionamento de HTTP para HTTPS
        app.UseHttpsRedirection();

        // Adiciona os middlewares para autentica��o e autoriza��o
        app.UseAuthentication(); // Adicionado para suporte ao JWT
        app.UseAuthorization();

        // Mapeia os controladores da API
        app.MapControllers();

        // Inicia a aplica��o
        app.Run();
    }
}

