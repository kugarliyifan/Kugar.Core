using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kugar.Core.BaseStruct
{
    public class ListItemObject
    {
        public ListItemObject(string text, object value)
        {
            _text = text;
            Value = value;
        }

        private string _text = "";

        public string Text
        {
            set
            {
                if (string.IsNullOrEmpty(_text))
                {
                    _text = "";
                }
                else
                {
                    _text = value;
                }
            }
            get { return _text; }
        }

        public object Value { set; get; }

        public override string ToString()
        {
            return Text;
        }
    }


    public class ListItemObject<T>
    {
        public ListItemObject(string text, T value)
        {
            _text = text;
            Value = value;
        }

        private string _text = "";

        public string Text
        {
            set
            {
                if (string.IsNullOrEmpty(_text))
                {
                    _text = "";
                }
                else
                {
                    _text = value;
                }
            }
            get { return _text; }
        }

        public T Value { set; get; }

        public override string ToString()
        {
            return Text;
        }
    }
}
