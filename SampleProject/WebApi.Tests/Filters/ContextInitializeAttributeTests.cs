using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dependencies;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
using Data;
using Moq;
using WebApi.App_Start;
using Xunit;

namespace WebApi.Tests.Filters
{
    public class ContextInitializeAttributeTests
    {
        private readonly ContextInitializeAttribute _filter = new ContextInitializeAttribute();
        private readonly Mock<IUnitOfWork> _unitOfWork = new Mock<IUnitOfWork>();

        [Theory]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public void OnActionExecuted_SuccessfulWrite_Commits(string method)
        {
            _filter.OnActionExecuted(ExecutedContext(new HttpMethod(method), HttpStatusCode.OK));

            _unitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void OnActionExecuted_ActionThrew_DoesNotCommit()
        {
            _filter.OnActionExecuted(ExecutedContext(HttpMethod.Post, null, new InvalidOperationException("halfway through an update")));

            _unitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Theory]
        [InlineData(HttpStatusCode.BadRequest)]
        [InlineData(HttpStatusCode.NotFound)]
        public void OnActionExecuted_ErrorResponse_DoesNotCommit(HttpStatusCode status)
        {
            _filter.OnActionExecuted(ExecutedContext(HttpMethod.Post, status));

            _unitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void OnActionExecuted_Read_DoesNotCommit()
        {
            _filter.OnActionExecuted(ExecutedContext(HttpMethod.Get, HttpStatusCode.OK));

            _unitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        private HttpActionExecutedContext ExecutedContext(HttpMethod method, HttpStatusCode? status, Exception exception = null)
        {
            var scope = new Mock<IDependencyScope>();
            scope.Setup(s => s.GetService(typeof(IUnitOfWork))).Returns(_unitOfWork.Object);

            var request = new HttpRequestMessage(method, "http://localhost/users");
            request.Properties[HttpPropertyKeys.DependencyScope] = scope.Object;

            var actionContext = new HttpActionContext(new HttpControllerContext { Request = request }, new ReflectedHttpActionDescriptor());
            var context = new HttpActionExecutedContext(actionContext, exception);
            if (status.HasValue)
            {
                context.Response = new HttpResponseMessage(status.Value);
            }
            return context;
        }
    }
}