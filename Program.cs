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

        // Configuração das definições de configuração (appsettings.json)
        var configuration = builder.Configuration;

        // Verificação da chave secreta do JWT
        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrEmpty(jwtSecret))
        {
            throw new ArgumentNullException("Jwt:Secret", "A chave secreta do JWT não está configurada em appsettings.json");
        }

        // Obter a string de conexão do banco de dados a partir das configurações
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("ConnectionStrings:DefaultConnection",
                "A string de conexão com o banco de dados não está configurada em appsettings.json");
        }

        // Adiciona os serviços ao contêiner de dependências
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

        // Configuração do Swagger para documentação da API
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Configuração da autenticação JWT
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
                // Validação da chave de assinatura do JWT
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,  // Não valida o emissor
                ValidateAudience = false // Não valida o público
            };
        });

        var app = builder.Build();

        // Adiciona o middleware do SoapCore para expor o serviço SOAP
        ((IApplicationBuilder)app).UseSoapEndpoint<IUserService>("/UserService.svc", new BasicHttpBinding(), SoapSerializer.XmlSerializer);

        // Configuração do pipeline de requisições HTTP
        if (app.Environment.IsDevelopment())
        {
            // Ativa o Swagger UI apenas em ambiente de desenvolvimento
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Redirecionamento de HTTP para HTTPS
        app.UseHttpsRedirection();

        // Adiciona os middlewares para autenticação e autorização
        app.UseAuthentication(); // Adicionado para suporte ao JWT
        app.UseAuthorization();

        // Mapeia os controladores da API
        app.MapControllers();

        // Inicia a aplicação
        app.Run();
    }
}

