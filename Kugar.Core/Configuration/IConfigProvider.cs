using System.Collections.Generic;

namespace Kugar.Core.Configuration
{
    /// <summary>
    ///     配置项加载及保存提供器
    /// </summary>
    /// <remarks>
    ///     XML读写配置请使用： <br/>
    ///     数据库读写配置请调用Kugar.Core.CustomConfig.DatabaseProvider；
    /// </remarks>
    public interface ICustomConfigProvider
    {
        IEnumerable<CustomConfigItem> Load();
        bool Write(IEnumerable<CustomConfigItem> configList);

    }

    /// <summary>
    ///     本地配置节的支持器,该支持器读取后的配置项不被序列化
    /// </summary>
    public interface ILocalCustomConfigProvider : ICustomConfigProvider { }
}
