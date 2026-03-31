using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Models;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;

namespace Bedazzled.Infrastructure.Repositories;

public class FirestoreBookingRepository : IBookingRepository
{
    private readonly FirestoreDb _db;
    private readonly ILogger<FirestoreBookingRepository> _logger;
    private const string CollectionName = "bookings";

    public FirestoreBookingRepository(FirestoreDb db, ILogger<FirestoreBookingRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<Booking>> GetAllAsync()
    {
        _logger.LogInformation("Attempting to get snapshot from Firestore collection: {Collection}", CollectionName);
        try 
        {
            var snapshot = await _db.Collection(CollectionName).GetSnapshotAsync();
            _logger.LogInformation("Successfully retrieved {Count} documents from Firestore", snapshot.Documents.Count);
            return snapshot.Documents.Select(doc => 
            {
                var booking = doc.ConvertTo<Booking>();
                booking.Id = doc.Id;
                return booking;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching data from Firestore");
            throw;
        }
    }

    public async Task<Booking?> GetByIdAsync(string id)
    {
        var doc = await _db.Collection(CollectionName).Document(id).GetSnapshotAsync();
        if (!doc.Exists) return null;
        
        var booking = doc.ConvertTo<Booking>();
        booking.Id = doc.Id;
        return booking;
    }

    public async Task AddAsync(Booking booking)
    {
        if (booking.EventDate.Kind == DateTimeKind.Unspecified)
        {
            booking.EventDate = DateTime.SpecifyKind(booking.EventDate, DateTimeKind.Utc);
        }
        else if (booking.EventDate.Kind == DateTimeKind.Local)
        {
            booking.EventDate = booking.EventDate.ToUniversalTime();
        }
        
        var docRef = _db.Collection(CollectionName).Document();
        await docRef.SetAsync(booking);
        booking.Id = docRef.Id;
    }

    public async Task DeleteAsync(string id)
    {
        await _db.Collection(CollectionName).Document(id).DeleteAsync();
    }
}

public class FirestoreContactRepository : IContactRepository
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "contact_messages";

    public FirestoreContactRepository(FirestoreDb db)
    {
        _db = db;
    }

    public async Task AddAsync(ContactMessage message)
    {
        var docRef = _db.Collection(CollectionName).Document();
        await docRef.SetAsync(message);
        message.Id = docRef.Id;
    }
}

public class FirestoreReviewRepository : IReviewRepository
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "reviews";

    public FirestoreReviewRepository(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Review>> GetAllAsync()
    {
        var snapshot = await _db.Collection(CollectionName).OrderByDescending("Date").GetSnapshotAsync();
        return snapshot.Documents.Select(doc => 
        {
            var review = doc.ConvertTo<Review>();
            review.Id = doc.Id;
            return review;
        });
    }

    public async Task AddAsync(Review review)
    {
        if (review.Date.Kind == DateTimeKind.Unspecified)
        {
            review.Date = DateTime.SpecifyKind(review.Date, DateTimeKind.Utc);
        }
        else if (review.Date.Kind == DateTimeKind.Local)
        {
            review.Date = review.Date.ToUniversalTime();
        }

        var docRef = _db.Collection(CollectionName).Document();
        await docRef.SetAsync(review);
        review.Id = docRef.Id;
    }
}
