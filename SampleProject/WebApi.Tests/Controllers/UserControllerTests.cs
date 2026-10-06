using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using BusinessEntities;
using Core.Services.Users;
using Moq;
using WebApi.Controllers;
using WebApi.Models.Users;
using Xunit;

namespace WebApi.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<ICreateUserService> _createUserService = new Mock<ICreateUserService>();
        private readonly Mock<IDeleteUserService> _deleteUserService = new Mock<IDeleteUserService>();
        private readonly Mock<IGetUserService> _getUserService = new Mock<IGetUserService>();
        private readonly Mock<IUpdateUserService> _updateUserService = new Mock<IUpdateUserService>();

        [Fact]
        public void CreateUser_PassesTheRequestToTheServiceAsUserDetails()
        {
            var id = Guid.NewGuid();
            var model = new UserModel
                        {
                            Name = "Mary",
                            Email = "mary@companya.com",
                            Type = UserTypes.Admin,
                            Age = 29,
                            AnnualSalary = 80000m,
                            Tags = new[] { "A" }
                        };
            _createUserService.Setup(s => s.Create(id, It.IsAny<UserDetails>())).Returns(new User());

            var response = CreateController().CreateUser(id, model);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _createUserService.Verify(s => s.Create(id, It.Is<UserDetails>(d =>
                d.Name == "Mary" && d.Email == "mary@companya.com" && d.Type == UserTypes.Admin && d.Age == 29 &&
                d.AnnualSalary == 80000m && d.Tags == model.Tags)), Times.Once);
        }

        [Fact]
        public void GetUser_UnknownUser_Returns404()
        {
            var response = CreateController().GetUser(Guid.NewGuid());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void UpdateUser_UnknownUser_Returns404WithoutUpdating()
        {
            var response = CreateController().UpdateUser(Guid.NewGuid(), new UserModel());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            _updateUserService.Verify(s => s.Update(It.IsAny<User>(), It.IsAny<UserDetails>()), Times.Never);
        }

        private UserController CreateController()
        {
            var request = new HttpRequestMessage();
            request.SetConfiguration(new HttpConfiguration());
            return new UserController(_createUserService.Object, _deleteUserService.Object, _getUserService.Object, _updateUserService.Object)
                   {
                       Request = request
                   };
        }
    }
}