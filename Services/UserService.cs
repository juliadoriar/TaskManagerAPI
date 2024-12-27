using Microsoft.Data.SqlClient; // Para trabalhar com SQL
using TaskManagerAPI.Helpers;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Models; // Para a classe User

namespace TaskManagerAPI.Services
{
    // A classe UserService implementa a interface IUserService
    // e é responsável por gerenciar as operações relacionadas aos usuários no banco de dados.
    public class UserService : IUserService
    {
        private readonly string _connectionString; // Variável para armazenar a string de conexão com o banco de dados
        private readonly string _jwtSecret; // Variável para armazenar a chave secreta para gerar tokens JWT

        // Construtor que recebe a configuração da aplicação e inicializa os campos privados
        public UserService(IConfiguration configuration)
        {
            // Obtém a string de conexão a partir da configuração
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection String cannot be null");

            // Obtém a chave secreta para JWT a partir da configuração
            _jwtSecret = configuration["Jwt:Secret"]
                ?? throw new ArgumentNullException("Jwt:Secret", "JWT Secret key is not configured");
        }

        // Método para registrar um novo usuário no banco de dados
        public bool RegisterUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString)) // Abre uma conexão com o banco de dados
            {
                connection.Open(); // Abre a conexão
                var command = new SqlCommand("INSERT INTO Users (Name, Email, Password) VALUES (@Name, " +
                    "@Email, @Password)", connection); // Prepara o comando SQL para inserção de dados
                // Adiciona os parâmetros necessários ao comando SQL
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                // Executa o comando e retorna se a inserção foi bem-sucedida
                return command.ExecuteNonQuery() > 0;
            }
        }

        // Método para autenticar um usuário utilizando e-mail e senha
        public string LoginUser(string email, string password)
        {
            using (var connection = new SqlConnection(_connectionString)) // Abre a conexão com o banco de dados
            {
                connection.Open();
                // Prepara o comando SQL para verificar se as credenciais de login estão corretas
                var command = new SqlCommand("SELECT Id, Email FROM Users WHERE Email = @Email AND " +
                    "Password = @Password", connection);
                // Adiciona os parâmetros de e-mail e senha ao comando SQL
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);

                // Executa o comando e verifica se o usuário foi encontrado
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) // Se um usuário com as credenciais fornecidas for encontrado
                    {
                        var userId = (int)reader["Id"]; // Obtém o ID do usuário
                        var userEmail = (string)reader["Email"]; // Obtém o e-mail do usuário

                        // Gera um token JWT para o usuário
                        var jwtHelper = new JwtTokenHelper(_jwtSecret); // Cria uma instância do helper de JWT
                        return jwtHelper.GenerateToken(userId, userEmail); // Retorna o token gerado
                    }
                }
            }
            // Caso o login falhe, retorna uma mensagem indicando que o e-mail ou a senha são inválidos
            return "Invalid email or password.";
        }

        // Método para obter os dados de um usuário com base no ID
        public User GetUser(int id)
        {
            using (var connection = new SqlConnection(_connectionString)) // Abre a conexão com o banco de dados
            {
                connection.Open();
                // Prepara o comando SQL para selecionar o usuário com o ID fornecido
                var command = new SqlCommand("SELECT * FROM Users WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id); // Adiciona o parâmetro do ID ao comando

                // Executa o comando e lê os dados retornados
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) // Se o usuário for encontrado
                    {
                        // Retorna um objeto User com os dados recuperados
                        return new User
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"],
                            Email = (string)reader["Email"],
                            Password = (string)reader["Password"]
                        };
                    }
                }
            }
            // Se o usuário não for encontrado, retorna null
            return null!;
        }

        // Método para listar todos os usuários registrados
        public IEnumerable<User> ListUsers()
        {
            var users = new List<User>(); // Cria uma lista para armazenar os usuários

            using (var connection = new SqlConnection(_connectionString)) // Abre a conexão com o banco de dados
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Users", connection); // Prepara o comando SQL para listar todos os usuários

                using (var reader = command.ExecuteReader()) // Executa o comando e lê os resultados
                {
                    while (reader.Read()) // Para cada usuário retornado pela consulta
                    {
                        // Adiciona o usuário à lista
                        users.Add(new User
                        {
                            Id = (int)reader["Id"],
                            Name = (string)reader["Name"],
                            Email = (string)reader["Email"],
                            Password = (string)reader["Password"]
                        });
                    }
                }
            }
            // Retorna a lista de usuários
            return users;
        }

        // Método para atualizar os dados de um usuário existente
        public bool UpdateUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString)) // Abre a conexão com o banco de dados
            {
                connection.Open();
                // Prepara o comando SQL para atualizar os dados do usuário
                var command = new SqlCommand("UPDATE Users SET Name = @Name, Email = @Email, Password = @Password WHERE Id = @Id", connection);
                // Adiciona os parâmetros necessários ao comando SQL
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                // Executa o comando e retorna se a atualização foi bem-sucedida
                return command.ExecuteNonQuery() > 0;
            }
        }

        // Método para excluir um usuário com base no ID
        public bool DeleteUser(int id)
        {
            using (var connection = new SqlConnection(_connectionString)) // Abre a conexão com o banco de dados
            {
                connection.Open();
                // Prepara o comando SQL para deletar o usuário com o ID fornecido
                var command = new SqlCommand("DELETE FROM Users WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id); // Adiciona o parâmetro do ID ao comando
                // Executa o comando e retorna se a exclusão foi bem-sucedida
                return command.ExecuteNonQuery() > 0;
            }
        }
    }
}
