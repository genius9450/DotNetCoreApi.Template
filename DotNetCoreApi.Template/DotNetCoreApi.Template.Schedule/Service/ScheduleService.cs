using System;
using System.Collections.Generic;
using System.Text;
using DotNetCoreApi.Template.Schedule.Interface;
using DotNetCoreApi.Template.Schedule.Process;
using Hangfire;

namespace DotNetCoreApi.Template.Schedule.Service
{
    public class ScheduleService : IScheduleService
    {
        public void Start()
        {
            // eg: Cron.Daily(07, 30)
            // 分鐘 小時 日期 月份 週
            // 30   07    *    *   *

            // 測試每分鐘塞Log
            // AddRecurringJob<AddLogProcess>(Cron.MinuteInterval(1), TimeZoneInfo.Local);

        }

        private void AddScheduleTask<T>(string cron, TimeZoneInfo local) where T : IProcess
        {
            var taskName = typeof(T).Name + cron;
            RecurringJob.RemoveIfExists(taskName);  // 清除Job
            RecurringJob.AddOrUpdate<T>(taskName, (x) => x.Main(), cron, local);
        }
    }
}
