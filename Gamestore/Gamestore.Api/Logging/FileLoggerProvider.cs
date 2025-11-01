namespace Gamestore.Api.Logging;

public class FileLoggerProvider(FileLoggerConfiguration config) : ILoggerProvider
{
    private readonly FileLoggerConfiguration _config = config;
    private bool _disposed;

    ~FileLoggerProvider()
    {
        Dispose(false);
    }

    public ILogger CreateLogger(string categoryName)
    {
        if (!Directory.Exists(_config.LogPath))
        {
            Directory.CreateDirectory(_config.LogPath);
        }

        return new FileLogger(categoryName, _config);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // No managed resources to dispose in this implementation, but if there were, they would be disposed here.
            }

            _disposed = true;
        }
    }
}
