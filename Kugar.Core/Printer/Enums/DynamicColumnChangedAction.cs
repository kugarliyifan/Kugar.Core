namespace Kugar.Core.Printer
{
    public enum DynamicColumnChangedAction
    {
        /// <summary>
        ///    新增动态列
        /// </summary>
        AddColumn,

        /// <summary>
        ///     删除动态列
        /// </summary>
        RemoveColumn,

        /// <summary>
        ///     动态列子列刷新
        /// </summary>
        RefreshChildColumn
    }
}