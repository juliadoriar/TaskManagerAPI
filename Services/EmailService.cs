using SendGrid;
using SendGrid.Helpers.Mail;

namespace TaskManagerAPI.Services
{
    public class EmailService
    {
        private readonly string _apiKey;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration)
        {
            _apiKey = configuration["EmailSettings:ApiKey"]
                      ?? throw new ArgumentNullException("EmailSettings:ApiKey");
            _senderEmail = configuration["EmailSettings:SenderEmail"]
                           ?? throw new ArgumentNullException("EmailSettings:SenderEmail");
            _senderName = configuration["EmailSettings:SenderName"]
                          ?? throw new ArgumentNullException("EmailSettings:SenderName");
        }

        public async Task<bool> SendEmailAsync(string recipientEmail, string subject, string htmlContent)
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_senderEmail, _senderName);
            var to = new EmailAddress(recipientEmail);
            var message = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlContent);

            var response = await client.SendEmailAsync(message);

            return response.StatusCode is System.Net.HttpStatusCode.OK or System.Net.HttpStatusCode.Accepted;
        }
    }
}
