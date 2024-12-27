using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using TaskManagerAPI.Data;
using TaskModel = TaskManagerAPI.Models.Task;
using TaskManagerAPI.NotificationService;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly Database _database;
        private readonly TaskService _taskService;

        // Construtor que injeta as dependências necessárias: banco de dados e serviço de tarefas
        public TasksController(Database database, TaskService taskService)
        {
            _database = database;
            _taskService = taskService;
        }

        // Endpoint para enviar lembretes de tarefas por e-mail
        [HttpGet("send-reminders")]
        public async Task<IActionResult> SendTaskReminders()
        {
            try
            {
                // Usa o serviço para notificar usuários sobre tarefas pendentes
                await _taskService.NotifyUsersForDueTasksAsync();
                return Ok("Task reminders sent successfully."); // Retorna sucesso
            }
            catch (Exception ex)
            {
                // Retorna erro 500 em caso de exceção
                return StatusCode(500, $"Error sending reminders: {ex.Message}");
            }
        }

        // Endpoint para criar uma nova tarefa
        [HttpPost]
        public IActionResult CreateTask(TaskModel task)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open(); // Abre a conexão com o banco de dados

                // Comando SQL para inserir a tarefa
                var command = new SqlCommand(
                    "INSERT INTO Tasks (Title, Description, DueDate, Status, UserId) VALUES (@Title, " +
                    "@Description, @DueDate, @Status, @UserId)",
                    connection
                );

                // Adiciona os parâmetros para evitar injeção de SQL
                command.Parameters.AddWithValue("@Title", task.Title);
                command.Parameters.AddWithValue("@Description", task.Description);
                command.Parameters.AddWithValue("@DueDate", task.DueDate);
                command.Parameters.AddWithValue("@Status", task.Status);
                command.Parameters.AddWithValue("@UserId", task.UserId);

                command.ExecuteNonQuery(); // Executa o comando
            }

            return Ok("Task created successfully"); // Retorna sucesso
        }

        // Endpoint para obter todas as tarefas de um usuário específico
        [HttpGet("user/{userId}")]
        public IActionResult GetTasksByUser(int userId)
        {
            var tasks = new List<TaskModel>();

            using (var connection = _database.GetConnection())
            {
                connection.Open(); // Abre a conexão

                // Comando SQL para selecionar as tarefas do usuário
                var command = new SqlCommand("SELECT * FROM Tasks WHERE UserId = @UserId", connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = command.ExecuteReader())
                {
                    // Lê os resultados e cria objetos de tarefa
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

            return Ok(tasks); // Retorna as tarefas encontradas
        }

        // Endpoint para obter uma tarefa pelo ID
        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open(); // Abre a conexão

                // Comando SQL para selecionar a tarefa pelo ID
                var command = new SqlCommand("SELECT * FROM Tasks WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Cria o objeto da tarefa se encontrado
                        var task = new TaskModel
                        {
                            Id = (int)reader["Id"],
                            Title = (string)reader["Title"],
                            Description = reader["Description"] as string,
                            DueDate = (DateTime)reader["DueDate"],
                            Status = (string)reader["Status"],
                            UserId = (int)reader["UserId"]
                        };
                        return Ok(task); // Retorna a tarefa
                    }
                }
            }
            return NotFound("Task not found"); // Retorna 404 se não encontrar a tarefa
        }

        // Endpoint para atualizar uma tarefa existente
        [HttpPut("{id}")]
        public IActionResult UpdateTask(int id, TaskModel task)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open(); // Abre a conexão

                // Comando SQL para atualizar a tarefa
                var command = new SqlCommand(
                    "UPDATE Tasks SET Title = @Title, Description = @Description, DueDate = @DueDate, " +
                    "Status = @Status WHERE Id = @Id",
                    connection
                );

                // Adiciona os parâmetros
                command.Parameters.AddWithValue("@Id", id);
                command.Parameters.AddWithValue("@Title", task.Title);
                command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DueDate", task.DueDate);
                command.Parameters.AddWithValue("@Status", task.Status);

                var rowsAffected = command.ExecuteNonQuery(); // Executa o comando
                if (rowsAffected > 0)
                {
                    return Ok("Task updated successfully"); // Retorna sucesso se houver mudanças
                }
            }
            return NotFound("Task not found"); // Retorna 404 se a tarefa não for encontrada
        }

        // Endpoint para excluir uma tarefa
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            using (var connection = _database.GetConnection())
            {
                connection.Open(); // Abre a conexão

                // Comando SQL para deletar a tarefa pelo ID
                var command = new SqlCommand("DELETE FROM Tasks WHERE Id = @Id", connection);
                command.Parameters.AddWithValue("@Id", id);

                var rowsAffected = command.ExecuteNonQuery(); // Executa o comando
                if (rowsAffected > 0)
                {
                    return Ok("Task deleted successfully"); // Retorna sucesso se a tarefa foi excluída
                }
            }
            return NotFound("Task not found"); // Retorna 404 se não encontrar a tarefa
        }
    }
}
