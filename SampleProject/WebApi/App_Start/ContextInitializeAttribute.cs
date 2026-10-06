using System;
using System.Net.Http;
using System.Web.Http.Filters;
using Data;

namespace WebApi.App_Start
{
    /// <summary>
    /// Commits the request's unit of work once a write action has completed successfully.
    /// A request that threw or answered with an error status is not committed, so
    /// changes applied by a failing action are discarded.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContextInitializeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Exception != null || !IsWriteMethod(actionExecutedContext.Request.Method))
            {
                return;
            }

            var response = actionExecutedContext.Response;
            if (response != null && !response.IsSuccessStatusCode)
            {
                return;
            }

            // Resolve from the request scope so we commit exactly the session the action used.
            var unitOfWork = (IUnitOfWork)actionExecutedContext.Request.GetDependencyScope().GetService(typeof(IUnitOfWork));
            unitOfWork.Commit();
        }

        private static bool IsWriteMethod(HttpMethod method)
        {
            return method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Delete;
        }
    }
}