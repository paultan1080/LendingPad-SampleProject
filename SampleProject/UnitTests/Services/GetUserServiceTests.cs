using BusinessEntities;
using Common.Exceptions;
using Core.Services;
using Core.Services.Users;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class GetUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository = new Mock<IUserRepository>();

        private GetUserService CreateService()
        {
            return new GetUserService(_userRepository.Object);
        }

        [Fact]
        public void GetUsersByTag_Valid_QueriesRepositoryWithTrimmedTagAndPage()
        {
            var users = new[] { new User() };
            _userRepository.Setup(r => r.GetByTag("C", 10, 20)).Returns(users);

            var result = CreateService().GetUsersByTag(" C ", 10, 20);

            Assert.Same(users, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void GetUsersByTag_MissingTag_ThrowsValidationErrorForTag(string tag)
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().GetUsersByTag(tag, 0, 10));

            Assert.Equal("tag", exception.MemberName);
            _userRepository.Verify(r => r.GetByTag(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void GetUsersByTag_NegativeSkip_ThrowsValidationErrorForSkip()
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().GetUsersByTag("C", -1, 10));

            Assert.Equal("skip", exception.MemberName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(Paging.MaxPageSize + 1)]
        public void GetUsersByTag_TakeOutOfRange_ThrowsValidationErrorForTake(int take)
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().GetUsersByTag("C", 0, take));

            Assert.Equal("take", exception.MemberName);
        }
    }
}