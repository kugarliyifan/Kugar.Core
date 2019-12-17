using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class ObjectContextExtMethod
    {
        public static IEnumerable<ObjectQuery> GetAllTableProperty(this ObjectContext oc)
        {
            var plist = oc.GetType().GetProperties();

            foreach (var propertyInfo in plist)
            {
                if (propertyInfo.PropertyType.IsSubclassOf(typeof(ObjectQuery)) || propertyInfo.PropertyType.IsSubclassOf(typeof(DbSet<>)))
                {
                    yield return (ObjectQuery)propertyInfo.GetValue(oc, null);
                }
            }
        }

    }
}
