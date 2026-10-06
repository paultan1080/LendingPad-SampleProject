using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Common.Exceptions;

namespace WebApi.App_Start
{
    /// <summary>
    /// Translates expected exceptions into HTTP error responses.
    /// Anything not handled here is a server fault and stays a 500.
    /// </summary>
    public class ExceptionMappingAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var request = actionExecutedContext.Request;
            switch (actionExecutedContext.Exception)
            {
                case ConflictException conflict:
                    actionExecutedContext.Response = request.CreateErrorResponse(HttpStatusCode.Conflict, conflict.Message);
                    break;
            }
        }
    }
}