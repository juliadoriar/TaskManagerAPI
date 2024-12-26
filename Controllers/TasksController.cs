using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;
using TaskModel = TaskManagerAPI.Models.Task;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly Database _database;
        private readonly TaskService _taskService;

        public TasksController(Database database, TaskService taskService)
        {
            _database = database;
            _taskService = taskService;
        }

        // Endpoint para testar o envio de notificações por email
        [HttpGet("send-reminders")]
        public async Task<IActionResult> SendTaskReminders()
        {
            try
            {
                await _taskService.NotifyUsersForDueTasksAsync();
                return Ok("Task reminders sent successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error sending reminders: {ex.Message}");
            }
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

        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("SELECT * FROM Tasks WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var task = new TaskModel
                        {
                            Id = (int)reader["Id"],
                            Title = (string)reader["Title"],
                            Description = reader["Description"] as string,
                            DueDate = (DateTime)reader["DueDate"],
                            Status = (string)reader["Status"],
                            UserId = (int)reader["UserId"]
                        };
                        return Ok(task);
                    }
                }
            }
            return NotFound("Task not found");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, TaskModel task)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("UPDATE Tasks SET Title = @Title, Description = @Description, DueDate = @DueDate, Status = @Status WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Title", task.Title);
                command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DueDate", task.DueDate);
                command.Parameters.AddWithValue("@Status", task.Status);

                var rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Task updated successfully");
                }
            }
            return NotFound("Task not found");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open();
                var command = new SqlCommand("DELETE FROM Tasks WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                var rowsAffected = command.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return Ok("Task deleted successfully");
                }
            }
            return NotFound("Task not found");
        }
    }
}
