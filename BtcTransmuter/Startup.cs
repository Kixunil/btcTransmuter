﻿using System.IO;
using BtcTransmuter.Abstractions.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BtcTransmuter.Data;
using BtcTransmuter.Data.Entities;
using BtcTransmuter.Services;
using ExtCore.WebApplication.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BtcTransmuter
{
    public class Startup
    {
        private string extensionsPath;

        public Startup(IHostingEnvironment  hostingEnvironment, IConfiguration configuration)
        {
            Configuration = configuration;
            
            extensionsPath = Path.Combine(hostingEnvironment.ContentRootPath, "Extensions");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase= false;
                options.Password.RequireUppercase= false;
                options.Password.RequiredUniqueChars= 0;
                options.Password.RequireNonAlphanumeric= false;
            });

            
            services.AddExtCore(this.extensionsPath, true);
            services.AddHttpClient();
            services.AddTransmuterServices();
            services.AddMemoryCache();
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<User>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceScopeFactory serviceScopeFactory)
        {
            DependencyHelper.ServiceScopeFactory = serviceScopeFactory;
            using (var scope = serviceScopeFactory.CreateScope())
            {
                using (var context = scope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }
            if (env.IsDevelopment())
            {
                
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            app.UseExtCore();
        }
    }
}