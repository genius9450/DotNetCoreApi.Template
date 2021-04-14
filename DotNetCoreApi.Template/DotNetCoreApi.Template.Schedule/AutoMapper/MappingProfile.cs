using DotNetCoreApi.Template.EF.Entity;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreApi.Template.Schedule.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //添加需要映射对象
            //CreateMap<BatchMakeInvoice, PaymentRecord>();
            //CreateMap<ResponsePaymentInvModelV2, PaymentRecord>();            
        }        
    }
}
