//using Microsoft.AspNetCore.Mvc;
//using TaskManagerAPI.Services;

//namespace TaskManagerAPI.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class NotificationController : ControllerBase
//    {
//        private readonly EmailService _emailService;

//        public NotificationController(EmailService emailService)
//        {
//            _emailService = emailService;
//        }

//        [HttpPost("send-reminder")]
//        public async Task<IActionResult> SendTaskReminder(string userEmail, string taskName, DateTime dueDate)
//        {
//            var daysLeft = (dueDate - DateTime.UtcNow).TotalDays;
//            if (daysLeft > 2)
//            {
//                return BadRequest(new { message = "The task is not close to its due date." });
//            }

//            var subject = "Task Reminder: Upcoming Due Date!";
//            var body = $"<p>Hello,</p><p>Your task <strong>{taskName}</strong> is due on <strong>{dueDate:dd/MM/yyyy}</strong>.</p>";

//            var isSuccess = await _emailService.SendEmailAsync(userEmail, subject, body);

//            if (isSuccess)
//            {
//                return Ok(new { message = "Reminder email sent successfully." });
//            }

//            return StatusCode(500, new { message = "Failed to send the reminder email." });
//        }
//    }
//}
