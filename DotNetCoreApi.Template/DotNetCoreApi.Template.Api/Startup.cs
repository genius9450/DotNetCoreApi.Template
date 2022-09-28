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

            #region ��l�ưѼ�

            Const.EnvironmentName = environment.EnvironmentName;
            Const.DefaultConnectionString = Configuration["DBConnectionString:DefaultConnectionString"];

            #endregion            
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddControllersAsServices()    // AddControllersAsServices ��k��{�F���Ʊ� - ���N�z���ε{�Ǥ����Ҧ�������U�� DI �e���]�p�G�|�����U�^�A�ñN IControllerActivator ���U�� ServiceBasedControllerActivator
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true; // ���Φ۰�400�^���A��Ѧ۩w�q���쮷��
                })
                .AddNewtonsoftJson(options =>
                {
                    //�ܧ󬰹w�]�j�p�g
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                });

            // ���ШD
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder =>
                {
                    // Not a permanent solution, but just trying to isolate the problem
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            // ���쪺Filter
            services.AddMvc(config =>
            {
                // �O��request, response
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

            // ���UHangfire�Ƶ{
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
            //AutoFac Ioc�`�J
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

            app.UseCors("CorsPolicy");  // �����

            app.UseMiddleware<ExceptionMiddleware>(); // ��������Exception

            // �Dlocalhost address�i�H�}��hangfire dashboard�A���ȯ��˵�
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

            // �}�l����Ƶ{
            BackgroundJob.Enqueue<IScheduleService>(x => x.Start());

        }
    }
}
