using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreApi.Template.Schedule.Interface
{
    interface IScheduleService
    {
        /// <summary>
        /// 註冊排程
        /// </summary>
        void Start();
    }
}
