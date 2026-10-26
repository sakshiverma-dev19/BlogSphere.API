using BlogSphere.API.Models;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace BlogSphere.API.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Post_WithValidTitleAndContent_ShouldPassValidation()
        {
            var post = new Post
            {
                Title = "My First Blog",
                Content = "This is my blog content.",
                AuthorId = 1
            };

            var results = ValidateModel(post);

            Assert.Empty(results);
        }

        [Fact]
        public void Post_WithEmptyTitle_ShouldFailValidation()
        {
            var post = new Post
            {
                Title = "",
                Content = "This is my blog content.",
                AuthorId = 1
            };

            var results = ValidateModel(post);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void Comment_WithValidContent_ShouldPassValidation()
        {
            var comment = new Comment
            {
                PostId = 1,
                Content = "This is a valid comment.",
                AuthorId = 1
            };

            var results = ValidateModel(comment);

            Assert.Empty(results);
        }

        [Fact]
        public void Comment_WithEmptyContent_ShouldFailValidation()
        {
            var comment = new Comment
            {
                PostId = 1,
                Content = "",
                AuthorId = 1
            };

            var results = ValidateModel(comment);

            Assert.NotEmpty(results);
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model);

            Validator.TryValidateObject(
                model,
                context,
                results,
                validateAllProperties: true);

            return results;
        }
    }
}