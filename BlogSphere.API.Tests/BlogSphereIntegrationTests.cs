using BlogSphere.API.Data;
using BlogSphere.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http.Headers;
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
                (int)response.StatusCode == 404);
        }

        [Fact]
        public async Task PostsEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Posts");

            Assert.Equal(
                System.Net.HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task CommentsEndpoint_WithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync("/api/Comments");

            Assert.Equal(
                System.Net.HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task PostsEndpoint_InvalidRequestWithoutToken_ReturnsUnauthorized()
        {
            var client = _factory.CreateClient();

            var postData = new
            {
                title = "",
                content = ""
            };

            var response = await client.PostAsJsonAsync(
                "/api/Posts",
                postData);

            Assert.Equal(
                System.Net.HttpStatusCode.Unauthorized,
                response.StatusCode);
        }

        [Fact]
        public async Task CreatePost_EmptyTitleAndContent_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            string email = "sakshi_test@example.com";
            string password = "TestPass123!";

            // Step 1: Test database mein user create karo
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

                // API ke PasswordHasher se password hash karo
                var passwordHasher = new PasswordHasher<User>();

                user.PasswordHash = passwordHasher.HashPassword(
                    user, password);

                await db.SaveChangesAsync();
            }

            // Step 2: Login request bhejo
            var loginData = new
            {
                email = email,
                passwordHash = password
            };

            var loginResponse = await client.PostAsJsonAsync(
                "/api/Auth/login",
                loginData);

            string loginBody =
                await loginResponse.Content.ReadAsStringAsync();

            Assert.True(
                loginResponse.IsSuccessStatusCode,
                $"Login failed. Status: {(int)loginResponse.StatusCode}. " +
                $"Response: {loginBody}");

            // Step 3: Response se Token nikalo
            using var loginDocument = System.Text.Json.JsonDocument.Parse(
                loginBody);

            // ASP.NET Core serializes anonymous-object property names as camelCase by default.
            // Accept either casing so the test matches the API response reliably.
            var tokenProperty = loginDocument.RootElement
                .EnumerateObject()
                .FirstOrDefault(property =>
                    string.Equals(property.Name, "token", StringComparison.OrdinalIgnoreCase));

            Assert.True(
                tokenProperty.Value.ValueKind == JsonValueKind.String,
                $"Login response does not contain a valid token property: {loginBody}");

            string token = tokenProperty.Value.GetString()!;

            // Step 4: Authorization header set karo
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            // Step 5: Empty title aur content bhejo
            var postData = new
            {
                title = "",
                content = ""
            };

            var response = await client.PostAsJsonAsync(
                "/api/Posts",
                postData);

            string responseBody =
                await response.Content.ReadAsStringAsync();

            // Step 6: Expected result 400 Bad Request hai
            Assert.True(
                response.StatusCode ==
                    System.Net.HttpStatusCode.BadRequest,
                $"Expected 400 Bad Request, but received " +
                $"{(int)response.StatusCode}. Response: {responseBody}");
        }
    }
}