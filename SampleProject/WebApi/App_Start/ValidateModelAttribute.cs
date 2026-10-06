using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebApi.App_Start
{
    /// <summary>
    /// Rejects a request before its action runs when the body is missing or when model binding or
    /// DataAnnotations validation failed, answering 400 with the errors per field.
    /// </summary>
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var bodyParameters = actionContext.ActionDescriptor.GetParameters().Where(p => p.ParameterBinderAttribute is FromBodyAttribute);
            foreach (var parameter in bodyParameters)
            {
                if (!actionContext.ActionArguments.TryGetValue(parameter.ParameterName, out var value) || value == null)
                {
                    actionContext.ModelState.AddModelError(parameter.ParameterName, "A request body is required.");
                }
            }

            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
}