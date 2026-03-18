using Bedazzled.Application.Models;

namespace Bedazzled.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<Booking?> GetBookingByIdAsync(string id);
    Task<string> CreateBookingAsync(Booking booking);
    Task DeleteBookingAsync(string id);
}

public interface IContactService
{
    Task SendMessageAsync(ContactMessage message);
}

public interface IReviewService
{
    Task<IEnumerable<Review>> GetAllReviewsAsync();
    Task CreateReviewAsync(Review review);
}
