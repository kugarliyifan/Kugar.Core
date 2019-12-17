using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Kugar.Core.Printer
{
    /// <summary>
    ///     打印模块管理器
    /// </summary>
    public interface IPrint_ModulePrintProvider
    {
        /// <summary>
        ///     用于打印的实体类
        /// </summary>
        object PrintingData { set; get; }

        /// <summary>
        ///     返回当前模块的打印格式列表
        /// </summary>
        /// <returns></returns>
        IList<PrintFormatItemInfo> GetModulePrintFormatList();

        /// <summary>
        ///     当前打印模块的ID
        /// </summary>
        string ModuleID { set; get; }

        /// <summary>
        ///     打开指定格式的设计器
        /// </summary>
        /// <param name="formatItem"></param>
        /// <returns></returns>
        bool Design(PrintFormatItemInfo formatItem);

        /// <summary>
        ///     保存指定名称的打印格式
        /// </summary>
        /// <param name="name">打印格式名称</param>
        void Save(string name);

        /// <summary>
        ///     新增指定名称的打印格式
        /// </summary>
        /// <param name="name">打印格式名称</param>
        /// <param name="printFormat">打印格式</param>
        void Add(string name, string printFormat);

        /// <summary>
        ///     使用指定名称的打印格式进行预览
        /// </summary>
        /// <param name="name">打印格式名称</param>
        void Preview(string name);

        /// <summary>
        ///     调用指定名称的打印格式直接打印
        /// </summary>
        /// <param name="name">打印格式名称</param>
        void Print(string name);

        /// <summary>
        ///     使用默认打印格式打印数据
        /// </summary>
        void PrintByDefault();

        /// <summary>
        ///     删除指定名称的打印格式
        /// </summary>
        /// <param name="name">打印格式名称</param>
        void Delete(string name);

        /// <summary>
        ///     创建一个新的打印格式
        /// </summary>
        /// <returns></returns>
        PrintFormatItemInfo CreateNew();

        /// <summary>
        ///     指定一个打印格式为默认打印格式
        /// </summary>
        /// <param name="name">打印格式名称</param>
        void SetDefault(string name);

        /// <summary>
        ///     调用指定名称的打印格式,并输出为Excel
        /// </summary>
        /// <param name="name">打印格式名称</param>
        /// <param name="exportStream">导出的数据流</param>
        void ExportToExcel(string name, Stream exportStream);

        /// <summary>
        ///     调用指定名称的打印格式,并输出为PDF
        /// </summary>
        /// <param name="name">打印格式名称</param>
        /// <param name="exportStream">导出的数据流</param>
        void ExportToPDF(string name, Stream exportStream);
    }

    /// <summary>
    ///     打印支持器
    /// </summary>
    public interface IPrint_PrintProvider
    {
        /// <summary>
        ///     打印支持器的名称
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        ///     获取指定名称的打印模块管理
        /// </summary>
        /// <param name="moduleID">指定打印模块的ID</param>
        /// <param name="printData">将要打印的数据对象</param>
        /// <returns></returns>
        IPrint_ModulePrintProvider GetModuleProvider(string moduleID, object printData);
    }
}
