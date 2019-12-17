using System.Threading;

namespace Kugar.Core.BaseStruct
{
	/// <summary>
	/// Description of AtomInt16.
	/// </summary>
	public class AtomInt16
	{
		private int _value; 
		
		public AtomInt16(short initValue)
		{
			_value=initValue;
			
			//Interlocked.Decrement
		}

	    public short Value
	    {
	        get { return (short) _value; }
            set { Interlocked.Exchange(ref _value, value); }
	    }
		
		public short Increment()
		{
            return (short)Interlocked.Increment(ref _value) ;
		}
		
		public short Decrement()
		{
            return (short)Interlocked.Decrement(ref _value);
		}

        public short Add(short addValue)
        {
            return (short) Interlocked.Add(ref _value, addValue);
        }

        public short Exchange(short value)
        {
            return (short) Interlocked.Exchange(ref _value, value);
        }
	}
}
