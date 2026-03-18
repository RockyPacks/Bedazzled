using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bedazzled.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
    {
        _logger.LogInformation("Fetching all bookings");
        var bookings = await _bookingService.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpPost]
    public async Task<ActionResult<Booking>> CreateBooking(Booking booking)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating a new booking for {Name}", booking.Name);
        var id = await _bookingService.CreateBookingAsync(booking);
        return CreatedAtAction(nameof(GetBookings), new { id = id }, booking);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBooking(string id)
    {
        _logger.LogInformation("Deleting booking with id {Id}", id);
        await _bookingService.DeleteBookingAsync(id);
        return NoContent();
    }
}
