using System.Collections.Generic;

namespace Kugar.Core.Collections
{
    public class MutileValuesDictionary<TKey,TValue>:Dictionary<TKey,List<TValue>>
    {
        public MutileValuesDictionary<TKey,TValue> Add(TKey key,params TValue[] values)
        {
            var valueList = base[key];

            bool isSame;

            for (int i = 0; i < values.Length; i++)
            {
                isSame = false;

                var item = valueList[i];

                for (int j = 0; j < valueList.Count; j++)
                {
                    if (Equals(values[j],item))
                    {
                        isSame = true;
                        break;
                    }                    
                }

                if (!isSame)
                {
                    valueList.Add(item);
                }
            }
            return this;
        }
    }
}
