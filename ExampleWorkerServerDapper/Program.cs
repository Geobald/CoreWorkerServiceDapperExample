using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ExampleWorkerServerDapper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var applicationBasePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config
                        .SetBasePath(applicationBasePath)
                        .AddJsonFile($"appsettings.json", false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<ITestRepository>(repo =>
                        new TestRepository(hostContext.Configuration.GetSection($"ConnectionString").Value));
                    services.AddHostedService<PopulateDatabase>();
                });

            return hostBuilder;
        }
    }
}
