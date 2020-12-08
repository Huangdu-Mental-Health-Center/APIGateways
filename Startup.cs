using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace APIGateways
{
    public class Startup
    {
        readonly string AllowThisSite = "AllowThisSite";
        readonly string userKey = "UserKey";
        readonly string adminKey = "AdminKey";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(name: AllowThisSite, builder =>
                  {
                      builder.WithOrigins($"http://*.{GlobalVars.domain}")
                      .SetIsOriginAllowedToAllowWildcardSubdomains()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
                  });
            });

            services.AddResponseCaching();

            services.AddRazorPages();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                // 用户验证：需要以任意身份登录
                .AddJwtBearer(userKey, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //是否验证Issuer
                        ValidateAudience = false, //是否验证Audience
                        ValidateLifetime = true, //是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, //是否验证SecurityKey
                        ValidAudiences = new[] { "user", "admin", "suadmin" }, //Audience
                        ValidIssuer = GlobalVars.domain, //Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalVars.secret)) //拿到SecurityKey
                    };
                })
                // 管理员验证：需要以管理员账户登录
                .AddJwtBearer(adminKey, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //是否验证Issuer
                        ValidateAudience = true, //是否验证Audience
                        ValidateLifetime = true, //是否验证失效时间
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, //是否验证SecurityKey
                        ValidAudiences = new[] { "admin", "suadmin" }, //Audience
                        ValidIssuer = GlobalVars.domain, //Issuer，这两项和前面签发jwt的设置一致
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalVars.secret)) //拿到SecurityKey
                    };
                });

            services.AddOcelot();
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
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseCors(AllowThisSite);

            app.UseResponseCaching();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });

            app.UseOcelot().Wait();
        }
    }
}
