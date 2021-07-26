#pragma warning disable IDE0058 // Expression value is never used

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SignalRFileTransfer_Server.Hubs;
using SignalRFileTransfer_Server.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRFileTransfer_Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSignalR()
            .AddMessagePackProtocol();
            //.AddMessagePackProtocol(options =>
            //{
            //    options.FormatterResolvers = new List<MessagePack.IFormatterResolver>()
            //        {
            //                        MessagePack.Resolvers.StandardResolver.Instance,
            //                        MessagePack.Resolvers.DynamicEnumAsStringResolver.Instance,
            //        };
            //});  //Manually turn on MessagePack. Must reference NuGet: Microsoft.AspNetCore.SignalR.Protocols.MessagePack

            services.AddSingleton<IFileTransferHandler, FileTransferHandler>();
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<FileTransferHub>("/" + nameof(FileTransferHub));
            });
        }
    }
}
#pragma warning restore IDE0058 // Expression value is never used
