using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class ProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider provider)
        {
            return (T)provider.GetService(typeof(T));
        }
    }
}
