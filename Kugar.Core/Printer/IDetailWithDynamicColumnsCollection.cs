using System.Collections.Generic;

namespace Kugar.Core.Printer
{
    public interface IDetailWithDynamicColumnsCollection
    {
        Dictionary<string, ColumnConfig> DynamicColumns { get; }

        /// <summary>
        ///     当动态列中的数据有变化时引发
        /// </summary>
        event DynamicColumnValueChanged DynamicColumnValueChanged;

        /// <summary>
        ///     当新增的行中,有未添加的动态列组,则引发该事件
        /// </summary>
        event DynamicColumnChanged<DynamicColumn> DynamicColumnChanged;
    }
}