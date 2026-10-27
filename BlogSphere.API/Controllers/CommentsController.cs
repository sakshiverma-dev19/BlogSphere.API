
using System.Security.Claims;
using BlogSphere.API.Data;
using BlogSphere.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly BlogSphereDbContext _context;

        public CommentsController(BlogSphereDbContext context)
        {
            _context = context;
        }

        // GET: api/Comments/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid comment ID.");
            }

            var comment = await _context.Comments
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }

            return Ok(comment);
        }

        // GET: api/Comments
        // GET: api/Comments?post_id=1
        [HttpGet]
        public async Task<IActionResult> GetComments(
            [FromQuery(Name = "post_id")] int? postId)
        {
            if (postId.HasValue && postId.Value <= 0)
            {
                return BadRequest("Invalid post ID.");
            }

            var query = _context.Comments.AsNoTracking();

            if (postId.HasValue)
            {
                var postExists = await _context.Posts
                    .AnyAsync(p => p.Id == postId.Value);

                if (!postExists)
                {
                    return NotFound("Post not found.");
                }

                query = query.Where(c => c.PostId == postId.Value);
            }

            var comments = await query
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return Ok(comments);
        }

        // POST: api/Comments
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateComment(
            [FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest("Comment data is required.");
            }

            if (string.IsNullOrWhiteSpace(comment.Content))
            {
                return BadRequest("Comment content is required.");
            }

            if (comment.Content.Length > 1000)
            {
                return BadRequest(
                    "Comment content cannot exceed 1000 characters.");
            }

            if (comment.PostId <= 0)
            {
                return BadRequest("A valid post ID is required.");
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            var postExists = await _context.Posts
                .AnyAsync(p => p.Id == comment.PostId);

            if (!postExists)
            {
                return BadRequest("Post does not exist.");
            }

            var authorExists = await _context.Users
                .AnyAsync(u => u.Id == currentUserId);

            if (!authorExists)
            {
                return Unauthorized();
            }

            // Server controls these values.
            comment.Id = 0;
            comment.AuthorId = currentUserId;
            comment.CreatedAt = DateTime.UtcNow;
            comment.UpdatedAt = null;
            comment.Post = null;
            comment.Author = null;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCommentById),
                new { id = comment.Id },
                comment);
        }

        // PUT: api/Comments/{id}
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateComment(
            int id,
            [FromBody] Comment updatedComment)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid comment ID.");
            }

            if (updatedComment == null)
            {
                return BadRequest("Comment data is required.");
            }

            if (updatedComment.Id != 0 &&
                id != updatedComment.Id)
            {
                return BadRequest(
                    "Comment ID in URL and body must match.");
            }

            if (string.IsNullOrWhiteSpace(updatedComment.Content))
            {
                return BadRequest("Comment content is required.");
            }

            if (updatedComment.Content.Length > 1000)
            {
                return BadRequest(
                    "Comment content cannot exceed 1000 characters.");
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            if (comment.AuthorId != currentUserId)
            {
                return Forbid();
            }

            // Only content can be changed.
            // PostId, AuthorId and CreatedAt remain unchanged.
            comment.Content = updatedComment.Content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        // DELETE: api/Comments/{id}
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid comment ID.");
            }

            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            if (comment.AuthorId != currentUserId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
