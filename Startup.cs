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
        readonly string AllowAllOrigins = "AllowAllOrigins";

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
                options.AddPolicy(name: AllowAllOrigins, builder =>
                  {
                      builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod();
                  });
            });
                
            services.AddRazorPages();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                // �û���֤����Ҫ��������ݵ�¼
                .AddJwtBearer("UserKey", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //�Ƿ���֤Issuer
                        ValidateAudience = false, //�Ƿ���֤Audience
                        ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                        // ValidAudience = "user", //Audience
                        ValidIssuer = GlobalVars.domain, //Issuer���������ǰ��ǩ��jwt������һ��
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GlobalVars.secret)) //�õ�SecurityKey
                    };
                })
                // ����Ա��֤����Ҫ�Թ���Ա�˻���¼
                .AddJwtBearer("AdminKey", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true, //�Ƿ���֤Issuer
                        ValidateAudience = true, //�Ƿ���֤Audience
                        ValidateLifetime = true, //�Ƿ���֤ʧЧʱ��
                        ClockSkew = TimeSpan.FromSeconds(30),
                        ValidateIssuerSigningKey = true, //�Ƿ���֤SecurityKey
                        ValidAudience = "admin", //Audience
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

            app.UseCors(AllowAllOrigins);

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
