using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GenericHostSample
{
    public class SampleHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private SampleOptions _options;

        public SampleHostedService(ILogger<SampleHostedService> logger, IConfiguration configuration, IOptions<SampleOptions> options, IOptionsMonitor<SampleOptions> optionsMonitor)
        {
            _logger = logger;
            _configuration = configuration;
            _options = options.Value;

            optionsMonitor.OnChange(value =>
            {
                _logger.LogWarning($"Options OnChanged. New Value - {value}");
                _options = value;
            });
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Value in SampleHostedService - {_options}");

            var es = _configuration.GetValue<string?>("Sample:eAStringProperty");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}