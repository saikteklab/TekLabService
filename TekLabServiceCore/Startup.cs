using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using TekLabServiceCore.Extensions;
using Microsoft.OpenApi.Models;
using Stripe;

namespace TekLabServiceCore
{
    public class Startup
    {
        // This class is needed to inject the JWT validation options including the public key
        public class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
        {
            public void Configure(string name, JwtBearerOptions options)
            {
                StripeConfiguration.ApiKey = "sk_test_51IelfuB8YTUa63fz9XSvBcyNn9Nl0sn4UPJIyM8oc4p6RsfeiCIwKJ2CNnl4OwxdsTOVlAoh5yNh2MOzLWOGXzWw00soqmy31l";
                RSA rsa = RSA.Create();
                rsa.ImportRSAPublicKey(Convert.FromBase64String(PUBLIC_KEY), out _);

                options.IncludeErrorDetails = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new RsaSecurityKey(rsa),
                    ValidateIssuer = true,
                    ValidIssuer = "TekLabAuthService",
                    ValidateAudience = true,
                    ValidAudience = "TekLabService",
                    CryptoProviderFactory = new CryptoProviderFactory()
                    {
                        CacheSignatureProviders = false
                    }
                };
            }

            public void Configure(JwtBearerOptions options)
            {
                throw new NotImplementedException();
            }
        }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddCors(op =>
            {
                op.AddPolicy("frontend", builder =>
                {
                    builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
                });
            });
            services.AddMvc().AddNewtonsoftJson();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TekLabService", Version = "v1" });
            });

            services.AddAuthentication(op =>
            {
                op.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                op.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer();

            services.AddTransient<IConfigureOptions<JwtBearerOptions>, ConfigureJwtBearerOptions>();

            services.AddTransient<MongoDBWrapper>(mongo => new
                         MongoDBWrapper(Configuration.GetMongoDBConnectionString(),
                                        Configuration.GetMongoDBName()
                                           ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseHttpsRedirection();
            app.UseCors("frontend");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TekLabService v1"));
        }

        private const string PUBLIC_KEY = @"MIICCgKCAgEAtCR2Pii+q9C76P2E9ydHYxnBPjJFGT7MvHuQPKpcS9RImfrkobt0
        LPS/406eWm/tRBvnYD9nDpHJNKN3TjEenFQuDGR4RHcGK/e43SAhTAi7+s0tfAQd
        6BK4gznIwvs5cWyilh1B7c9sCnxhJ/EYLIe1N2yiD8mhvfojIF4vMYxONIMTGYXy
        87lnO9zRAdXAZ39YbtmFmQwK8gfXX5d/XVlKy0tc2y5bRY5iXn9kwqwvFlzL6O4v
        pjhqA5kwsJV7efhL9nU0ACR4dG3zwFR3SAOOSETXjnfmjH2ocga+oa65ToypUz2L
        1DwnNHt+M5CtDJ9um4dbYaqfBWkjWe3FuGB0GNPS8pbX2nVt76OfHA/QKmxTWvFd
        POZnjpg2QhDujyXgoIY731zx5bAklKVoKFma/qfWfCyCSTUzhgu1KQm9swipMsQy
        NYr9CjbnIlPn4EvrBIbGcIiaRNCLCIlcAuxE/GiH1zBUfeJxfJQmurejp6mBAtAS
        FY08DmUebBz8mlUbB+LXMYKHZ4GK6TecPy0WJU2qRMQ//PKfOa+wkesp4M53SQdp
        ItDp5akTzYUo4rXwk3HPCtemKaSNhyG+EYtZ1CAmPN5sEjU0/x0Dq7SU5o8KhogB
        m/5HRJ3M9dMRcwD3OcsMl0kW1PPUt04itboS3SlFav90V9uc2YNGpPsCAwEAAQ==";
    }
}
