using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TT.Deliveries.Data.Mongo;

namespace TT.Deliveries.Web.Api;

public class Startup
{
    private const string ServiceName = "Glue-API";

    public Startup(IWebHostEnvironment env)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(env.ContentRootPath)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
            .AddEnvironmentVariables();

        Configuration = builder.Build();
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        services.AddHttpLogging(options => { options.LoggingFields = HttpLoggingFields.All; });
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "TT.Deliveries.Web.Api", Version = "v1"});

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
        services.AddSingleton(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(Configuration.GetConnectionString("mongodb"));
            settings.ClusterConfigurator = cb =>
                cb.Subscribe(new DiagnosticsActivityEventSubscriber(new InstrumentationOptions
                    {CaptureCommandText = true}));
            var client = new MongoClient(settings);
            return client.GetDatabase("glue");
        });
        services.AddSingleton<IDeliveriesDataStore, MongoDeliveriesDataStore>();

        ConfigureAuthentication(services);

        ConfigureOpenTelemetry(services);
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.AddAuthentication(BasicAuthenticationDefaults.AuthenticationScheme).AddBasic(options =>
        {
            options.AllowInsecureProtocol = true;
            options.Events = new BasicAuthenticationEvents
            {
                OnValidateCredentials = context =>
                {
                    if (context.Username == context.Password)
                    {
                        var claims = new List<Claim>
                        {
                            new(
                                ClaimTypes.NameIdentifier,
                                context.Username,
                                ClaimValueTypes.String,
                                context.Options.ClaimsIssuer),
                            new(
                                ClaimTypes.Name,
                                context.Username,
                                ClaimValueTypes.String,
                                context.Options.ClaimsIssuer)
                        };
                        switch (context.Username.ToLowerInvariant())
                        {
                            case "partner":
                                claims.Add(new Claim(ClaimTypes.Role, "partner", ClaimValueTypes.String,
                                    context.Options.ClaimsIssuer));
                                context.Principal = new ClaimsPrincipal(
                                    new ClaimsIdentity(claims, context.Scheme.Name));
                                context.Success();
                                break;

                            case "user":
                                claims.Add(new Claim(ClaimTypes.Role, "user", ClaimValueTypes.String,
                                    context.Options.ClaimsIssuer));
                                context.Principal = new ClaimsPrincipal(
                                    new ClaimsIdentity(claims, context.Scheme.Name));
                                context.Success();
                                break;
                        }
                    }

                    return Task.CompletedTask;
                }
            };
        });
    }

    private void ConfigureOpenTelemetry(IServiceCollection services)
    {
        var otEndpoint = Configuration.GetValue<string>("OTEL_EXPORTER_OTLP_ENDPOINT");

        services.AddOpenTelemetry().WithTracing(tcb =>
            {
                services.Configure<AspNetCoreInstrumentationOptions>(
                    Configuration.GetSection("AspNetCoreInstrumentation"));

                tcb = tcb
                    .AddSource(ServiceName)
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(ServiceName,
                                serviceVersion: typeof(Startup).Assembly.GetName().Version?.ToString()))
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddMongoDBInstrumentation();

                if (!string.IsNullOrWhiteSpace(otEndpoint)) tcb.AddOtlpExporter();
            })
            .WithMetrics(mcb =>
            {
                mcb = mcb
                    .AddAspNetCoreInstrumentation();

                if (!string.IsNullOrWhiteSpace(otEndpoint)) mcb.AddOtlpExporter();
            });

        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                if (!string.IsNullOrWhiteSpace(otEndpoint)) options.AddOtlpExporter();
            });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TT.Deliveries.Web.Api v1"));
        }


        app.UseHttpLogging();
        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(e => e.MapControllers());
    }
}