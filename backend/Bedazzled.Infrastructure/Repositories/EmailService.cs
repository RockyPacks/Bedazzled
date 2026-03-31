using Bedazzled.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace Bedazzled.Infrastructure.Repositories;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IResend _resend;
    private readonly string _fromEmail;

    public EmailService(ILogger<EmailService> logger, IResend resend, IConfiguration configuration)
    {
        _logger = logger;
        _resend = resend;
        _fromEmail = configuration["Resend:FromEmail"] ?? "Bedazzled <onboarding@resend.dev>";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        _logger.LogInformation("Attempting to send email via Resend to {Email} with subject: {Subject}", toEmail, subject);

        try
        {
            var email = new EmailMessage
            {
                From = _fromEmail,
                Subject = subject,
                HtmlBody = body
            };
            email.To.Add(toEmail);

            await _resend.EmailSendAsync(email);
            _logger.LogInformation("Email sent successfully via Resend to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Resend to {Email}. Exception: {ExceptionMessage}", toEmail, ex.Message);
            throw; // Re-throw so we can see the actual error
        }
    }
}
