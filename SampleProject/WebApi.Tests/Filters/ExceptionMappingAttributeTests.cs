using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Common.Exceptions;
using WebApi.App_Start;
using Xunit;

namespace WebApi.Tests.Filters
{
    public class ExceptionMappingAttributeTests
    {
        private readonly ExceptionMappingAttribute _filter = new ExceptionMappingAttribute();

        [Fact]
        public void OnException_Conflict_Returns409WithMessage()
        {
            var context = ContextFor(new ConflictException("A user with id 'x' already exists."));

            _filter.OnException(context);

            Assert.Equal(HttpStatusCode.Conflict, context.Response.StatusCode);
            Assert.Equal("A user with id 'x' already exists.", ErrorOf(context.Response).Message);
        }

        [Fact]
        public void OnException_DomainValidation_Returns400WithModelStateForTheMember()
        {
            var context = ContextFor(new DomainValidationException("age", "Age must be between 0 and 150."));

            _filter.OnException(context);

            Assert.Equal(HttpStatusCode.BadRequest, context.Response.StatusCode);
            var modelState = ErrorOf(context.Response).ModelState;
            Assert.Equal(new[] { "Age must be between 0 and 150." }, (string[])modelState["age"]);
        }

        [Theory]
        [InlineData(typeof(InvalidOperationException))]
        [InlineData(typeof(ArgumentException))]
        public void OnException_UnexpectedException_LeavesItUnhandled(Type exceptionType)
        {
            var context = ContextFor((Exception)Activator.CreateInstance(exceptionType, "bug"));

            _filter.OnException(context);

            Assert.Null(context.Response);
        }

        internal static HttpActionExecutedContext ContextFor(Exception exception)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/users");
            request.SetConfiguration(new HttpConfiguration());
            var actionContext = new HttpActionContext(new HttpControllerContext { Request = request }, new ReflectedHttpActionDescriptor());
            return new HttpActionExecutedContext(actionContext, exception);
        }

        internal static HttpError ErrorOf(HttpResponseMessage response)
        {
            return (HttpError)((ObjectContent)response.Content).Value;
        }
    }
}