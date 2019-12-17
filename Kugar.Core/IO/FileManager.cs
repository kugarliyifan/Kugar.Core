using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.IO
{
    /// <summary>
    ///     文件管理,负责打开,读取或者写入
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        ///     读取指定文件的内容
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <param name="TimeOut">读取超时时间</param>
        /// <returns></returns>
        public static string GetFileContents(string FileName, int TimeOut = 5000)
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException("FileName");
            if (!FileExists(FileName))
                return "";
            StreamReader Reader = null;
            int StartTime = System.Environment.TickCount;
            try
            {
                bool Opened = false;
                while (!Opened)
                {
                    try
                    {
                        if (System.Environment.TickCount - StartTime >= TimeOut)
                            throw new System.IO.IOException("File opening timed out");
                        Reader = File.OpenText(FileName);
                        Opened = true;
                    }
                    catch (System.IO.IOException) { throw; }
                }
                string Contents = Reader.ReadToEnd();
                Reader.Close();
                return Contents;
            }
            catch { throw; }
            finally
            {
                if (Reader != null)
                {
                    Reader.Close();
                    Reader.Dispose();
                }
            }
        }

        public static void GetFileContents(string FileName, out byte[] Output, int TimeOut = 5000)
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException("FileName");
            if (!FileExists(FileName))
            {
                Output = null;
                return;
            }
            FileStream Reader = null;
            int StartTime = System.Environment.TickCount;
            try
            {
                bool Opened = false;
                while (!Opened)
                {
                    try
                    {
                        if (System.Environment.TickCount - StartTime >= TimeOut)
                            throw new System.IO.IOException("File opening timed out");
                        Reader = File.OpenRead(FileName);
                        Opened = true;
                    }
                    catch (System.IO.IOException) { throw; }
                }
                byte[] Buffer = new byte[1024];
                using (MemoryStream TempReader = new MemoryStream())
                {
                    while (true)
                    {
                        int Count = Reader.Read(Buffer, 0, 1024);
                        TempReader.Write(Buffer, 0, Count);
                        if (Count < 1024)
                            break;
                    }
                    Reader.Close();
                    Output = TempReader.ToArray();
                    TempReader.Close();
                }
            }
            catch
            {
                Output = null;
                throw;
            }
            finally
            {
                if (Reader != null)
                {
                    Reader.Close();
                    Reader.Dispose();
                }
            }
        }

        public static string GetFileContents(Uri FileName, string UserName = "", string Password = "")
        {
            if (FileName == null)
                throw new ArgumentNullException("FileName");
            using (WebClient Client = new WebClient())
            {
                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                    Client.Credentials = new NetworkCredential(UserName, Password);
                using (StreamReader Reader = new StreamReader(Client.OpenRead(FileName)))
                {
                    string Contents = Reader.ReadToEnd();
                    Reader.Close();
                    return Contents;
                }
            }
        }

        public static void GetFileContents(Uri FileName, out Stream OutputStream, out WebClient Client, string UserName = "", string Password = "")
        {
            if (FileName == null)
                throw new ArgumentNullException("FileName");
            Client = new WebClient();
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                Client.Credentials = new NetworkCredential(UserName, Password);
            OutputStream = Client.OpenRead(FileName);
        }

        /// <summary>
        ///     将内容写入到指定的文件
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="FileName"></param>
        /// <param name="Append"></param>
        public static void SaveFile(string Content, string FileName, bool Append = false)
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException("FileName");
            int Index = FileName.LastIndexOf('/');
            if (Index <= 0)
                Index = FileName.LastIndexOf('\\');
            if (Index <= 0)
                throw new Exception("Directory must be specified for the file");
            string Directory = FileName.Remove(Index) + "/";
            CreateDirectory(Directory);
            FileStream Writer = null;
            try
            {
                byte[] ContentBytes = Encoding.UTF8.GetBytes(Content);
                bool Opened = false;
                while (!Opened)
                {
                    try
                    {
                        if (Append)
                        {
                            Writer = File.Open(FileName, FileMode.Append, FileAccess.Write, FileShare.None);
                        }
                        else
                        {
                            Writer = File.Open(FileName, FileMode.Create, FileAccess.Write, FileShare.None);
                        }
                        Opened = true;
                    }
                    catch (System.IO.IOException) { throw; }
                }
                Writer.Write(ContentBytes, 0, ContentBytes.Length);
                Writer.Close();
            }
            catch { throw; }
            finally
            {
                if (Writer != null)
                {
                    Writer.Close();
                    Writer.Dispose();
                }
            }
        }

        /// <summary>
        ///     重命名文件
        /// </summary>
        /// <param name="FileName">原文件名</param>
        /// <param name="NewFileName">新文件名</param>
        public static void RenameFile(string FileName, string NewFileName)
        {
            if (string.IsNullOrEmpty(FileName) || FileExists(FileName))
                throw new ArgumentNullException("FileName");
            if (string.IsNullOrEmpty(NewFileName))
                throw new ArgumentNullException("NewFileName");

            File.Move(FileName, NewFileName);
        }

        /// <summary>
        ///     判断文件是否存在
        /// </summary>
        /// <param name="FileName">文件名路径</param>
        /// <returns></returns>
        public static bool FileExists(string FileName)
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException("FileName");
            return File.Exists(FileName);
        }

        /// <summary>
        ///     判断一个目录是否存在
        /// </summary>
        /// <param name="DirectoryPath">目录路径</param>
        /// <returns></returns>
        public static bool DirectoryExists(string DirectoryPath)
        {
            if (string.IsNullOrEmpty(DirectoryPath))
                throw new ArgumentNullException("DirectoryPath");
            return Directory.Exists(DirectoryPath);
        }

        /// <summary>
        ///     新建一个文件夹
        /// </summary>
        /// <param name="DirectoryPath"></param>
        public static void CreateDirectory(string DirectoryPath)
        {
            if (string.IsNullOrEmpty(DirectoryPath))
                throw new ArgumentNullException("DirectoryPath");
            if (!DirectoryExists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        ///     读取指定路径下的所有文件夹
        /// </summary>
        /// <param name="DirectoryPath"></param>
        /// <returns></returns>
        public static List<DirectoryInfo> GetDirectoryList(string DirectoryPath)
        {
            if (string.IsNullOrEmpty(DirectoryPath))
                throw new ArgumentNullException("DirectoryPath");
            List<DirectoryInfo> Directories = new List<DirectoryInfo>();
            if (DirectoryExists(DirectoryPath))
            {
                DirectoryInfo Directory = new DirectoryInfo(DirectoryPath);
                DirectoryInfo[] SubDirectories = Directory.GetDirectories();
                foreach (DirectoryInfo SubDirectory in SubDirectories)
                {
                    Directories.Add(SubDirectory);
                }
            }
            return Directories;
        }

        public static FileInfo[] GetFileList(string directoryPath, bool Recursive = false)
        {
            return GetFileList(directoryPath, "*", SearchOption.TopDirectoryOnly, Recursive);
        }

        public static FileInfo[] GetFileList(string directoryPath, string searchPattern, bool Recursive = false)
        {
            if (searchPattern.IsNullOrEmpty())
            {
                searchPattern = "*";
            }

            return GetFileList(directoryPath, searchPattern, SearchOption.TopDirectoryOnly, Recursive);
        }

        /// <summary>
        ///     读取指定文件夹下的所有文件
        /// </summary>
        /// <param name="directoryPath">指定的文件夹路径</param>
        /// <param name="Recursive">是否递归获取子目录的文件</param>
        /// <returns></returns>
        public static FileInfo[] GetFileList(string directoryPath,string searchPattern,SearchOption searchOption, bool Recursive = false)
        {
            if (string.IsNullOrEmpty(directoryPath))
                throw new ArgumentNullException("DirectoryPath");
            List<FileInfo> Files = new List<FileInfo>();
            if (DirectoryExists(directoryPath))
            {
                DirectoryInfo Directory = new DirectoryInfo(directoryPath);
                Files.AddRange(Directory.GetFiles(searchPattern,searchOption));
                if (Recursive)
                {
                    DirectoryInfo[] SubDirectories = Directory.GetDirectories();
                    foreach (DirectoryInfo SubDirectory in SubDirectories)
                    {
                        Files.AddRange(GetFileList(SubDirectory.FullName, true));
                    }
                }
            }
            return Files.ToArray();
        }

        /// <summary>
        ///     复制整个文件夹
        /// </summary>
        /// <param name="Source">源文件夹路径</param>
        /// <param name="Destination">目标文件夹路径</param>
        /// <param name="Recursive">是否递归复制子文件夹</param>
        /// <param name="Options">复制的配置</param>
        /// <param name="fileCopyProgressCallback">文件拷贝进度,当复制完一个文件之后,会回调一次该函数</param>
        public static void CopyDirectory(string Source, 
                                                          string Destination, 
                                                          bool Recursive = true, 
                                                          CopyOptions Options = CopyOptions.CopyAlways,
                                                          FileCopyProgress fileCopyProgressCallback=null
            )
        {
            if (string.IsNullOrEmpty(Source))
                throw new ArgumentNullException("Source");
            if (string.IsNullOrEmpty(Destination))
                throw new ArgumentNullException("Destination");
            if (!DirectoryExists(Source))
                throw new ArgumentException("Source directory does not exist");

            DirectoryInfo SourceInfo = new DirectoryInfo(Source);
            DirectoryInfo DestinationInfo = new DirectoryInfo(Destination);

            CreateDirectory(Destination);
            FileInfo[] Files = GetFileList(Source);

            bool isCanCopy = true;

            foreach (FileInfo File in Files)
            {
                string filePath = Path.Combine(DestinationInfo.FullName, File.Name);

                if (Options == CopyOptions.CopyAlways)
                {
                    //File.CopyTo(Path.Combine(DestinationInfo.FullName, File.Name), true);
                }
                else if (Options == CopyOptions.CopyIfNewer)
                {
                    if (FileExists(filePath))
                    {
                        FileInfo FileInfo = new FileInfo(filePath);
                        if (FileInfo.LastWriteTime.CompareTo(File.LastWriteTime) > 0)
                        {
                            isCanCopy = false;
                            //File.CopyTo(Path.Combine(DestinationInfo.FullName, File.Name), true);
                        }
                    }
                    else
                    {
                        isCanCopy = true;
                        //File.CopyTo(Path.Combine(DestinationInfo.FullName, File.Name), true);
                    }
                }
                else if (Options == CopyOptions.DoNotOverwrite)
                {
                    if (FileExists(filePath))
                    {
                        isCanCopy = false;
                    }
                    //File.CopyTo(Path.Combine(DestinationInfo.FullName, File.Name), false);
                }

                if (isCanCopy)
                {
                    Exception error = null;
                    try
                    {
                        File.CopyTo(filePath, true);
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }

                    if (fileCopyProgressCallback != null)
                    {
                        fileCopyProgressCallback(filePath, error);
                    }
                }
            }
            if (Recursive)
            {
                List<DirectoryInfo> Directories = GetDirectoryList(SourceInfo.FullName);
                foreach (DirectoryInfo Directory in Directories)
                {
                    CopyDirectory(Directory.FullName, Path.Combine(DestinationInfo.FullName, Directory.Name), Recursive, Options);
                }
            }
        }

        /// <summary>
        ///     复制文件到指定的文件路径,默认为不覆盖目标文件
        /// </summary>
        /// <param name="sourceFile">原文件路径</param>
        /// <param name="descFile">目标文件完整路径</param>
        public static void CopyFile(string sourceFile, string descFile)
        {
            CopyFile(sourceFile, descFile, false);
        }

        /// <summary>
        ///     复制文件到指定的文件路径
        /// </summary>
        /// <param name="sourceFile">原文件路径</param>
        /// <param name="descFile">目标文件完整路径</param>
        /// <param name="isOverwrite">当目标文件已存在,是否覆盖</param>
        public static void CopyFile(string sourceFile,string descFile,bool isOverwrite)
        {
            if (sourceFile.IsNullOrEmpty() || !File.Exists(sourceFile))
            {
                throw new FileNotFoundException("查找不到指定名称的文件夹",sourceFile);
            }
            
            var descFolder = Path.GetDirectoryName(descFile);

            if (descFolder != null && !Directory.Exists(descFolder))
            {
                Directory.CreateDirectory(descFolder);
            }

            //File.Delete(descFile);

            File.Copy(sourceFile,descFile,isOverwrite);

        }


        /// <summary>
        ///     预申请磁盘空间
        /// </summary>
        /// <param name="filePath">生成的空白文件</param>
        /// <param name="fileSize">申请的磁盘空间大小</param>
        public static void PreRequestDiskSpace(string filePath,long fileSize)
        {
            if (filePath.IsEmptyOrWhileSpace())
            {
                throw new Exception("指定路径错误");
            }

            if (FileExists(filePath))
            {
                throw new FileLoadException("指定路径的文件已存在");
            }

            if (fileSize<=0)
            {
                throw new ArgumentOutOfRangeException("fileSize");
            }

            try
            {
                using (FileStream fs = File.Create(filePath))
                {
                    long offset = fs.Seek(fileSize - 1, SeekOrigin.Begin);
                    fs.WriteByte(new byte());
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 枚举指定目录下所有文件，包括子文件夹中的文件
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="searchPatten"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetAllFile(string folder,string searchPatten="")
        {
            Stack<string> skDir = new Stack<string>();
            skDir.Push(folder);
            while (skDir.Count > 0)
            {
                folder = skDir.Pop();
                string[] subDirs = Directory.GetDirectories(folder);
                string[] subFiles = Directory.GetFiles(folder,searchPatten);
                if (subDirs.HasData())
                {
                    for (int i = 0; i < subDirs.Length; i++)
                    {
                        //string dirName = Path.GetFileName(subDirs[i]);
                        skDir.Push(subDirs[i]);
                    }
                }

                if (subFiles.HasData())
                {
                    for (int i = 0; i < subFiles.Length; i++)
                    {
                        yield return subFiles[i];
                    }
                }
            }

        }

        public enum CopyOptions
        {
            CopyIfNewer,
            CopyAlways,
            DoNotOverwrite
        }

        public delegate void FileCopyProgress(string filePath,Exception error);
    }


}
