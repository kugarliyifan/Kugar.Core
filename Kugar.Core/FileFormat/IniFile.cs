using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Kugar.Core.Collections;
using Kugar.Core.IO;
using Kugar.Core.BaseStruct;

namespace Kugar.Core.FileFormat
{
    /// <summary>
    ///     读写Ini类型的文件
    /// </summary>
    public class IniFile
    {
        private static readonly Regex SectionReg = new Regex("[" + Regex.Escape(" ") + "\t]*" + Regex.Escape("[") + ".*" + Regex.Escape("]\r\n"));

        private DictionaryEx<string, DictionaryEx<string, string>> cacheData = null;

        private IniFile(DictionaryEx<string, DictionaryEx<string, string>> data)
        {
            if (data == null)
            {
                throw new ArgumentOutOfRangeException("data");
            }
            cacheData = data;
        }

        public DictionaryEx<string, string> this[string sectionName]
        {
            get { return cacheData[sectionName]; }
            set { cacheData[sectionName] = value; }
        }

        public string FilePath { internal set; get; }

        public static IniFile LoadFile(string fileName)
        {
            var tempCache = new DictionaryEx<string, DictionaryEx<string, string>>();

            tempCache.IsAutoAddKeyPair = true;

            if (string.IsNullOrEmpty(fileName))
                return null;

            string Contents = FileManager.GetFileContents(fileName);

            //Regex Section = new Regex("[" + Regex.Escape(" ") + "\t]*" + Regex.Escape("[") + ".*" + Regex.Escape("]\r\n"));
            string[] Sections = SectionReg.Split(Contents);

            MatchCollection SectionHeaders = SectionReg.Matches(Contents);

            int Counter = 1;

            foreach (Match SectionHeader in SectionHeaders)
            {
                string[] Splitter = { "\r\n" };
                string[] Splitter2 = { "=" };
                string[] Items = Sections[Counter].Split(Splitter, StringSplitOptions.RemoveEmptyEntries);
                var SectionValues = new DictionaryEx<string, string>();

                SectionValues.IsAutoAddKeyPair = true;

                foreach (string Item in Items)
                {
                    SectionValues.Add(Item.Split(Splitter2, StringSplitOptions.None)[0], Item.Split(Splitter2, StringSplitOptions.None)[1]);
                }

                tempCache.Add(SectionHeader.Value.Replace("[", "").Replace("]\r\n", ""), SectionValues);

                ++Counter;
            }


            return new IniFile(tempCache) { FilePath = fileName };
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(this.FilePath))
                return;

            StringBuilder Builder = new StringBuilder();

            foreach (string Header in cacheData.Keys)
            {
                Builder.Append("[" + Header + "]\r\n");
                foreach (string Key in cacheData[Header].Keys)
                {
                    Builder.Append(Key + "=" + cacheData[Header][Key] + "\r\n");
                }
            }

            FileManager.SaveFile(Builder.ToString(), FilePath);
        }

    }


}
