using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using DotNetCoreApi.Template.Api.Attribute;
using DotNetCoreApi.Template.Api.Middleware;
using DotNetCoreApi.Template.Api.Ioc;
using Autofac;
using DotNetCoreApi.Template.Api;
using DotNetCoreApi.Template.Domain.Shared;
using Microsoft.Extensions.Logging;
using DotNetCoreApi.Template.EF;
using DotNetCoreApi.Template.Schedule;
using DotNetCoreApi.Template.Schedule.Interface;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace DotNetCoreApi.Template.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;

            var builder = new ConfigurationBuilder()
                               .SetBasePath(environment.ContentRootPath)
                               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                               .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
                               .AddEnvironmentVariables();

            Configuration = builder.Build();

            #region 初始化參數

            Const.EnvironmentName = environment.EnvironmentName;
            Const.DefaultConnectionString = Configuration["DBConnectionString:DefaultConnectionString"];

            #endregion            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddControllersAsServices()    // AddControllersAsServices 方法實現了兩件事情 - 它將您應用程序中的所有控制器註冊到 DI 容器（如果尚未註冊），並將 IControllerActivator 註冊為 ServiceBasedControllerActivator
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true; // 停用自動400回應，改由自定義全域捕捉
                })
                .AddNewtonsoftJson(options =>
                {
                    //變更為預設大小寫
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            // 跨域請求
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder =>
                {
                    // Not a permanent solution, but just trying to isolate the problem
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // 全域的Filter
            services.AddMvc(config =>
            {
                // 記錄request, response
                config.Filters.Add(new TypeFilterAttribute(typeof(ActionLogAttribute)));
                // Model Validate
                config.Filters.Add(new TypeFilterAttribute(typeof(ModelStateValidationAttribute)));
            });

            //Seq Logger
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSeq(Configuration.GetSection("Seq"));
            });

            // runtime db
            services.AddDbContext<DotNetCoreApiTemplateDBContext>(options =>
            {
                options.UseSqlServer(Const.DefaultConnectionString);
            });

            // Register Swagger services
            services.AddSwaggerGen(c => 
            {
                c.SwaggerDoc("v1", new OpenApiInfo() { Title = "DotNetCoreApi.Template", Version = "v1" });
            });

            // 註冊Hangfire排程
            services.AddHangfire(config =>
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_110)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage());
            services.AddHangfireServer();

            services.AddHttpClient();

        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            //AutoFac Ioc注入
            var config = new AutofacConfig
            {
                DBConnectionString = Const.DefaultConnectionString
            };
            config.ConfigContainer(builder);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment() || env.IsEnvironment("Debug"))
            {
                app.UseDeveloperExceptionPage();                

                //app.UseHttpsRedirection();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();

                //app.UseHttpsRedirection();
            }

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseSwagger();
            app.UseSwaggerUI(c=> 
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNetCoreApi.Template");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("CorsPolicy");  // 跨網域

            app.UseMiddleware<ExceptionMiddleware>(); // 捕捉全域Exception

            // 非localhost address可以開啟hangfire dashboard，但僅能檢視
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new ScheduleAuthorizationFilter() },
                IsReadOnlyFunc = (DashboardContext context) => context.Request.RemoteIpAddress != context.Request.LocalIpAddress
            });

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllers();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "api/{controller}/{action}"
                );
            });

            Const.Logger = logger;

            // 開始執行排程
            BackgroundJob.Enqueue<IScheduleService>(x => x.Start());

        }
    }
}
