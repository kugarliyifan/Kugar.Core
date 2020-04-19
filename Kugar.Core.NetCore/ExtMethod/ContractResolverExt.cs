using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Serialization;

namespace Kugar.Core.ExtMethod
{
    public static class ContractResolverExt
    {
        public static bool IsPropertyCamelCase(this IContractResolver resolver)
        {
            if (resolver is CamelCasePropertyNamesContractResolver)
            {
                return true;
            }
            else if (resolver is DefaultContractResolver r)
            {
                return r.NamingStrategy is CamelCaseNamingStrategy;
            }
            else
            {

            }

            return false;
        }
    }
}
