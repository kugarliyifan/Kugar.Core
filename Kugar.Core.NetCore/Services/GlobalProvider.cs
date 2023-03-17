using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Kugar.Core.Services
{
    /// <summary>
    /// 全局的ServiceProvider
    /// </summary>
    public static class GlobalProvider
    {
        public static IServiceProvider Provider { set; get; }

        public static IServiceCollection Services { set; get; }

        public static IServiceCollection RegisterGlobalProvider(this IServiceCollection services)
        {
            GlobalProvider.Services = services;
            services.AddHostedService<GlobalProviderTask>();

            return services;
        }
    }

    public class GlobalProviderTask : BackgroundService
    {
        public GlobalProviderTask(IServiceProvider provider)
        {
            GlobalProvider.Provider = provider;
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
             return Task.CompletedTask;
        }
    }
}
