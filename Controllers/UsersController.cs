//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.Extensions.Configuration;
//using TaskManagerAPI.Data;
//using TaskManagerAPI.Helpers;
//using TaskManagerAPI.Models;

//namespace TaskManagerAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class UsersController : ControllerBase
//    {
//        private readonly Database _database;
//        private readonly IConfiguration _configuration;

//        public UsersController(Database database, IConfiguration configuration)
//        {
//            _database = database;
//            _configuration = configuration;
//        }

//        [HttpPost("register")]
//        public IActionResult Register(User user)
//        {
//            using (var connection = _database.GetConnection())
//            {
//                connection.Open();
//                var command = new SqlCommand("INSERT INTO Users (Name, Email, Password) VALUES (@Name, @Email, @Password)", connection);
//                command.Parameters.AddWithValue("@Name", user.Name);
//                command.Parameters.AddWithValue("@Email", user.Email);
//                command.Parameters.AddWithValue("@Password", user.Password);

//                command.ExecuteNonQuery();
//            }

//            return Ok("User registered successfully");
//        }

//        [HttpGet("{id}")]
//        public IActionResult GetUser(int id)
//        {
//            User? user = null;
//            using (var connection = _database.GetConnection())
//            {
//                connection.Open();
//                var command = new SqlCommand("SELECT * FROM Users WHERE Id = @Id", connection);
//                command.Parameters.AddWithValue("@Id", id);

//                using (var reader = command.ExecuteReader())
//                {
//                    if (reader.Read())
//                    {
//                        user = new User
//                        {
//                            Id = (int)reader["Id"],
//                            Name = (string)reader["Name"],
//                            Email = (string)reader["Email"],
//                            Password = (string)reader["Password"]
//                        };
//                    }
//                }
//            }

//            if (user == null)
//                return NotFound("User not found");

//            return Ok(user);
//        }

//        [HttpPost("login")]
//        public IActionResult Login([FromBody] User loginUser)
//        {
//            using (var connection = _database.GetConnection())
//            {
//                connection.Open();
//                var command = new SqlCommand("SELECT * FROM Users WHERE Email = @Email AND Password = @Password", connection);
//                command.Parameters.AddWithValue("@Email", loginUser.Email);
//                command.Parameters.AddWithValue("@Password", loginUser.Password);

//                using (var reader = command.ExecuteReader())
//                {
//                    if (reader.Read())
//                    {
//                        var user = new User
//                        {
//                            Id = (int)reader["Id"],
//                            Name = (string)reader["Name"],
//                            Email = (string)reader["Email"],
//                            Password = (string)reader["Password"]
//                        };

//                        var jwtHelper = new JwtTokenHelper(_configuration["Jwt:Secret"]);
//                        var token = jwtHelper.GenerateToken(user.Id, user.Email);
//                        return Ok(new { Token = token });
//                    }
//                }
//            }

//            return Unauthorized("Invalid email or password.");
//        }

//    }
//}
