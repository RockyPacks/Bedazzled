using Bedazzled.Application.Models;

namespace Bedazzled.Application.Interfaces;

public interface IBookingRepository
{
    Task<IEnumerable<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(string id);
    Task AddAsync(Booking booking);
    Task DeleteAsync(string id);
}

public interface IContactRepository
{
    Task AddAsync(ContactMessage message);
}

public interface IReviewRepository
{
    Task<IEnumerable<Review>> GetAllAsync();
    Task AddAsync(Review review);
}
