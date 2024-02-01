using DBCD.Providers;
using ToolsAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ToolsAPI
{
    public class Startup
    {
        readonly string wtOrigins = "_wtOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddResponseCompression();
            services.AddCors(options =>
            {
                options.AddPolicy(name: wtOrigins,
                    builder =>
                    {
                        builder.WithOrigins("https://wow.tools");
                        builder.WithOrigins("http://wow.tools.localhost");
                        builder.WithOrigins("https://wowtools.work");
                        builder.WithOrigins("http://wowtools.work");
                        builder.WithOrigins("http://wowtools.work.local");
                    });
            });


            services.AddSingleton<IDBDProvider, DBDProvider>();
            services.AddSingleton<IDBCProvider, DBCProvider>();
            services.AddSingleton<IDBCManager, DBCManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseResponseCompression();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
