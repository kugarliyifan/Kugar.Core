using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class IOExtMethod
    {

        public static string[] ToFilePathStr(this IEnumerable<FileInfo> ff )
        {
            var list = new List<string>();

            foreach (var fileInfo in ff)
            {
                list.Add(fileInfo.FullName);
            }

            return list.ToArray();
        }
    }
}
