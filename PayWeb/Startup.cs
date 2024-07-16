using System;
using System.Text;
using CRM.Features.Accounting.BankConfiguration;
using CRM.Features.Accounting.BankStatement;
using CRM.Features.Accounting.BankStatementDetails;
using CRM.Features.Accounting.BankStatementServiceAX;
using CRM.Features.Accounting.HostToHostBanPais;
using CRM.Features.Admin.Roles;
using CRM.Features.Credits.ReceiptBreakdown;
using CRM.Features.Credits.ReceiptBreakdownReport;
using CRM.Infrastructure;
using CRM.Infrastructure.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PayWeb.Features.Security;
using PayWeb.Features.Users;
using PayWeb.Infrastructure;
using PayWeb.Infrastructure.Core;

namespace PayWeb
{
    public class Startup
    {
        readonly string CorsOrigins = "AllowAnyCorsPolicy";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //Agregado por Antonio - Inicializar la lectura de las props de "Route" en el appSettings
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {     
            var jwtTokenConfig = Configuration.GetSection("jwtTokenConfig").Get<JwtTokenConfig>();
            services.AddCors(options =>
            {
                options.AddPolicy(name: CorsOrigins,
                                  builder =>
                                  {
                                      builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                                  });
               
            });
            services.AddSingleton(jwtTokenConfig);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtTokenConfig.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfig.Secret)),
                    ValidAudience = jwtTokenConfig.Audience,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
            services.AddSingleton<IJwtAuthManager, JwtAuthManager>();
            services.AddHostedService<JwtRefreshTokenCache>();

            //services.AddScoped<IUserService, UserService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT Auth Demo", Version = "v1" });
              
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authentication",
                    Description = "Enter JWT Bearer token **_only_**",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer", // must be lower case
                    BearerFormat = "JWT",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, new string[] { }}
                });
            });

            services.AddControllers();

            //Reportes
            services.AddHttpContextAccessor();

            services.AddDbContext<PayWebContext>(o => {
                o.UseSqlServer(Configuration.GetConnectionString("PayWeb"));
            });
            services.AddDbContext<IMFinanzasContext>(o => {
                o.UseSqlServer(Configuration.GetConnectionString("IMFinanzas"));
            });

            services.AddDbContext<PayrollContext>(o =>
                o.UseSqlServer(Configuration.GetConnectionString("Payroll")));

            services.AddScoped<IUnitOfWorkPayWeb, UnitOfWorkPayWeb>(s => {
                DbContext db = s.GetService<PayWebContext>();
                return new UnitOfWorkPayWeb(db);
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>(s => {
                DbContext db = s.GetService<IMFinanzasContext>();
                return new UnitOfWork(db);
            });

            services.AddScoped<IUnitOfWorkPayroll, UnitOfWorkPayroll>(s => {
                DbContext db = s.GetService<PayrollContext>();
                return new UnitOfWorkPayroll(db);
            });

            services.AddScoped<UserAppService>();
            services.AddScoped<RolesService>();
            services.AddScoped<BankStatementAppService>();
            services.AddScoped<BankStatementDetailsAppService>();
            services.AddScoped<BankConfigurationAppService>();
            services.AddScoped<HostToHostBanPaisServices>();
            services.AddScoped<BanskStatementServiceAXService>();
            services.AddScoped<WorkpaperReportService>();
            services.AddScoped<ReceiptDetailBreakdownService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }           

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWT Auth Demo V1");
                c.DocumentTitle = "JWT Auth Demo";
            });

            app.UseRouting();
            app.UseCors(CorsOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
