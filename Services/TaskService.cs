using Microsoft.Data.SqlClient;
using TaskModel = TaskManagerAPI.Models.Task;

namespace TaskManagerAPI.Services
{
    public class TaskService
    {
        private readonly string _connectionString;
        private readonly EmailService _emailService;

        public TaskService(string connectionString, EmailService emailService)
        {
            _connectionString = connectionString;
            _emailService = emailService;
        }

        // Alteração para async Task, permitindo await corretamente
        public async Task NotifyUsersForDueTasksAsync()
        {
            var tasksToNotify = new List<(string UserEmail, string TaskName, DateTime DueDate)>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Query para pegar tarefas com vencimento nos próximos 2 dias
                var command = new SqlCommand(
                    "SELECT u.Email, t.Title, t.DueDate FROM Tasks t JOIN Users u ON t.UserId = u.Id WHERE DATEDIFF(day, GETDATE(), t.DueDate) <= 2 AND t.Status != 'Completed'",
                    connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Garantir que não estamos adicionando nulos
                        var email = reader["Email"] as string ?? string.Empty; // Se Email for nulo, usa string vazia
                        var taskName = reader["Title"] as string ?? string.Empty; // Se Title for nulo, usa string vazia
                        var dueDate = reader["DueDate"] as DateTime? ?? DateTime.MinValue; // Garantir que a data é válida

                        tasksToNotify.Add((email, taskName, dueDate));
                    }
                }
            }

            // Enviar os e-mails
            foreach (var task in tasksToNotify)
            {
                var subject = "Task Reminder: Upcoming Due Date!";
                var body = $"<p>Hello,</p><p>Your task <strong>{task.TaskName}</strong> is due on <strong>{task.DueDate:dd/MM/yyyy}</strong>.</p>";

                // Enviar o e-mail para o usuário
                var emailSent = await _emailService.SendEmailAsync(task.UserEmail, subject, body);

                if (emailSent)
                {
                    Console.WriteLine($"Email sent to {task.UserEmail} about task: {task.TaskName}");
                }
                else
                {
                    Console.WriteLine($"Failed to send email to {task.UserEmail}.");
                }
            }
        }
    }
}
