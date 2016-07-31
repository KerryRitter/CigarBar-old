﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CigarBar.Api.Data.Models;
using CigarBar.Api.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using CigarBar.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace CigarBar.Api
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets();
            }

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration["DefaultConnection"]));

            services.AddIdentity<ApplicationUser, IdentityRole>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOpenIddict<ApplicationUser, ApplicationDbContext>()
                .EnableTokenEndpoint("/connect/token")
                .AllowPasswordFlow()
                .DisableHttpsRequirement()
                .AddEphemeralSigningKey()
                .Configure(o =>
                {
                    o.AccessTokenLifetime = new System.TimeSpan(3650, 0, 0, 0);
                });

            services.AddSingleton(Configuration);
            services.AddSingleton<IContextFactory, ContextFactory>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<ICigarsRepository, CigarsRepository>();
            services.AddSingleton<IRatingsRepository, RatingsRepository>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors(builder =>
                builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
            );

            app.UseIdentity();
            app.UseOAuthValidation();
            app.UseOpenIddict();
            app.UseMvc();
        }
    }
}
