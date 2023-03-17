using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Kugar.Core.Services
{

    public abstract class BackgroundServiceEx:BackgroundService
    {
        protected BackgroundServiceEx(IServiceProvider provider)
        {
            Provider = provider;
        }

        protected T GetService<T>()
        {
            return (T)this.Provider.GetService(typeof(T));
        }

        protected object GetService(Type type)
        {
            return this.Provider.GetService(type);
        }

        protected IServiceProvider Provider { get; } = null;
    }

    public delegate Task FuncBackgroundServiceHandler(IServiceProvider provider);
}
