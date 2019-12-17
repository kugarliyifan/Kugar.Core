using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;

namespace Kugar.Core.ExtMethod
{
#if NET45
    


    public static class RegistryExtMethod
    {
        public static RegistryKey OpenOrCreate(this RegistryKey parentKey, string keyPath)
        {
            return OpenOrCreate(parentKey, keyPath, false);
        }

        public static RegistryKey OpenOrCreate(this RegistryKey parentKey, string keyPath,bool isWritable)
        {
            var key = parentKey.OpenSubKey(keyPath, isWritable);

            if (key!=null)
            {
                return key;
            }
            else
            {
                return parentKey.CreateSubKey(keyPath);
            }
        }
    }

#endif
}
