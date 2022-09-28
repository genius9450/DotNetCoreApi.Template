using System.Threading.Tasks;
using DotNetCoreApi.Template.Schedule.Interface;
using Microsoft.Extensions.Logging;

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
