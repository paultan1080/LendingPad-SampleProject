using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using BusinessEntities;
using WebApi.App_Start;
using WebApi.Controllers;
using WebApi.Models.Users;
using Xunit;

namespace WebApi.Tests.Filters
{
    public class ValidateModelAttributeTests
    {
        private readonly ValidateModelAttribute _filter = new ValidateModelAttribute();

        [Fact]
        public void OnActionExecuting_MissingBody_Returns400ForTheBodyParameter()
        {
            var context = CreateUserContext(null);

            _filter.OnActionExecuting(context);

            Assert.Equal(HttpStatusCode.BadRequest, context.Response.StatusCode);
            Assert.True(ExceptionMappingAttributeTests.ErrorOf(context.Response).ModelState.ContainsKey("model"));
        }

        [Fact]
        public void OnActionExecuting_InvalidModelState_Returns400()
        {
            var context = CreateUserContext(new UserModel());
            context.ModelState.AddModelError("model.Email", "The Email field is required.");

            _filter.OnActionExecuting(context);

            Assert.Equal(HttpStatusCode.BadRequest, context.Response.StatusCode);
        }

        [Fact]
        public void OnActionExecuting_ValidRequest_LetsTheActionRun()
        {
            var context = CreateUserContext(new UserModel { Name = "Mary", Email = "mary@companya.com", Type = UserTypes.Admin });

            _filter.OnActionExecuting(context);

            Assert.Null(context.Response);
        }

        private static HttpActionContext CreateUserContext(UserModel model)
        {
            var configuration = new HttpConfiguration();
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/users");
            request.SetConfiguration(configuration);

            var controller = new HttpControllerDescriptor(configuration, "User", typeof(UserController));
            var action = new ReflectedHttpActionDescriptor(controller, typeof(UserController).GetMethod(nameof(UserController.CreateUser)));
            var context = new HttpActionContext(new HttpControllerContext { Request = request, Configuration = configuration, ControllerDescriptor = controller }, action);
            context.ActionArguments["model"] = model;
            return context;
        }
    }
}