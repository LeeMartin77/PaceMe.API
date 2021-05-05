using System;
using PaceMe.Storage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaceMe.Storage.Repository;
using Microsoft.Extensions.Configuration;
using PaceMe.FunctionApp.Authentication;
using PaceMe.Storage.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(PaceMe.FunctionApp.Startup))]

namespace PaceMe.FunctionApp
{
    public record MsalConfig {
        public string OpenIdConfig { get; set; }
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
    }
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<MsalConfig>(new MsalConfig{
                OpenIdConfig = Environment.GetEnvironmentVariable("OpenIdConfig"),
                ValidAudience = Environment.GetEnvironmentVariable("ValidAudience"),
                ValidIssuer = Environment.GetEnvironmentVariable("ValidIssuer")
            });
            builder.Services.AddSingleton<IRequestAuthenticator, RequestAuthenticator>();

            builder.Services.AddSingleton<ITrainingPlanRepository, TrainingPlanRepository>();
            builder.Services.AddSingleton<ITrainingPlanActivityRepository, TrainingPlanActivityRepository>();
            builder.Services.AddSingleton<ITrainingPlanActivitySegmentRepository, TrainingPlanActivitySegmentRepository>();
        }
    }


}