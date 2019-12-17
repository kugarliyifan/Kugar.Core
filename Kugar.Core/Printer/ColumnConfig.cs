using System.Collections.Generic;
using Kugar.Core.BaseStruct;
using System.Linq;
using Kugar.Core.ExtMethod;


namespace Kugar.Core.Printer
{
    public class ColumnConfig
    {
        private int _refCount = 0;

        public ColumnConfig()
        {
            HeaderTextList = new List<KeyValuePairEx<object,int>>();
            _refCount = 1;
            HeaderText = "";
        }

        public string HeaderText { set; get; }

        public int RefCount
        {
            get 
            {
                var temp = 0;
                temp = System.Threading.Interlocked.Exchange(ref _refCount, _refCount);
                return temp;
            }
            set { System.Threading.Interlocked.Exchange(ref _refCount, value); }
        }

        public void IncrementRefCount()
        {
            System.Threading.Interlocked.Increment(ref _refCount);
        }

        public void DecrementRefCount()
        {
            System.Threading.Interlocked.Decrement(ref _refCount);
        }

        public bool ContainsKey(object header)
        {
        	return HeaderTextList.Any(x=>x.Key.SafeEquals(header));
        }
        
        public void Add(object header)
        {
        	//var p=new KeyValuePairEx<object,int>(header,1)
        	
        	var pair=new KeyValuePairEx<object,int>(header,1);
        	
        	HeaderTextList.Add(pair);
        }
        
        public void Increment(object header)
        {
        	var pair=HeaderTextList.FirstOrDefault(x=>x.Key.SafeEquals(header));
        	
        	if (pair==null) {
        		Add(header);
        	}
        	else{
        		pair.Value+=1;
        	}
        	
        }
        
        public void Decrement(object header)
        {
        	
        	var pair=HeaderTextList.FirstOrDefault(x=>x.Key.SafeEquals(header));
        	
        	if (pair==null) {
        		throw new System.ArgumentOutOfRangeException("header");
        	}
        	else{
        		pair.Value-=1;
        	}
        }
        
        public void Remove(object header)
        {
        	var index=HeaderTextList.IndexOf(x=>x.Value.SafeEquals(header));
        	
        	if (index<0) {
        		return;
        	}
        	
        	HeaderTextList.RemoveAt(index);
        }
        
        public List<KeyValuePairEx<object,int>> HeaderTextList { get; private set; }
    }
}