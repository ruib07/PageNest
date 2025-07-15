using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PageNest.Application.Constants;
using PageNest.Application.Interfaces.Services;
using PageNest.Application.Shared.DTOs;
using PageNest.Domain.Entities;

namespace PageNest.API.Controllers;

[Route($"api/{AppSettings.ApiVersion}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewsService _reviewsService;

    public ReviewsController(IReviewsService reviewsService)
    {
        _reviewsService = reviewsService;
    }

    // GET api/v1/reviews
    [Authorize(Policy = AppSettings.AdminRole)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
    {
        return Ok(await _reviewsService.GetReviews());
    }

    // GET api/v1/reviews/user/{userId}
    [Authorize]
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByUserId(Guid userId)
    {
        return Ok(await _reviewsService.GetReviewsByUserId(userId));
    }

    // GET api/v1/reviews/book/{bookId}
    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<Review>>> GetReviewsByBookId(Guid bookId)
    {
        return Ok(await _reviewsService.GetReviewsByBookId(bookId));
    }

    // GET api/v1/reviews/{reviewId}
    [Authorize]
    [HttpGet("{reviewId}")]
    public async Task<IActionResult> GetReviewById(Guid reviewId)
    {
        var result = await _reviewsService.GetReviewById(reviewId);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        return Ok(result.Data);
    }

    // POST api/v1/reviews
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] Review review)
    {
        var result = await _reviewsService.CreateReview(review);

        if (!result.IsSuccess) return StatusCode(result.Error.StatusCode, result.Error);

        var response = new ResponsesDTO.Creation(result.Message, result.Data.Id);

        return CreatedAtAction(nameof(GetReviewById), new { reviewId = result.Data.Id }, response);
    }

    // DELETE api/v1/reviews/{reviewId}
    [Authorize]
    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(Guid reviewId)
    {
        await _reviewsService.DeleteReview(reviewId);

        return NoContent();
    }
}
