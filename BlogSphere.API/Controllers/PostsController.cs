using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using BlogSphere.API.Data;
using BlogSphere.API.Models;
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
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _context.Posts
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
            var posts = await _context.Posts.ToListAsync();

            return Ok(posts);
        }

        // POST: api/Posts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(Post post)
        {
            if (post == null ||
                string.IsNullOrWhiteSpace(post.Title) ||
                string.IsNullOrWhiteSpace(post.Content))
            {
                return BadRequest("Title and Content are required.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            var authorExists = await _context.Users
                .AnyAsync(u => u.Id == currentUserId);

            if (!authorExists)
            {
                return BadRequest("Logged-in user does not exist.");
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(
            int id,
            Post updatedPost)
        {
            if (updatedPost == null)
            {
                return BadRequest("Post data is required.");
            }

            if (id != updatedPost.Id && updatedPost.Id != 0)
            {
                return BadRequest(
                    "Post ID in URL and body must match.");
            }

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null ||
                !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return Unauthorized();
            }

            if (post.AuthorId != currentUserId)
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(updatedPost.Title) ||
                string.IsNullOrWhiteSpace(updatedPost.Content))
            {
                return BadRequest("Title and Content are required.");
            }

            post.Title = updatedPost.Title;
            post.Content = updatedPost.Content;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // DELETE: api/Posts/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound("Post not found.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

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