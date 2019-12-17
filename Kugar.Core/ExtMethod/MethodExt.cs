using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.ExtMethod
{
    public class MethodExt
    {
        public static void InvokeIgoneError(Action invoke)
        {
            try
            {
                invoke();
            }
            catch (Exception)
            {
                
            }
        }
    }
}
