
using BlogSphere.API.Data;
using BlogSphere.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace BlogSphere.API.Tests
{
    public class BlogSphereIntegrationTests
        : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public BlogSphereIntegrationTests(
            WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<BlogSphereDbContext>>();

                    services.AddDbContext<BlogSphereDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("BlogSphereTestDb");
                    });
                });
            });
        }

        [Fact]
        public async Task Api_ShouldStartSuccessfully()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger/index.html");

            Assert.True(
                response.IsSuccessStatusCode ||
                response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task PostsEndpoint_WithoutToken_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Posts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CommentsEndpoint_WithoutToken_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Comments");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostsEndpoint_InvalidRequestWithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Posts",
                new { title = "", content = "" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreatePost_EmptyTitleAndContent_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            string email = "sakshi_test@example.com";
            string password = "TestPass123!";

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider
                    .GetRequiredService<BlogSphereDbContext>();

                var user = await db.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    user = new User
                    {
                        Username = "sakshi_test",
                        Email = email
                    };

                    db.Users.Add(user);
                }

                var passwordHasher = new PasswordHasher<User>();
                user.PasswordHash = passwordHasher.HashPassword(user, password);

                await db.SaveChangesAsync();
            }

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new { email, passwordHash = password });

            string loginBody = await loginResponse.Content.ReadAsStringAsync();

            Assert.True(
                loginResponse.IsSuccessStatusCode,
                $"Login failed. Status: {(int)loginResponse.StatusCode}. Response: {loginBody}");

            using var loginDocument = JsonDocument.Parse(loginBody);

            var tokenProperty = loginDocument.RootElement
                .EnumerateObject()
                .FirstOrDefault(property =>
                    string.Equals(
                        property.Name,
                        "token",
                        StringComparison.OrdinalIgnoreCase));

            Assert.True(
                tokenProperty.Value.ValueKind == JsonValueKind.String,
                $"Login response does not contain a valid token: {loginBody}");

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    tokenProperty.Value.GetString()!);

            var response = await client.PostAsJsonAsync(
                "/api/Posts",
                new { title = "", content = "" });

            string responseBody = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.BadRequest,
                $"Expected 400 Bad Request, but received {(int)response.StatusCode}. Response: {responseBody}");
        }

        [Fact]
        public async Task GetPostById_PostDoesNotExist_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Posts/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreatePost_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Posts",
                new { title = "Test Post", content = "Test Content" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetCommentById_CommentDoesNotExist_ReturnsNotFound()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Comments/99999");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task CreateComment_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Comments",
                new { postId = 1, content = "Test comment" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetComments_WithPostId_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(
                "/api/Comments?post_id=1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GetComments_WithPostId_ReturnsOnlyMatchingComments()
        {
            var client = _factory.CreateClient();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<BlogSphereDbContext>();

            var user = new User
            {
                Username = "filter_test_user",
                Email = "filter_test@example.com",
                PasswordHash = "TestPassword123!"
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var post1 = new Post
            {
                Title = "First Post",
                Content = "First post content",
                AuthorId = user.Id
            };

            var post2 = new Post
            {
                Title = "Second Post",
                Content = "Second post content",
                AuthorId = user.Id
            };

            db.Posts.AddRange(post1, post2);
            await db.SaveChangesAsync();

            db.Comments.AddRange(
                new Comment
                {
                    PostId = post1.Id,
                    Content = "Comment for first post",
                    AuthorId = user.Id
                },
                new Comment
                {
                    PostId = post2.Id,
                    Content = "Comment for second post",
                    AuthorId = user.Id
                });

            await db.SaveChangesAsync();

            var response = await client.GetAsync(
                $"/api/Comments?post_id={post1.Id}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var comments = await response.Content
                .ReadFromJsonAsync<List<Comment>>();

            Assert.NotNull(comments);
            Assert.NotEmpty(comments);

            Assert.All(
                comments,
                comment => Assert.Equal(post1.Id, comment.PostId));
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsOk()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "new_test_user",
                    email = "new_test_user@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "missing_user@example.com",
                    passwordHash = "WrongPass123!"
                });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            var client = _factory.CreateClient();

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "login_test_user",
                    email = "login_test_user@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "login_test_user@example.com",
                    passwordHash = "TestPass123!"
                });

            var body = await loginResponse.Content.ReadAsStringAsync();

            Assert.True(
                loginResponse.StatusCode == HttpStatusCode.OK,
                $"Login failed: {body}");

            using var document = JsonDocument.Parse(body);

            var token = document.RootElement
                .GetProperty("token")
                .GetString();

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task CreatePost_WithValidToken_ReturnsCreated()
        {
            var client = _factory.CreateClient();

            string email = "create_post_test@example.com";
            string password = "TestPass123!";

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "create_post_test_user",
                    email,
                    passwordHash = password
                });

            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new { email, passwordHash = password });

            var loginBody = await loginResponse.Content.ReadAsStringAsync();

            Assert.True(loginResponse.IsSuccessStatusCode, loginBody);

            using var document = JsonDocument.Parse(loginBody);

            string token = document.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            var response = await client.PostAsJsonAsync(
                "/api/Posts",
                new
                {
                    title = "Integration Test Post",
                    content = "Created using a JWT token.",
                    authorId = 1
                });

            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.True(
                response.StatusCode == HttpStatusCode.Created,
                $"Expected 201 Created, got {(int)response.StatusCode}. Response: {responseBody}");
        }

        [Fact]
        public async Task UpdatePost_ByDifferentUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();

            var registerResponse = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "owner_user",
                    email = "owner_user@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "owner_user@example.com",
                    passwordHash = "TestPass123!"
                });

            var loginBody = await loginResponse.Content.ReadAsStringAsync();
            Assert.True(loginResponse.IsSuccessStatusCode, loginBody);

            using var loginDocument = JsonDocument.Parse(loginBody);

            string ownerToken = loginDocument.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", ownerToken);

            var createResponse = await client.PostAsJsonAsync(
                "/api/Posts",
                new
                {
                    title = "Owner Post",
                    content = "This post belongs to the first user.",
                    authorId = 1
                });

            var createBody = await createResponse.Content.ReadAsStringAsync();

            Assert.True(
                createResponse.StatusCode == HttpStatusCode.Created,
                $"Post creation failed: {createBody}");

            using var postDocument = JsonDocument.Parse(createBody);
            int postId = postDocument.RootElement.GetProperty("id").GetInt32();

            var secondRegister = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "other_user",
                    email = "other_user@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, secondRegister.StatusCode);

            var secondLogin = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "other_user@example.com",
                    passwordHash = "TestPass123!"
                });

            var secondBody = await secondLogin.Content.ReadAsStringAsync();
            Assert.True(secondLogin.IsSuccessStatusCode, secondBody);

            using var secondDocument = JsonDocument.Parse(secondBody);

            string secondToken = secondDocument.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", secondToken);

            var updateResponse = await client.PutAsJsonAsync(
                $"/api/Posts/{postId}",
                new
                {
                    id = postId,
                    title = "Changed by another user",
                    content = "This update should be rejected.",
                    authorId = 2
                });

            Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        }

        [Fact]
        public async Task DeletePost_ByDifferentUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();

            var register1 = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "delete_owner",
                    email = "delete_owner@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, register1.StatusCode);

            var login1 = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "delete_owner@example.com",
                    passwordHash = "TestPass123!"
                });

            var body1 = await login1.Content.ReadAsStringAsync();
            Assert.True(login1.IsSuccessStatusCode, body1);

            using var doc1 = JsonDocument.Parse(body1);

            string token1 = doc1.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token1);

            var create = await client.PostAsJsonAsync(
                "/api/Posts",
                new
                {
                    title = "Post to protect",
                    content = "Only its owner should delete it.",
                    authorId = 1
                });

            var createBody = await create.Content.ReadAsStringAsync();

            Assert.True(
                create.StatusCode == HttpStatusCode.Created,
                $"Post creation failed: {createBody}");

            using var postDoc = JsonDocument.Parse(createBody);
            int postId = postDoc.RootElement.GetProperty("id").GetInt32();

            var register2 = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "delete_other",
                    email = "delete_other@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, register2.StatusCode);

            var login2 = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "delete_other@example.com",
                    passwordHash = "TestPass123!"
                });

            var body2 = await login2.Content.ReadAsStringAsync();
            Assert.True(login2.IsSuccessStatusCode, body2);

            using var doc2 = JsonDocument.Parse(body2);

            string token2 = doc2.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token2);

            var deleteResponse = await client.DeleteAsync(
                $"/api/Posts/{postId}");

            Assert.Equal(HttpStatusCode.Forbidden, deleteResponse.StatusCode);
        }

        [Fact]
        public async Task UpdateComment_ByDifferentUser_ReturnsForbidden()
        {
            var client = _factory.CreateClient();

            var register1 = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "comment_owner",
                    email = "comment_owner@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, register1.StatusCode);

            var login1 = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "comment_owner@example.com",
                    passwordHash = "TestPass123!"
                });

            var body1 = await login1.Content.ReadAsStringAsync();
            Assert.True(login1.IsSuccessStatusCode, body1);

            using var doc1 = JsonDocument.Parse(body1);

            string token1 = doc1.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token1);

            var postResponse = await client.PostAsJsonAsync(
                "/api/Posts",
                new
                {
                    title = "Comment Test Post",
                    content = "Post for testing comment ownership.",
                    authorId = 1
                });

            var postBody = await postResponse.Content.ReadAsStringAsync();

            Assert.True(
                postResponse.StatusCode == HttpStatusCode.Created,
                postBody);

            using var postDoc = JsonDocument.Parse(postBody);
            int postId = postDoc.RootElement.GetProperty("id").GetInt32();

            var commentResponse = await client.PostAsJsonAsync(
                "/api/Comments",
                new
                {
                    postId,
                    content = "Original comment",
                    authorId = 1
                });

            var commentBody = await commentResponse.Content.ReadAsStringAsync();

            Assert.True(
                commentResponse.StatusCode == HttpStatusCode.Created,
                commentBody);

            using var commentDoc = JsonDocument.Parse(commentBody);
            int commentId = commentDoc.RootElement.GetProperty("id").GetInt32();

            var register2 = await client.PostAsJsonAsync(
                "/api/Auth/register",
                new
                {
                    username = "comment_other",
                    email = "comment_other@example.com",
                    passwordHash = "TestPass123!"
                });

            Assert.Equal(HttpStatusCode.OK, register2.StatusCode);

            var login2 = await client.PostAsJsonAsync(
                "/api/Auth/login",
                new
                {
                    email = "comment_other@example.com",
                    passwordHash = "TestPass123!"
                });

            var body2 = await login2.Content.ReadAsStringAsync();
            Assert.True(login2.IsSuccessStatusCode, body2);

            using var doc2 = JsonDocument.Parse(body2);

            string token2 = doc2.RootElement
                .GetProperty("token")
                .GetString()!;

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token2);

            var updateResponse = await client.PutAsJsonAsync(
                $"/api/Comments/{commentId}",
                new
                {
                    id = commentId,
                    postId,
                    content = "Changed by another user",
                    authorId = 2
                });

            Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        }
    }
}
