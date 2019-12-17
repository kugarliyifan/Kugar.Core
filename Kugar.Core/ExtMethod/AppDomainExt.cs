using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Kugar.Core.ExtMethod
{
    public static class AppDomainExt
    {
        /// <summary>
        ///     新建一个AppDomain,并在该域内创建一个指定type类型的实例,并返回为指定类型T(接口)
        /// </summary>
        /// <typeparam name="T">返回的类型,建议为接口</typeparam>
        /// <param name="type">将要运行在新AppDomain的类型</param>
        /// <param name="paramList">构造函数的参数</param>
        /// <returns></returns>
        public static T CreateToNewAppDomain<T>(this Type type, params object[] paramList)
        {
            var appDomain = createNewDomain("");
            return CreateToAppDomain<T>(type, appDomain, paramList);
        }

        /// <summary>
        ///    在指定AppDomain内创建一个指定type类型的实例,并返回为指定类型T(接口)
        /// </summary>
        /// <typeparam name="T">返回的类型,建议为接口</typeparam>
        /// <param name="appDomain">指定的应用程序域</param>
        /// <param name="type">将要运行在新AppDomain的类型</param>
        /// <param name="paramList">构造函数的参数</param>
        /// <returns></returns>
        public static T CreateType<T>(this AppDomain appDomain, Type type, params object[] paramList)
        {
            return CreateToAppDomain<T>(type, appDomain, paramList);
        }

        /// <summary>
        ///    在指定AppDomain内创建一个指定type类型的实例,并返回为指定类型T(接口)
        /// </summary>
        /// <typeparam name="T">返回的类型,建议为接口</typeparam>
        /// <param name="appDomain">指定的应用程序域</param>
        /// <param name="type">将要运行在新AppDomain的类型</param>
        /// <param name="paramList">构造函数的参数</param>
        /// <returns></returns>
        public static T CreateToAppDomain<T>(this Type type, AppDomain appDomain, params object[] paramList)
        {
            var assemblyName = type.Assembly.GetName().FullName;
            var typeName = typeof(T).FullName;

            //var appDomain = createNewDomain("");
            appDomain.AppendPrivatePath(Path.GetDirectoryName(type.Assembly.Location));
            appDomain.AppendPrivatePath(Path.GetDirectoryName(typeof(T).Assembly.Location));

            try
            {
                var retInterface = (T)appDomain.CreateInstanceAndUnwrap(assemblyName,
                                                                      typeName
                                                                      , false
                                                                      , BindingFlags.Default
                                                                      , null
                                                                      , paramList
                                                                      , Thread.CurrentThread.CurrentCulture
                                                                      , null
                                                                      , null);


                return retInterface;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static AppDomain createNewDomain(string name="")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "KugarDomain_" + ShortGuid.NewGuid().ToString();
            }

            var setup = new AppDomainSetup();
            setup.ApplicationName = "ApplicationLoader";
            setup.LoaderOptimization = LoaderOptimization.MultiDomain;
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            setup.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            setup.CachePath = setup.ApplicationBase;
            //setup.ShadowCopyFiles = "true";
            //setup.ShadowCopyDirectories = setup.ApplicationBase;

            var appDomain = AppDomain.CreateDomain(name, null, setup);

            return appDomain;
        }
    }
}
