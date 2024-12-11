
namespace OrderMicroservice.Repositories.Services
{
    public class TempFileCleaner : IHostedService, IDisposable
    {
        private readonly ILogger<TempFileCleaner> _logger;

        public TempFileCleaner(ILogger<TempFileCleaner> logger)
        {
            _logger = logger;
        }

        private Timer _timer;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(Cleanup, null, TimeSpan.Zero, TimeSpan.FromHours(1));
            return Task.CompletedTask;
        }

        private void Cleanup(object state)
        {
            var rootPath = Directory.GetCurrentDirectory();
            var tempDir = Path.Combine(rootPath, "wwwroot", "temp");

            if (!Directory.Exists(tempDir))
            {
                _logger.LogWarning("Temp directory does not exist.");
                return;
            }

            var files = Directory.GetFiles(tempDir);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < DateTime.Now.AddHours(-1))
                {
                    _logger.LogInformation($"Deleting file: {fileInfo.Name}");
                    fileInfo.Delete();
                }
            }
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
