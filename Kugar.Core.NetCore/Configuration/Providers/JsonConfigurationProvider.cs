using System;
using System.Collections.Generic;
using System.Text;

namespace Kugar.Core.Configuration.Providers
{
    public class JsonConfigurationProvider: MarshalByRefObject, ILocalCustomConfigProvider
    {
        public IEnumerable<CustomConfigItem> Load()
        {
            throw new NotImplementedException();
        }

        public bool Write(IEnumerable<CustomConfigItem> configList)
        {
            throw new NotImplementedException();
        }
    }
}
