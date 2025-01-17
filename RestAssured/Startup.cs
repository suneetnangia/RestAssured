using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RestAssured.Configuration;
using RestAssured.Worker;

using System;
using System.Collections.Concurrent;

namespace RestAssured
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
            services.AddControllers();

            // Background worker dependencies.
            services.AddSingleton<ConcurrentQueue<WorkerTask>>();
            services.AddSingleton(sp =>
                new TaskProcessor(
                    sp.GetRequiredService<ConcurrentQueue<WorkerTask>>(),
                    TimeSpan.FromMilliseconds(sp.GetRequiredService<IOptions<BackgroundWorkerConfiguration>>().Value.DequeueIntervalInMilliseconds),
                    sp.GetRequiredService<ILogger<TaskProcessor>>()));
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