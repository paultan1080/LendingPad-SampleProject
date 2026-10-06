using System;
using BusinessEntities;
using Common.Exceptions;
using Core.Factories;
using Core.Services.Users;
using Data.Repositories;
using Moq;
using Xunit;

namespace UnitTests.Services
{
    public class CreateUserServiceTests
    {
        private static readonly Guid Id = Guid.NewGuid();
        private static readonly string[] Tags = { "A" };

        private readonly Mock<IIdObjectFactory<User>> _userFactory = new Mock<IIdObjectFactory<User>>();
        private readonly Mock<IUserRepository> _userRepository = new Mock<IUserRepository>();
        private readonly Mock<IUpdateUserService> _updateUserService = new Mock<IUpdateUserService>();

        private CreateUserService CreateService()
        {
            return new CreateUserService(_userFactory.Object, _userRepository.Object, _updateUserService.Object);
        }

        [Fact]
        public void Create_IdAlreadyTaken_ThrowsConflictWithoutTouchingTheExistingUser()
        {
            _userRepository.Setup(r => r.Get(Id)).Returns(new User());

            var exception = Assert.Throws<ConflictException>(() => CreateService().Create(Id, "Mary", "mary@companya.com", UserTypes.Admin, 80000m, Tags));

            Assert.Contains(Id.ToString(), exception.Message);
            _userFactory.Verify(f => f.Create(It.IsAny<Guid>()), Times.Never);
            _userRepository.Verify(r => r.Save(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void Create_EmptyId_ThrowsValidationErrorForId()
        {
            var exception = Assert.Throws<DomainValidationException>(() => CreateService().Create(Guid.Empty, "Mary", "mary@companya.com", UserTypes.Admin, 80000m, Tags));

            Assert.Equal("id", exception.MemberName);
            _userRepository.Verify(r => r.Save(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public void Create_NewId_PopulatesAndStoresTheUser()
        {
            var user = new User();
            _userFactory.Setup(f => f.Create(Id)).Returns(user);

            var result = CreateService().Create(Id, "Mary", "mary@companya.com", UserTypes.Admin, 80000m, Tags);

            Assert.Same(user, result);
            _updateUserService.Verify(u => u.Update(user, "Mary", "mary@companya.com", UserTypes.Admin, 80000m, Tags), Times.Once);
            _userRepository.Verify(r => r.Save(user), Times.Once);
        }
    }
}