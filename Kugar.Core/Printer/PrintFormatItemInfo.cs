using System;
using System.IO;

namespace Kugar.Core.Printer
{
    public class PrintFormatItemInfo:ICloneable
    {
        private string _printFormat;
        private bool _modify = false;
        private bool _isCreated = false;
        private object _lockObj = new object();
        private bool _isDefault = false;

        public string ModuleID { set; get; }

        public string Name { set; get; }

        public string PrintFormat
        {
            set
            {
                if (_printFormat != value)
                {
                    _isCreated = true;
                    _modify = true;
                    _printFormat = value;
                }
            }
            get
            {
                if (!_isCreated)
                {
                    lock (_lockObj)
                    {
                        if (!_isCreated)
                        {
                            _printFormat = onLoadingFormat();
                        }
                    }
                }

                return _printFormat;
            }
        }

        public bool IsDefault
        {
            get { return _isDefault; }
            set { _isDefault = value; }
        }

        public object Clone()
        {
            var temp = new PrintFormatItemInfo();

            temp.ModuleID = this.ModuleID;
            temp.Name = this.Name;
            temp.PrintFormat =string.IsNullOrWhiteSpace(this._printFormat)?"":string.Copy(this.PrintFormat);

            return temp;
        }

        protected string onLoadingFormat()
        {
            if (LoadingFormat != null)
            {
                var e = new PrintFormatLoadEventArgs(ModuleID, Name);

                LoadingFormat(this, e);

                if (e.HasError)
                {
                    throw e.Error;
                }
                else
                {
                    return e.Format;
                }
            }
            else
            {
                throw new Exception("必须订阅LoadingFormat事件返回打印格式数据");
            }
        }

        public event EventHandler<PrintFormatLoadEventArgs> LoadingFormat;



    }

    public class PrintFormatLoadEventArgs : EventArgs
    {
        public PrintFormatLoadEventArgs(string moduleId, string formatName)
        {
            ModuleID = moduleId;
            FormatName = formatName;
        }

        public string FormatName { get; private set; }
        public Exception Error { set; get; }
        public bool HasError { get { return Error != null; } }
        public string ModuleID { get; private set; }
        public string Format { set; get; }
    }

    public class PrintFormatDefaultChanged : EventArgs
    {
        public PrintFormatDefaultChanged(PrintFormatItemInfo item)
        {
            Item = item;
        }

        public PrintFormatItemInfo Item { get; private set; }
    }
}