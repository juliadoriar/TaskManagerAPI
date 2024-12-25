using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskModel = TaskManagerAPI.Models.Task;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly Database _database;

        public TasksController(Database database)
        {
            _database = database;
        }

        [HttpPost]
        public IActionResult CreateTask(TaskModel task)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("INSERT INTO Tasks (Title, Description, DueDate, Status, UserId) VALUES (@Title, @Description, @DueDate, @Status, @UserId)", connection);
                command.Parameters.AddWithValue("@Title", task.Title);
                command.Parameters.AddWithValue("@Description", task.Description);
                command.Parameters.AddWithValue("@DueDate", task.DueDate);
                command.Parameters.AddWithValue("@Status", task.Status);
                command.Parameters.AddWithValue("@UserId", task.UserId);

                command.ExecuteNonQuery();
            }

            return Ok("Task created successfully");
        }

        [HttpGet("user/{userId}")]
        public IActionResult GetTasksByUser(int userId)
        {
            var tasks = new List<TaskModel>();

            using (var connection = _database.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Tasks WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new TaskModel
                        {
                            Id = (int)reader["Id"],
                            Title = (string)reader["Title"],
                            Description = reader["Description"] as string,
                            DueDate = (DateTime)reader["DueDate"],
                            Status = (string)reader["Status"],
                            UserId = (int)reader["UserId"]
                        });
                    }
                }
            }

            return Ok(tasks);
        }
    }
}
