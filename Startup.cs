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
                // �û���֤����Ҫ��������ݵ�¼
                .AddJwtBearer(userKey, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //�Ƿ���֤Issuer
                        ValidateAudience = false, //�Ƿ���֤Audience
                        ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                        ValidAudiences = new[] { "user", "admin", "suadmin" }, //Audience
                        ValidIssuer = GlobalVars.domain, //Issuer���������ǰ��ǩ��jwt������һ��
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalVars.secret)) //�õ�SecurityKey
                    };
                })
                // ����Ա��֤����Ҫ�Թ���Ա�˻���¼
                .AddJwtBearer(adminKey, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //�Ƿ���֤Issuer
                        ValidateAudience = true, //�Ƿ���֤Audience
                        ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                        ValidAudiences = new[] { "admin", "suadmin" }, //Audience
                        ValidIssuer = GlobalVars.domain, //Issuer���������ǰ��ǩ��jwt������һ��
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalVars.secret)) //�õ�SecurityKey
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
