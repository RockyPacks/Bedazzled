using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Models;
using Microsoft.Extensions.Logging;

namespace Bedazzled.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;
    private const string AdminEmail = "rockychueu21@gmail.com";

    public BookingService(
        IBookingRepository repository,
        IEmailService emailService,
        ILogger<BookingService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Booking?> GetBookingByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<string> CreateBookingAsync(Booking booking)
    {
        await _repository.AddAsync(booking);

        await TrySendEmailAsync(
            booking.Email,
            "Magic Booking Confirmed! - Bedazzled",
            $"<h1>Hi {booking.Name}!</h1><p>Your booking for a <strong>{booking.EventType}</strong> on {booking.EventDate:MMM dd, yyyy} has been received.</p><p>We will contact you shortly to finalize the details.</p><p>Stay Sparkly!<br/>The Bedazzled Team</p>");

        await TrySendEmailAsync(
            AdminEmail,
            "New Magic Booking Request!",
            $"<h1>New Booking Alert!</h1><p><strong>Client:</strong> {booking.Name} ({booking.Email})</p><p><strong>Event:</strong> {booking.EventType}</p><p><strong>Date:</strong> {booking.EventDate:MMM dd, yyyy}</p><p><strong>Message:</strong> {booking.Message}</p>");

        return booking.Id;
    }

    private async Task TrySendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Booking email delivery failed for {Email} with subject {Subject}", toEmail, subject);
        }
    }

    public async Task DeleteBookingAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task<AdminAnalytics> GetAnalyticsAsync()
    {
        var bookings = await _repository.GetAllAsync();
        var analytics = new AdminAnalytics
        {
            TotalBookings = bookings.Count(),
            PopularEventTypes = bookings.GroupBy(b => b.EventType)
                                       .ToDictionary(g => g.Key, g => g.Count()),
            MonthlyTrends = bookings.GroupBy(b => new { b.EventDate.Year, b.EventDate.Month })
                                   .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                   .Select(g => new MonthlyTrend
                                   {
                                       Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                                       Count = g.Count()
                                   }).ToList()
        };
        return analytics;
    }
}

public class ContactService : IContactService
{
    private readonly IContactRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger<ContactService> _logger;
    private const string AdminEmail = "rockychueu21@gmail.com";

    public ContactService(
        IContactRepository repository,
        IEmailService emailService,
        ILogger<ContactService> logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendMessageAsync(ContactMessage message)
    {
        await _repository.AddAsync(message);

        await TrySendEmailAsync(
            AdminEmail,
            $"New Contact Message: {message.Subject}",
            $"<h1>New Contact Message</h1><p><strong>Name:</strong> {message.Name}</p><p><strong>Email:</strong> {message.Email}</p><p><strong>Subject:</strong> {message.Subject}</p><p><strong>Message:</strong><br/>{message.Message}</p>");

        await TrySendEmailAsync(
            message.Email,
            "We received your message - Bedazzled",
            $"<h1>Hi {message.Name}!</h1><p>Thanks for reaching out to Bedazzled.</p><p>We received your message about <strong>{message.Subject}</strong> and will get back to you soon.</p><p>Stay Sparkly!<br/>The Bedazzled Team</p>");
    }

    private async Task TrySendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            await _emailService.SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Contact email delivery failed for {Email} with subject {Subject}", toEmail, subject);
        }
    }
}

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repository;

    public ReviewService(IReviewRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Review>> GetAllReviewsAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task CreateReviewAsync(Review review)
    {
        await _repository.AddAsync(review);
    }
}
