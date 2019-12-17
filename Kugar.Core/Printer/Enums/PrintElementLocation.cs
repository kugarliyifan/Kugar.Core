namespace Kugar.Core.Printer
{
    /// <summary>
    ///     字段所处位置<br/>
    /// </summary>
    /// <remarks>
    ///     该标识位置,在标识的属性为一个Class属性时,,Class下的所有字段则忽略位置定义
    /// </remarks>
    public enum PrintElementLocation
    {
        /// <summary>
        ///     指定该元素的位置在报表头
        /// </summary>
        ReportHead,

        /// <summary>
        ///     指定该元素的位置在每页的页头
        /// </summary>
        PageHead,

        /// <summary>
        ///     指定该元素的位置在每页的页脚
        /// </summary>
        PageFooter,

        /// <summary>
        ///     指定该元素的位置在报表脚
        /// </summary>
        ReportFooter,

        /// <summary>
        ///     指定该元素是明细
        /// </summary>
        DetailInside,
    }
}