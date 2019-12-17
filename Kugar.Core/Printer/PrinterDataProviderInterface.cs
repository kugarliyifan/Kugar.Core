using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.Printer
{
    /// <summary>
    ///     打工功能 的模块数据支持,用于提供打印模块的增删查改等功能
    /// </summary>
    public interface IPrint_ModulePrintDataProvider
    {
        /// <summary>
        ///     获取指定模块的所有打印格式列表
        /// </summary>
        /// <returns></returns>
        IList<PrintFormatItemInfo> GetModulePrintFormatList();

        /// <summary>
        ///     获取指定名称的打印格式字符串
        /// </summary>
        /// <param name="printFormatName">打印格式的名称或ID</param>
        /// <returns></returns>
        string GetModulePrintFormatByName(string printFormatName);

        /// <summary>
        ///     获取当前模块的默认打印格式
        /// </summary>
        /// <returns></returns>
        string GetDefaultFormat();

        /// <summary>
        ///     添加一个指定名称的打印格式
        /// </summary>
        /// <param name="name">打印格式的名称</param>
        /// <param name="formatData">打印格式序列化后的字符串</param>
        void Add(string name, string formatData);

        /// <summary>
        ///     更新指定名称的打印格式
        /// </summary>
        /// <param name="name">打印格式名称</param>
        /// <param name="formatData">打印格式字符串</param>
        void Update(string name, string formatData);

        /// <summary>
        ///     删除一个指定名称的打印格式
        /// </summary>
        /// <param name="name"></param>
        void Delete(string name);

        /// <summary>
        ///     设置一个指定名称的打印格式为默认打印格式
        /// </summary>
        /// <param name="name"></param>
        void SetDefault(string name);
    }

    public interface IPrint_DataProvider
    {
        /// <summary>
        ///     获取指定模块名称的数据支持器
        /// </summary>
        /// <param name="moduleID"></param>
        /// <returns></returns>
        IPrint_ModulePrintDataProvider GetDataProviderByModuleID(string moduleID);
    }
}
