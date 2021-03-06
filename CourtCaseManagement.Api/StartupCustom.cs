﻿using CourtCaseManagement.Api.Swagger;
using CourtCaseManagement.ApplicationCore.Facades;
using CourtCaseManagement.ApplicationCore.Interfaces;
using CourtCaseManagement.ApplicationCore.Services;
using CourtCaseManagement.Infrastructure.Data;
using CourtCaseManagement.Infrastructure.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CourtCaseManagement.Api
{
    public class StartupCustom : IStartupCustom
    {
        public virtual void AddDatabaseConfigure(IServiceCollection services)
        {
            var connection = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

            services.AddEntityFrameworkNpgsql().AddDbContext<CatalogContext>(c => c.UseNpgsql(connection));
        }

        public virtual void AddApplicationCoreClassDependencyInject(IServiceCollection services)
        {
            services.AddScoped<IProcessFacade, ProcessFacade>();
            services.AddScoped<IResponsibleFacade, ResponsibleFacade>();

            services.AddScoped<IProcessService, ProcessService>();
            services.AddScoped<IResponsibleService, ResponsibleService>();

            services.AddScoped<IProcessRepository, ProcessRepository>();
            services.AddScoped<IResponsibleRepository, ResponsibleRepository>();
            services.AddScoped<ISituationRepository, SituationRepository>();
        }

        public virtual void AddInfrastructureClassDependencyInject(IServiceCollection services)
        {
            services.AddScoped(typeof(IAsyncRepository<>), typeof(EfRepository<>));
            services.AddScoped(typeof(IAppLogger<>), typeof(LoggerAdapter<>));
        }

        public virtual void AddSwaggerConfigure(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Court Case Management API",
                    Description = "Court Case Management Web API",
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.OperationFilter<RemoveVersionParameterFilter>();
                c.DocumentFilter<ReplaceVersionWithExactValueInPathFilter>();
                c.DocumentFilter<BasePathFilter>("/courtCaseManagement");
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                c.CustomSchemaIds(schema => $"{schema.FullName}{Regex.Match(schema.Namespace.ToString(), @"(?<=\.)v[0-9]*$")}");
            });

            services.AddSwaggerGenNewtonsoftSupport();

            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.Converters.Add(new StringEnumConverter()));
        }

        public virtual void SwaggerConfigure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();

            app.UseSwagger(c =>
            {
                c.RouteTemplate = "swagger/{documentName}/swagger.json";
            });

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Court Case Management V1");
                c.RoutePrefix = "swagger";

                c.InjectStylesheet("/css/custom_swagger.css");
            });
        }
    }
}