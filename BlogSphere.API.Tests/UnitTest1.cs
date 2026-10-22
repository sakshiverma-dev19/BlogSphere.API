
using Xunit;

namespace BlogSphere.API.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Title_ShouldNotBeEmpty()
        {
            string title = "My First Blog";

            Assert.False(string.IsNullOrWhiteSpace(title));
        }

        [Fact]
        public void Content_ShouldNotBeEmpty()
        {
            string content = "This is my blog content.";

            Assert.False(string.IsNullOrWhiteSpace(content));
        }
    }
}
