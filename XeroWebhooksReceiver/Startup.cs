using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using XeroWebhooksReceiver.Config;
using XeroWebhooksReceiver.Helpers;
using XeroWebhooksReceiver.Queue;

namespace XeroWebhooksReceiver
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
            services.TryAddSingleton(Configuration.GetSection("PayloadQueueSettings").Get<PayloadQueueSettings>());
            services.TryAddSingleton(Configuration.GetSection("WebhookSettings").Get<WebhookSettings>());

            services.TryAddTransient<IQueue<string>, PayloadQueue>();
            services.TryAddSingleton<ISignatureVerifier, SignatureVerifier>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
