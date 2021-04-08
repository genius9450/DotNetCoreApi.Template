using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using DotNetCoreApi.Template.Domain.Shared;

namespace DotNetCoreApi.Template.Api.Attribute
{
    public class ActionLogAttribute : ActionFilterAttribute
    {
        private ILogger<ActionLogAttribute> logger;
        private ApiLogModel apiLogModel { get; set; }

        public ActionLogAttribute(ILogger<ActionLogAttribute> _logger)
        {
            logger = _logger;
            apiLogModel = new ApiLogModel();
        }

        /// <summary>
        /// 動作執行前
        /// </summary>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            apiLogModel.HttpMethod = context.HttpContext.Request.Method;
            apiLogModel.ClientIP = context.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            apiLogModel.FullPath = $"{context.HttpContext.Request.Host}{context.HttpContext.Request.Path.Value}";

            var Data = context.ActionArguments;
            apiLogModel.Request = Data == null ? "" : JsonConvert.SerializeObject(Data);
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// 動作執行後
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var Data = context.Result;
            apiLogModel.Response = Data == null ? "" : JsonConvert.SerializeObject(((ObjectResult)context.Result).Value);
            logger.LogInformation("{HttpMethod} / {FullPath} / {Request} / {Response} / {ClientIP}", apiLogModel.HttpMethod, apiLogModel.FullPath, apiLogModel.Request, apiLogModel.Response, apiLogModel.ClientIP);

            await base.OnResultExecutionAsync(context, next);
        }

    }

}
