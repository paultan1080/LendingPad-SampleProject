using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Core.Services.Users;
using Moq;
using WebApi.Controllers;
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
        public void GetUser_UnknownUser_Returns404()
        {
            var response = CreateController().GetUser(Guid.NewGuid());

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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