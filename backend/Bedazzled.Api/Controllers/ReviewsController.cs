using Bedazzled.Application.Interfaces;
using Bedazzled.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bedazzled.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;
    private readonly ILogger<ReviewsController> _logger;

    public ReviewsController(IReviewService reviewService, ILogger<ReviewsController> logger)
    {
        _reviewService = reviewService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
    {
        _logger.LogInformation("Fetching all reviews");
        var reviews = await _reviewService.GetAllReviewsAsync();
        
        // If no reviews exist, potentially seed some or return empty
        if (!reviews.Any())
        {
            _logger.LogInformation("No reviews found in database.");
        }
        
        return Ok(reviews);
    }

    [HttpPost]
    public async Task<ActionResult<Review>> CreateReview(Review review)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _logger.LogInformation("Creating a new review from {Author}", review.Author);
        await _reviewService.CreateReviewAsync(review);
        return Ok(review);
    }
    
    [HttpPost("seed")]
    public async Task<IActionResult> SeedReviews()
    {
        var existing = await _reviewService.GetAllReviewsAsync();
        if (existing.Any()) return BadRequest("Reviews already exist.");

        var initialReviews = new List<Review>
        {
            new Review { 
                Author = "Jessica L.", 
                Subtitle = "Mother of 2",
                Content = "Bedazzled Facepainting was a huge hit at our daughter's birthday party! The designs were so beautiful and detailed, and the kids were thrilled. Highly recommend!",
                Rating = 5,
                Date = DateTime.UtcNow.AddMonths(-1)
            },
            new Review { 
                Author = "Mark R.", 
                Subtitle = "Corporate Event Plannner",
                Content = "We had Bedazzled Facepainting at our company's family day, and they were amazing! The artists were professional and handled the large crowd with ease.",
                Rating = 5,
                Date = DateTime.UtcNow.AddMonths(-2)
            },
            new Review { 
                Author = "Emily W.", 
                Subtitle = "Festival Coordinator",
                Content = "Thank you, Bedazzled Facepainting, for making our festival so colorful and fun! The kids (and even kids at heart!) loved the designs. Excellent service!",
                Rating = 5,
                Date = DateTime.UtcNow.AddMonths(-3)
            }
        };

        foreach (var review in initialReviews)
        {
            await _reviewService.CreateReviewAsync(review);
        }

        return Ok("Seeded 3 reviews.");
    }
}
