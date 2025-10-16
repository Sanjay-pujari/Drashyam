using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Drashyam.API.DTOs;
using Drashyam.API.Services;

namespace Drashyam.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentController> _logger;

    public CommentController(ICommentService commentService, ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CommentDto>> Add([FromBody] CommentCreateDto createDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var comment = await _commentService.AddCommentAsync(createDto, userId);
        return Ok(comment);
    }

    [HttpPut("{commentId:int}")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> Update([FromRoute] int commentId, [FromBody] CommentUpdateDto updateDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var comment = await _commentService.UpdateCommentAsync(commentId, updateDto, userId);
        return Ok(comment);
    }

    [HttpDelete("{commentId:int}")]
    [Authorize]
    public async Task<IActionResult> Delete([FromRoute] int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _commentService.DeleteCommentAsync(commentId, userId);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpGet("video/{videoId:int}")]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetVideoComments([FromRoute] int videoId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var comments = await _commentService.GetVideoCommentsAsync(videoId, page, pageSize);
        return Ok(comments);
    }

    [HttpGet("{parentCommentId:int}/replies")]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetReplies([FromRoute] int parentCommentId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var comments = await _commentService.GetCommentRepliesAsync(parentCommentId, page, pageSize);
        return Ok(comments);
    }

    [HttpPost("{commentId:int}/like")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> Like([FromRoute] int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _commentService.LikeCommentAsync(commentId, userId);
        return Ok(result);
    }

    [HttpPost("{commentId:int}/unlike")]
    [Authorize]
    public async Task<ActionResult<CommentDto>> Unlike([FromRoute] int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _commentService.UnlikeCommentAsync(commentId, userId);
        return Ok(result);
    }

    [HttpGet("{commentId:int}/is-liked")]
    [Authorize]
    public async Task<ActionResult<bool>> IsLiked([FromRoute] int commentId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _commentService.IsCommentLikedAsync(commentId, userId);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<PagedResult<CommentDto>>> GetMyComments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var comments = await _commentService.GetUserCommentsAsync(userId, page, pageSize);
        return Ok(comments);
    }

    [HttpPost("{commentId:int}/report")]
    [Authorize]
    public async Task<ActionResult<bool>> Report([FromRoute] int commentId, [FromQuery] string reason)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _commentService.ReportCommentAsync(commentId, userId, reason);
        return Ok(result);
    }
}


