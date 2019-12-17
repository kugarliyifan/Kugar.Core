using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kugar.Core.BaseStruct
{
    public class DateTimeRanage
    {
        public DateTime StartDt { set; get; }

        public DateTime EndDt { set; get; }
    }

    public class DateTimeRangeNullable
    {
        public DateTime? StartDt { set; get; }

        public DateTime? EndDt { set; get; }
    }
}
