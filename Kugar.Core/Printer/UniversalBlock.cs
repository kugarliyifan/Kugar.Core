using System.Collections.Generic;

namespace Kugar.Core.Printer
{
    public class UniversalBlock
    {
        public UniversalBlock()
        {
            Fields = new List<PrintElementItem>();
        }

        public List<PrintElementItem> Fields { get; private set; }
    }

    public class UniversalDetailTable : UniversalBlock
    {
        public UniversalDetailTable()
        {
            DynamicColumnConfig = new List<PrintElementItem>();
        }

        public string BindingProperty { set; get; }

        public string DetailName
        {
            set;
            get;
        }

        public List<PrintElementItem> DynamicColumnConfig { set; get; }
    }
}