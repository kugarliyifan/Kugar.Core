using System;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Kugar.Core.SystemManager
{
    public static class RegistryManager
    {
        private static Dictionary<string, RegistryKey> mainKeyList = new Dictionary<string, RegistryKey>(StringComparer.InvariantCultureIgnoreCase);

        static RegistryManager()
        {
            mainKeyList.Add("HKEY_CURRENT_USER", Registry.CurrentUser);
            mainKeyList.Add("HKEY_CLASSES_ROOT", Registry.ClassesRoot);
            mainKeyList.Add("HKEY_LOCAL_MACHINE", Registry.LocalMachine);
            mainKeyList.Add("HKEY_USERS", Registry.Users);
            mainKeyList.Add("HKEY_CURRENT_CONFIG", Registry.CurrentConfig);
        }

        public static T GetKeyValue<T>(string regPath, string keyName)
        {
            RegistryKey[] klist = null;

            var registryKey = GetRegistryKeyByPath(regPath, out klist);

            if (registryKey == null)
            {
                throw new Exception("不存在指定路径");
            }

            var ret = (T)registryKey.GetValue(keyName);

            CloseKeyList(klist);

            return ret;
        }

        public static void SetKeyValue(string regPath, string keyName, object value, RegistryValueKind valueType)
        {
            var sPath = regPath.Split('\\');

            if (sPath.Length < 1)
            {
                throw new Exception("不存在指定路径");
            }

            if (!mainKeyList.ContainsKey(sPath[0]))
            {
                throw new Exception("不存在指定的主键：" + sPath[0]);
            }

            var registryKey = mainKeyList[sPath[0]];

            if (registryKey == null)
            {
                throw new Exception("不存在指定的主键：" + sPath[0]);
            }

            var kList = new List<RegistryKey>(sPath.Length);

            kList.Add(registryKey);

            for (int i = 1; i < sPath.Length; i++)
            {
                var tempKey = registryKey.OpenSubKey(sPath[i]);

                if (tempKey == null)
                {
                    tempKey = registryKey.CreateSubKey(sPath[i]);
                }

                kList.Add(tempKey);

                registryKey = tempKey;
            }

            if (registryKey != null)
            {
                //registryKey.DeleteSubKey(keyName, false);

                //registryKey.CreateSubKey(keyName, RegistryKeyPermissionCheck.ReadWriteSubTree);

                registryKey.SetValue(keyName, value, valueType);
            }

        }

        public static RegistryKey GetRegistryKeyByPath(string regPath, out RegistryKey[] keysList)
        {
            var sPath = regPath.Split('\\');


            if (sPath.Length < 1)
            {
                throw new Exception("不存在指定路径");
            }

            if (!mainKeyList.ContainsKey(sPath[0]))
            {
                throw new Exception("不存在指定的键：" + sPath[0]);
            }

            var tlist = new List<RegistryKey>(sPath.Length);


            var registryKey = mainKeyList[sPath[0]];

            tlist.Add(registryKey);

            if (registryKey == null)
            {
                throw new Exception("不存在指定的键：" + sPath[0]);
            }

            for (int i = 1; i < sPath.Length; i++)
            {
                registryKey = registryKey.OpenSubKey(sPath[i]);

                if (registryKey == null)
                {
                    throw new Exception("不存在指定的键：" + sPath[i]);
                }

                tlist.Add(registryKey);
            }

            keysList = tlist.ToArray();

            return registryKey;
        }

        public static void DeleteRegistryKey(string regPath, string keyName)
        {
            RegistryKey[] kList = null;

            try
            {
                var regKey = GetRegistryKeyByPath(regPath, out kList);
                if (regKey == null)
                {
                    return;
                }

                regKey.DeleteSubKeyTree(keyName);
            }
            catch (Exception)
            {
                return;
            }

            CloseKeyList(kList);

        }

        public static bool IsContansKey(string regPath, string keyName)
        {

            RegistryKey[] registryKeys = null;
            try
            {

                var key = GetRegistryKeyByPath(regPath, out registryKeys);

                if (key != null)
                {
                    var keyNameList = key.GetSubKeyNames();

                    foreach (var s in keyNameList)
                    {
                        if (s == keyName)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                if (registryKeys != null)
                {
                    CloseKeyList(registryKeys);
                }
            }
        }

        private static void CloseKeyList(RegistryKey[] keys)
        {
            if (keys == null || keys.Length <= 0)
            {
                return;
            }

            for (int i = keys.Length - 1; i < 0; i++)
            {
                keys[i].Close();
            }
        }

        public static RegistryKey StringToMainKey(string mainKeyName)
        {
            if (mainKeyList.ContainsKey(mainKeyName))
            {
                return mainKeyList[mainKeyName];
            }
            else
            {
                return null;
            }
        }
    }
}
