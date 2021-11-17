using CAPTest.Init;
using CAPTest.Services;
using DotNetCore.CAP.Dashboard.NodeDiscovery;
using DotNetCore.CAP.Messages;
using DotNetCore.CAP.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ICAPTestService, CAPTestService>();
            //将继承了SqlServerStorageInitializer的子类注入，以重命名发布和接受消息存储的持久化表名
            services.AddSingleton<IStorageInitializer, SqlServerInitTable>();
            services.AddCap(config => 
            {
                //配置SQL Server的连接字符串
                config.UseSqlServer("Data Source=localhost;Initial Catalog=Test;Integrated Security=SSPI;");
                //配置Kafka的连接地址
                config.UseKafka("localhost:9092");
                //可视化消息，可以看到：发布了多少消息，发布的消息内容，订阅消息的方法，消息重试等
                //默认访问为：localhost:5001/cap
                config.UseDashboard(u => u.AppPath = "show");

                //当kafka存在集群，指定dashboard显示哪个kafka节点
                config.UseDiscovery(d =>
                {
                    //API地址
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 5001;
                    //Kafka地址
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 9092;
                    d.NodeId = "1";
                    d.NodeName = "CAP No.1 Node";
                });
            });
            services.AddControllers();

            //Skywalking CAP性能追踪，实现SkyAPM-dotnet
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
