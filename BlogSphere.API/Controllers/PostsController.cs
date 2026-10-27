
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
    public class PostsController : ControllerBase
    {
        private readonly BlogSphereDbContext _context;

        public PostsController(BlogSphereDbContext context)
        {
            _context = context;
        }

        // GET: api/Posts/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid post ID.");
            }

            var post = await _context.Posts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            return Ok(post);
        }

        // GET: api/Posts
        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.Posts
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(posts);
        }

        // POST: api/Posts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(
            [FromBody] Post post)
        {
            if (post == null)
            {
                return BadRequest("Post data is required.");
            }

            if (string.IsNullOrWhiteSpace(post.Title))
            {
                return BadRequest("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(post.Content))
            {
                return BadRequest("Content is required.");
            }

            post.Title = post.Title.Trim();
            post.Content = post.Content.Trim();

            if (post.Title.Length > 200)
            {
                return BadRequest(
                    "Title cannot exceed 200 characters.");
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            var authorExists = await _context.Users
                .AnyAsync(u => u.Id == currentUserId);

            if (!authorExists)
            {
                return Unauthorized();
            }

            post.Id = 0;
            post.AuthorId = currentUserId;
            post.CreatedAt = DateTime.UtcNow;
            post.UpdatedAt = null;
            post.Author = null;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetPostById),
                new { id = post.Id },
                post);
        }

        // PUT: api/Posts/{id}
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePost(
            int id,
            [FromBody] Post updatedPost)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid post ID.");
            }

            if (updatedPost == null)
            {
                return BadRequest("Post data is required.");
            }

            if (updatedPost.Id != 0 &&
                id != updatedPost.Id)
            {
                return BadRequest(
                    "Post ID in URL and body must match.");
            }

            if (string.IsNullOrWhiteSpace(updatedPost.Title))
            {
                return BadRequest("Title is required.");
            }

            if (string.IsNullOrWhiteSpace(updatedPost.Content))
            {
                return BadRequest("Content is required.");
            }

            updatedPost.Title = updatedPost.Title.Trim();
            updatedPost.Content = updatedPost.Content.Trim();

            if (updatedPost.Title.Length > 200)
            {
                return BadRequest(
                    "Title cannot exceed 200 characters.");
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            if (post.AuthorId != currentUserId)
            {
                return Forbid();
            }

            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // DELETE: api/Posts/{id}
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid post ID.");
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            if (post.AuthorId != currentUserId)
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
