﻿using CourtCaseManagement.Api;
using CourtCaseManagement.Test.Scenario.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace CourtCaseManagement.Test.Scenario.WebApi
{
    internal class TestServerFixture : WebApplicationFactory<Startup<Test.Scenario.WebApi.StartupCustom>>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            Environment.SetEnvironmentVariable("DB_CONNECTION_STRING", string.Empty);
        }

        protected override IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup<Test.Scenario.WebApi.StartupCustom>>();
                });

        public void CreateHttpClient()
        {
            Variable.Client = CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });
        }
    }
}