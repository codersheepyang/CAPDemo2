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
            //���̳���SqlServerStorageInitializer������ע�룬�������������ͽ�����Ϣ�洢�ĳ־û�����
            services.AddSingleton<IStorageInitializer, SqlServerInitTable>();
            services.AddCap(config => 
            {
                //����SQL Server�������ַ���
                config.UseSqlServer("Data Source=localhost;Initial Catalog=Test;Integrated Security=SSPI;");
                //����Kafka�����ӵ�ַ
                config.UseKafka("localhost:9092");
                //���ӻ���Ϣ�����Կ����������˶�����Ϣ����������Ϣ���ݣ�������Ϣ�ķ�������Ϣ���Ե�
                //Ĭ�Ϸ���Ϊ��localhost:5001/cap
                config.UseDashboard(u => u.AppPath = "show");

                //��kafka���ڼ�Ⱥ��ָ��dashboard��ʾ�ĸ�kafka�ڵ�
                config.UseDiscovery(d =>
                {
                    //API��ַ
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 5001;
                    //Kafka��ַ
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 9092;
                    d.NodeId = "1";
                    d.NodeName = "CAP No.1 Node";
                });
            });
            services.AddControllers();

            //Skywalking CAP����׷�٣�ʵ��SkyAPM-dotnet
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
