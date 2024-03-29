﻿//using Altob.PaymentService.Domain.Model.Shared;

using DotNetCoreApi.Template.Domain.Shared;
using Microsoft.Extensions.Logging;

namespace DotNetCoreApi.Template.Api
{
    public static class Const 
    {
        /// <summary>
        /// 環境名稱
        /// </summary>
        public static string EnvironmentName { get; set; }

        /// <summary>
        /// 預設資料庫連線
        /// </summary>
        public static string DefaultConnectionString { get; set; }

        /// <summary>
        /// Logger
        /// </summary>
        public static ILogger<Startup> Logger { get; set; }

    }
}
