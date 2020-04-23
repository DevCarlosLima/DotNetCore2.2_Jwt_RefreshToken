using System;
using Api.Models;
using Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;

namespace Api {
    public class Startup {
        public Startup (IConfiguration configuration) {
            _configuration = configuration;
        }

        public IConfiguration _configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {
            services.AddDistributedRedisCache (options => {
                options.Configuration =
                    _configuration.GetConnectionString ("RedisConnection");
                options.InstanceName = "JwtRefreshToken";
            });

            var signingConfigurations = new SigningConfigurations ();
            services.AddSingleton (signingConfigurations);

            var tokenConfigurations = new TokenConfigurations ();
            new ConfigureFromConfigurationOptions<TokenConfigurations> (
                    _configuration.GetSection ("TokenConfigurations"))
                .Configure (tokenConfigurations);
            services.AddSingleton (tokenConfigurations);

            services.AddAuthentication (options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer (options => {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingConfigurations.Key,
                    ValidateIssuer = true,
                    ValidIssuer = tokenConfigurations.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenConfigurations.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddAuthorization (options => {
                options.AddPolicy ("Bearer", new AuthorizationPolicyBuilder ()
                    .AddAuthenticationSchemes (JwtBearerDefaults.AuthenticationScheme‌​)
                    .RequireAuthenticatedUser ()
                    .Build ());
            });

            services.AddSwaggerGen (options => {
                options.SwaggerDoc ("v1", new Info { Version = "v1", Title = ".Net Core 2.2 and JWT (Refresh Token)" });
            });

            services.AddScoped<TokenService> ();

            services.AddMvc ().SetCompatibilityVersion (CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IHostingEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            } else {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts ();
            }

            app.UseHttpsRedirection ();
            app.UseMvc ();
        }
    }
}