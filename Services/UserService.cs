using Microsoft.Data.SqlClient; // Para trabalhar com SQL
using TaskManagerAPI.Helpers;
using TaskManagerAPI.Interfaces;
using TaskManagerAPI.Models; // Para a classe User

namespace TaskManagerAPI.Services
{
    public class UserService : IUserService
    {
        private readonly string _connectionString;
        private readonly string _jwtSecret;

        public UserService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "Connection String cannot be null");

            _jwtSecret = configuration["Jwt:Secret"]
                ?? throw new ArgumentNullException("Jwt:Secret", "JWT Secret key is not configured");
        }

        public bool RegisterUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)", connection);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public string LoginUser(string email, string password)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT Id, Email FROM Users WHERE Email = @Email AND Password = @Password", connection);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Password", password);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var userId = (int)reader["Id"];
                        var userEmail = (string)reader["Email"];

                        // Generate JWT token
                        var jwtHelper = new JwtTokenHelper(_jwtSecret); // Inject _jwtSecret via IConfiguration
                        return jwtHelper.GenerateToken(userId, userEmail);
                    }
                }
            }
            return "Invalid email or password.";
        }

        public User GetUser(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Users WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
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
            return null!;
        }

        public IEnumerable<User> ListUsers()
        {
            var users = new List<User>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Users", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
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
            return users;
        }

        public bool UpdateUser(User user)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Users SET Name = @Name, Email = @Email, Password = @Password WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", user.Id);
                command.Parameters.AddWithValue("@Name", user.Name);
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                return command.ExecuteNonQuery() > 0;
            }
        }

        public bool DeleteUser(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Users WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                return command.ExecuteNonQuery() > 0;
            }
        }
    }
}
