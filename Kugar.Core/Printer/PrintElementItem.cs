using System;

namespace Kugar.Core.Printer
{
    /// <summary>
    ///     打印字段定义类
    /// </summary>
    public class PrintElementItem
    {
        /// <summary>
        ///     打印字段的唯一名称
        /// </summary>
        public virtual string Name { set; get; }

        /// <summary>
        ///     打印字段标头
        /// </summary>
        public virtual string HeaderText { set; get; }

        /// <summary>
        ///     打印字段数据类型
        /// </summary>
        public virtual Type DataType { set; get; }

        /// <summary>
        ///     打印字段的源字段
        /// </summary>
        public virtual string BindingColumn { set; get; }

        /// <summary>
        ///     如果BindingColumn为空,则显示该属性值
        /// </summary>
        public virtual string Text { set; get; }

        /// <summary>
        ///     打印字段显示的类型
        /// </summary>
        public virtual PrintElementDisplayType DisplayType { set; get; }

        public bool IsDynamicColumn { set; get; }

        public override bool Equals(object obj)
        {
            return Name == ((PrintElementItem)obj).Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public PrintElementItem Copy()
        {
            var pe = new PrintElementItem();

            pe.BindingColumn = this.BindingColumn;
            pe.HeaderText = this.HeaderText;
            pe.Name = this.Name;
            pe.DataType = this.DataType;
            pe.Text = this.Text;
            pe.DisplayType = this.DisplayType;

            return pe;
        }

    }
}