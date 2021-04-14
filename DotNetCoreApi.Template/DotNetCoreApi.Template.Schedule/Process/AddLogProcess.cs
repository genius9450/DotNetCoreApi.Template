using DotNetCoreApi.Template.EF.Entity;
using DotNetCoreApi.Template.Schedule.Interface;
using DotNetCoreApi.Template.Service.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace DotNetCoreApi.Template.Schedule.Process
{
    /// <summary>
    /// 排程-測試塞Log
    /// </summary>
    public class AddLogProcess : IProcess
    {
        public ILogger<AddLogProcess> Logger { get; set; }

        public async Task Main()
        {
            await Task.Delay(0);
            Logger.LogInformation("Schedule / {Process}", "AddLogProcess");
        }
    }
}
