using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kugar.Core.Services
{
    public class FuncBackgroundService : BackgroundServiceEx
    {
        internal event FuncBackgroundServiceHandler _handler;

        public FuncBackgroundService( IServiceProvider provider) : base(provider)
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_handler!=null)
            {
                await _handler.Invoke(Provider);
            }
        }
    }
}