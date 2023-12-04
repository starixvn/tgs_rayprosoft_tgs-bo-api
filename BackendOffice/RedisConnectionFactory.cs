using System;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using TGS.Core.Infrastructures.Redis;

namespace TGS.Apis.BackendOffice;

internal class RedisConnectionFactory : IRedisConnectionFactory, IDisposable
{
    private readonly ConnectionMultiplexer _connectionMultiplexer;
    private bool _disposedValue;

    public RedisConnectionFactory(
        IOptions<RedisOptions> optionsAccessor)
    {
        ArgumentNullException.ThrowIfNull(optionsAccessor);

        var options = optionsAccessor.Value;

        var connectionOptions = ConfigurationOptions.Parse(options.ConnectionString);

        _connectionMultiplexer = ConnectionMultiplexer.Connect(connectionOptions);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public IConnectionMultiplexer GetConnection()
        => _connectionMultiplexer;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _connectionMultiplexer?.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            _disposedValue = true;
        }
    }
}
