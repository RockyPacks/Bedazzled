using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Models;

namespace Bedazzled.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
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
        return booking.Id;
    }

    public async Task DeleteBookingAsync(string id)
    {
        await _repository.DeleteAsync(id);
    }
}

public class ContactService : IContactService
{
    private readonly IContactRepository _repository;

    public ContactService(IContactRepository repository)
    {
        _repository = repository;
    }

    public async Task SendMessageAsync(ContactMessage message)
    {
        await _repository.AddAsync(message);
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
