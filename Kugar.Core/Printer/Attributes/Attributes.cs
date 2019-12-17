using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Printer
{
    /// <summary>
    ///     标识一个字段是动态列
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PrintDynamicColumnAttribute : Attribute
    {
        public PrintDynamicColumnAttribute(string groupName, Type dataType)
        {
            GroupName = groupName;
            DataType = dataType;
        }

        public string GroupName { private set; get; }
        public Type DataType { private set; get; }
    }

    /// <summary>
    ///     标识一个属性为打印属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrintElementAttribute : Attribute
    {
        /// <summary>
        ///    
        /// </summary>
        /// <param name="displayText">显示的字段名</param>
        /// <param name="location">字段显示的位置 </param>
        /// <param name="displayType">字段显示的类型 </param>
        public PrintElementAttribute(string displayText, PrintElementLocation location, PrintElementDisplayType displayType = PrintElementDisplayType.Auto)
        {
            DisplayText = displayText;
            DisplayType = displayType;
            Location = location;
        }

        /// <summary>
        ///    显示的字段名
        /// </summary>
        public string DisplayText { private set; get; }

        public PrintElementLocation Location { private set; get; }

        public PrintElementDisplayType DisplayType { private set; get; }
    }

    /// <summary>
    ///     标识一个属性为明细表字段
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrintElementDetailListAttribute : Attribute
    {
        public PrintElementDetailListAttribute(string detailName)
        {
            DetailName = detailName;
        }

        public string DetailName { private set; get; }

    }

}
