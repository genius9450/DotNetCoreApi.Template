using DotNetCoreApi.Template.Api.Helper;
using DotNetCoreApi.Template.Domain.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace DotNetCoreApi.Template.Api.Attribute
{
    /// <summary>
    /// Model驗證
    /// </summary>
    public class ModelStateValidationAttribute : ActionFilterAttribute
    {

        public ModelStateValidationAttribute()
        {

        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errMsg = string.Join(",", context.ModelState.SelectMany(x => x.Value.Errors).Select(x => x.ErrorMessage));
                context.Result = new ObjectResult(new
                {
                    StatusCode = ResponseStatusCode.ParameterError.ToInt(),
                    Message = errMsg
                });
            }

            base.OnActionExecuting(context);
        }

    }
}
