using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChallengeController : ControllerBase
{
    private readonly IChallengeService _challengeService;
    private readonly ILogger<ChallengeController> _logger;

    public ChallengeController(IChallengeService challengeService, ILogger<ChallengeController> logger)
    {
        _challengeService = challengeService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ChallengeDto>> CreateChallenge([FromBody] ChallengeCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var challenge = await _challengeService.CreateChallengeAsync(createDto, userId);
            return CreatedAtAction(nameof(GetChallenge), new { id = challenge.Id }, challenge);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating challenge");
            return StatusCode(500, "An error occurred while creating the challenge");
        }
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ChallengeDto>> GetChallenge([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var challenge = await _challengeService.GetChallengeByIdAsync(id, userId);
            return Ok(challenge);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while retrieving the challenge");
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ChallengeDto>>> GetChallenges([FromQuery] ChallengeFilterDto filter)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var challenges = await _challengeService.GetChallengesAsync(filter, userId);
            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenges");
            return StatusCode(500, "An error occurred while retrieving challenges");
        }
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<PagedResult<ChallengeDto>>> GetMyChallenges([FromQuery] ChallengeFilterDto filter)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var challenges = await _challengeService.GetUserChallengesAsync(userId, filter);
            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user challenges");
            return StatusCode(500, "An error occurred while retrieving user challenges");
        }
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<ChallengeDto>> UpdateChallenge([FromRoute] int id, [FromBody] ChallengeUpdateDto updateDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var challenge = await _challengeService.UpdateChallengeAsync(id, updateDto, userId);
            return Ok(challenge);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while updating the challenge");
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteChallenge([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.DeleteChallengeAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Challenge deleted successfully" });
            }
            
            return BadRequest("Failed to delete challenge");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while deleting the challenge");
        }
    }

    [HttpPost("{id:int}/publish")]
    [Authorize]
    public async Task<ActionResult> PublishChallenge([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.PublishChallengeAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Challenge published successfully" });
            }
            
            return BadRequest("Failed to publish challenge");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while publishing the challenge");
        }
    }

    [HttpPost("{id:int}/cancel")]
    [Authorize]
    public async Task<ActionResult> CancelChallenge([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.CancelChallengeAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Challenge cancelled successfully" });
            }
            
            return BadRequest("Failed to cancel challenge");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while cancelling the challenge");
        }
    }

    [HttpPost("{id:int}/join")]
    [Authorize]
    public async Task<ActionResult> JoinChallenge([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.JoinChallengeAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Successfully joined the challenge" });
            }
            
            return BadRequest("Failed to join challenge");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while joining the challenge");
        }
    }

    [HttpPost("{id:int}/leave")]
    [Authorize]
    public async Task<ActionResult> LeaveChallenge([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.LeaveChallengeAsync(id, userId);
            
            if (success)
            {
                return Ok(new { message = "Successfully left the challenge" });
            }
            
            return BadRequest("Failed to leave challenge");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while leaving the challenge");
        }
    }

    [HttpGet("{id:int}/participants")]
    public async Task<ActionResult<PagedResult<ChallengeParticipantDto>>> GetParticipants([FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var participants = await _challengeService.GetChallengeParticipantsAsync(id, page, pageSize);
            return Ok(participants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenge participants for {ChallengeId}", id);
            return StatusCode(500, "An error occurred while retrieving participants");
        }
    }

    [HttpPost("submissions")]
    [Authorize]
    public async Task<ActionResult<ChallengeSubmissionDto>> SubmitToChallenge([FromBody] ChallengeSubmissionCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var submission = await _challengeService.SubmitToChallengeAsync(createDto, userId);
            return CreatedAtAction(nameof(GetSubmission), new { id = submission.Id }, submission);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting to challenge");
            return StatusCode(500, "An error occurred while submitting to the challenge");
        }
    }

    [HttpGet("submissions/{id:int}")]
    public async Task<ActionResult<ChallengeSubmissionDto>> GetSubmission([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var submission = await _challengeService.GetSubmissionByIdAsync(id, userId);
            return Ok(submission);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting submission {SubmissionId}", id);
            return StatusCode(500, "An error occurred while retrieving the submission");
        }
    }

    [HttpGet("{id:int}/submissions")]
    public async Task<ActionResult<PagedResult<ChallengeSubmissionDto>>> GetChallengeSubmissions([FromRoute] int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var submissions = await _challengeService.GetChallengeSubmissionsAsync(id, page, pageSize, userId);
            return Ok(submissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenge submissions for {ChallengeId}", id);
            return StatusCode(500, "An error occurred while retrieving submissions");
        }
    }

    [HttpPost("submissions/{submissionId:int}/approve")]
    [Authorize]
    public async Task<ActionResult> ApproveSubmission([FromRoute] int submissionId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.ApproveSubmissionAsync(submissionId, userId);
            
            if (success)
            {
                return Ok(new { message = "Submission approved successfully" });
            }
            
            return BadRequest("Failed to approve submission");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving submission {SubmissionId}", submissionId);
            return StatusCode(500, "An error occurred while approving the submission");
        }
    }

    [HttpPost("submissions/{submissionId:int}/reject")]
    [Authorize]
    public async Task<ActionResult> RejectSubmission([FromRoute] int submissionId, [FromBody] string reason)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.RejectSubmissionAsync(submissionId, userId, reason);
            
            if (success)
            {
                return Ok(new { message = "Submission rejected successfully" });
            }
            
            return BadRequest("Failed to reject submission");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting submission {SubmissionId}", submissionId);
            return StatusCode(500, "An error occurred while rejecting the submission");
        }
    }

    [HttpDelete("submissions/{submissionId:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteSubmission([FromRoute] int submissionId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.DeleteSubmissionAsync(submissionId, userId);
            
            if (success)
            {
                return Ok(new { message = "Submission deleted successfully" });
            }
            
            return BadRequest("Failed to delete submission");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting submission {SubmissionId}", submissionId);
            return StatusCode(500, "An error occurred while deleting the submission");
        }
    }

    [HttpPost("votes")]
    [Authorize]
    public async Task<ActionResult> VoteOnSubmission([FromBody] ChallengeVoteCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.VoteOnSubmissionAsync(createDto, userId);
            
            if (success)
            {
                return Ok(new { message = "Vote recorded successfully" });
            }
            
            return BadRequest("Failed to record vote");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error voting on submission");
            return StatusCode(500, "An error occurred while recording the vote");
        }
    }

    [HttpDelete("votes")]
    [Authorize]
    public async Task<ActionResult> RemoveVote([FromQuery] int challengeId, [FromQuery] int submissionId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.RemoveVoteAsync(challengeId, submissionId, userId);
            
            if (success)
            {
                return Ok(new { message = "Vote removed successfully" });
            }
            
            return BadRequest("Failed to remove vote");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing vote");
            return StatusCode(500, "An error occurred while removing the vote");
        }
    }

    [HttpGet("{id:int}/top-submissions")]
    public async Task<ActionResult<List<ChallengeSubmissionDto>>> GetTopSubmissions([FromRoute] int id, [FromQuery] int count = 10)
    {
        try
        {
            var submissions = await _challengeService.GetTopSubmissionsAsync(id, count);
            return Ok(submissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top submissions for challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while retrieving top submissions");
        }
    }

    [HttpPost("comments")]
    [Authorize]
    public async Task<ActionResult<ChallengeCommentDto>> AddComment([FromBody] ChallengeCommentCreateDto createDto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var comment = await _challengeService.AddCommentAsync(createDto, userId);
            return CreatedAtAction(nameof(GetComments), new { challengeId = createDto.ChallengeId }, comment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            return StatusCode(500, "An error occurred while adding the comment");
        }
    }

    [HttpGet("{challengeId:int}/comments")]
    public async Task<ActionResult<PagedResult<ChallengeCommentDto>>> GetComments([FromRoute] int challengeId, [FromQuery] int? submissionId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var comments = await _challengeService.GetCommentsAsync(challengeId, submissionId, page, pageSize);
            return Ok(comments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments for challenge {ChallengeId}", challengeId);
            return StatusCode(500, "An error occurred while retrieving comments");
        }
    }

    [HttpPost("comments/{commentId:int}/like")]
    [Authorize]
    public async Task<ActionResult> LikeComment([FromRoute] int commentId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.LikeCommentAsync(commentId, userId);
            
            if (success)
            {
                return Ok(new { message = "Comment liked successfully" });
            }
            
            return BadRequest("Failed to like comment");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error liking comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while liking the comment");
        }
    }

    [HttpDelete("comments/{commentId:int}/like")]
    [Authorize]
    public async Task<ActionResult> UnlikeComment([FromRoute] int commentId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.UnlikeCommentAsync(commentId, userId);
            
            if (success)
            {
                return Ok(new { message = "Comment unliked successfully" });
            }
            
            return BadRequest("Failed to unlike comment");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unliking comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while unliking the comment");
        }
    }

    [HttpDelete("comments/{commentId:int}")]
    [Authorize]
    public async Task<ActionResult> DeleteComment([FromRoute] int commentId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.DeleteCommentAsync(commentId, userId);
            
            if (success)
            {
                return Ok(new { message = "Comment deleted successfully" });
            }
            
            return BadRequest("Failed to delete comment");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while deleting the comment");
        }
    }

    [HttpPost("comments/{commentId:int}/pin")]
    [Authorize]
    public async Task<ActionResult> PinComment([FromRoute] int commentId)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var success = await _challengeService.PinCommentAsync(commentId, userId);
            
            if (success)
            {
                return Ok(new { message = "Comment pin status updated successfully" });
            }
            
            return BadRequest("Failed to update comment pin status");
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pinning comment {CommentId}", commentId);
            return StatusCode(500, "An error occurred while updating the comment pin status");
        }
    }

    [HttpGet("trending")]
    public async Task<ActionResult<List<ChallengeDto>>> GetTrendingChallenges([FromQuery] int count = 10)
    {
        try
        {
            var challenges = await _challengeService.GetTrendingChallengesAsync(count);
            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending challenges");
            return StatusCode(500, "An error occurred while retrieving trending challenges");
        }
    }

    [HttpGet("recommended")]
    [Authorize]
    public async Task<ActionResult<List<ChallengeDto>>> GetRecommendedChallenges([FromQuery] int count = 10)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var challenges = await _challengeService.GetRecommendedChallengesAsync(userId, count);
            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended challenges");
            return StatusCode(500, "An error occurred while retrieving recommended challenges");
        }
    }

    [HttpGet("hashtag/{hashtag}")]
    public async Task<ActionResult<List<ChallengeDto>>> GetChallengesByHashtag([FromRoute] string hashtag, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var challenges = await _challengeService.GetChallengesByHashtagAsync(hashtag, page, pageSize);
            return Ok(challenges);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenges by hashtag {Hashtag}", hashtag);
            return StatusCode(500, "An error occurred while retrieving challenges by hashtag");
        }
    }

    [HttpGet("analytics")]
    [Authorize]
    public async Task<ActionResult<ChallengeAnalyticsDto>> GetAnalytics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var analytics = await _challengeService.GetChallengeAnalyticsAsync(userId, startDate, endDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting challenge analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }

    [HttpGet("{id:int}/analytics")]
    [Authorize]
    public async Task<ActionResult<ChallengeAnalyticsDto>> GetChallengeAnalytics([FromRoute] int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var analytics = await _challengeService.GetChallengeAnalyticsByIdAsync(id, userId);
            return Ok(analytics);
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting analytics for challenge {ChallengeId}", id);
            return StatusCode(500, "An error occurred while retrieving challenge analytics");
        }
    }
}
