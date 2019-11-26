using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExampleWorkerServerDapper
{
    public class PopulateDatabase : BackgroundService
    {
        private readonly ILogger<PopulateDatabase> _logger;
        private readonly ITestRepository _testRepository;

        public PopulateDatabase(ILogger<PopulateDatabase> logger, ITestRepository testRepository)
        {
            _logger = logger;
            _testRepository = testRepository;
        }

        public void Do(CancellationToken stoppingToken)
        {
            bool truncate;
            bool populate;
            try
            {
                truncate = _testRepository.Truncate();
                if (truncate)
                {
                    populate = _testRepository.Populate();
                }
                else
                {
                    throw new Exception("Issues with truncating table.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally 
            {
                base.StopAsync(stoppingToken);
                base.Dispose();
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Do(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
