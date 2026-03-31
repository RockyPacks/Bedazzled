using Bedazzled.Api.Infrastructure;
using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bedazzled.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[FirebaseAdminAuthorize]
public class AdminController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IBookingService bookingService, IEmailService emailService, ILogger<AdminController> logger)
    {
        _bookingService = bookingService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AdminAnalytics>> GetAnalytics()
    {
        var analytics = await _bookingService.GetAnalyticsAsync();
        return Ok(analytics);
    }

    [HttpPost("test-email")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail = "test@example.com")
    {
        try
        {
            _logger.LogInformation("Sending test email to {Email}", toEmail);
            await _emailService.SendEmailAsync(
                toEmail,
                "Test Email from Bedazzled API",
                "<h1>Test Email</h1><p>If you're reading this, the email service is working!</p>");
            return Ok(new { Message = "Test email sent successfully", Email = toEmail });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email");
            return StatusCode(500, new { Message = "Failed to send test email", Error = ex.Message, InnerError = ex.InnerException?.Message });
        }
    }
}
