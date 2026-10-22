using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BlogSphere.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSphere.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly BlogSphereDbContext _context;

        public CommentsController(BlogSphereDbContext context)
        {
            _context = context;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }

            return Ok(comment);
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int? post_id)
        {
            var comments = await _context.Comments
                .Where(c => post_id == null || c.PostId == post_id)
                .ToListAsync();

            return Ok(comments);
        }
        [HttpPost]
        public async Task<IActionResult> CreateComment(
            BlogSphere.API.Models.Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Content))
            {
                return BadRequest("Comment content is required.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

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
                return BadRequest("Logged-in user does not exist.");
            }

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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(
    int id,
    BlogSphere.API.Models.Comment updatedComment)
        {
            if (id != updatedComment.Id && updatedComment.Id != 0)
            {
                return BadRequest("Comment ID in URL and body must match.");
            }

            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }


            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userIdClaim.Value);

            if (comment.AuthorId != currentUserId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(updatedComment.Content))
            {
                return BadRequest("Comment content is required.");
            }

            comment.Content = updatedComment.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(comment);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound("Comment not found.");
            }


            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int currentUserId = int.Parse(userIdClaim.Value);

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
