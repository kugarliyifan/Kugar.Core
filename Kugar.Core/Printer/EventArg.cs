using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Printer
{
    public class DynamicColumnValueChangedEventArgs : EventArgs
    {
        /// <summary>
        ///     构造函数 添加数据时使用
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="value"></param>
        public DynamicColumnValueChangedEventArgs(DynamicColumn column, object headerText, object newValue)
        {
            Action = DynamicColumnValueChangedAction.Add;
            HeaderText = headerText;
            NewValue = newValue;
            Column = column;
        }

        /// <summary>
        ///     构造函数 修改数据的时候使用
        /// </summary>
        /// <param name="headerText"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        public DynamicColumnValueChangedEventArgs(DynamicColumn column, object headerText, object oldValue, object newValue)
        {
            Action = DynamicColumnValueChangedAction.Modify;
            HeaderText = headerText;
            NewValue = newValue;
            OldValue = oldValue;
            Column = column;
        }

        /// <summary>
        ///     构造函数 删除数据时使用
        /// </summary>
        /// <param name="headerText"></param>
        public DynamicColumnValueChangedEventArgs(DynamicColumn column, object headerText)
        {
            Action = DynamicColumnValueChangedAction.Remove;
            HeaderText = headerText;
            Column = column;
        }

        public DynamicColumn Column { private set; get; }

        public DynamicColumnValueChangedAction Action { private set; get; }

        public object HeaderText { private set; get; }

        public object NewValue { private set; get; }

        public object OldValue { private set; get; }

    }

    public class DynamicColumnChangedEventArgs<T> : EventArgs where T : DynamicColumn
    {
        public DynamicColumnChangedEventArgs(DynamicColumnChangedAction action, string columnName)
        {
            ColumnName = columnName;
            Action = action;
        }

        public string ColumnName { private set; get; }
        public DynamicColumnChangedAction Action { private set; get; }
    }
}
