using System;
using System.Collections.Generic;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.ByteDataConverter
{
    public class ByteToDataItem
    {
        public ByteToDataItem()
        {
            ByteBuffer = null;
            ValueBuffer = null;
            ConfigList = new List<ItemsBase>(2);

        }

        public ByteToDataItem(byte[] buf)
            : this()
        {
            ByteBuffer = buf;
        }

        public byte[] ByteBuffer { set; get; }

        public Dictionary<string, object> ValueBuffer { set; get; }

        public List<ItemsBase> ConfigList { get; set; }


        public static byte[] ConverToByte(Dictionary<string,object> buf,ItemsBase[] config,bool isAutoIgionValueError)
        {
            //最基本的检查,检查两个列表是否为空
            if (buf==null || config==null || buf.Count<=0 || config.Length<=0)
            {
                return null;
            }


            lock (buf)
                lock (config)
                {
                    //检查每个config名称在值缓冲区中是否有相应的值
                    for (int i = 0; i < config.Length; i++)
                    {
                        if (!buf.ContainsKey(config[i].Text) && !isAutoIgionValueError)
                        {
                            return null;
                        }
                    }


                    var lst = new List<byte>(255);

                    for (var i = 0; i < config.Length; i++)
                    {
                        object value = null;

                        value = buf.TryGetValue(config[i].Text, null);

                        if (value == null)
                        {
                            if (isAutoIgionValueError)
                            {
                                value = config[i].DefaultValue;
                            }
                            else
                            {
                                throw new ArgumentNullException();
                            }
                        }

                        lst.AddRange(config[i].GetByte(value));
                    }

                    return lst.ToArray();
                }





        }

        public static Dictionary<string, object> ConvertToValue(byte[] buf, int startIndex, ItemsBase[] config, bool isAutoIgionValueError)
        {
            if (buf == null || buf.Length <= 0 || 
                config==null || config.Length<=0
                )
            {
                return null;
            }

            lock (buf)
            lock (config)
            {
                //计算全部转换的话,总共需要多少个字节的数据
                int tempcout = 0;
                for (int i = 0; i < config.Length; i++)
                {
                    tempcout += config[i].ItemLength;
                }

                if (startIndex + tempcout > buf.Length && !isAutoIgionValueError)
                {
                    return null;
                }

                //存放转换后的值
                var temp = new Dictionary<string, object>();

                int tempIndex = startIndex;

                for (var i = 0; i < config.Length; i++)
                {
                    object value = null;

                    try
                    {
                        value = config[i].GetValue(buf, tempIndex);
                    }
                    catch (Exception)
                    {
                            
                    }
                        

                    if (value == null || tempIndex + config[i].ItemLength > buf.Length)
                    {
                        if (!isAutoIgionValueError)
                        {
                            throw new ArgumentNullException();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    temp.Add(config[i].Text, value);

                    tempIndex += config[i].ItemLength;
                }

                return temp;
            }



        }


    }

    public abstract class ItemsBase
    {
        public ItemsBase()
        {
            DefaultValue = 0;
        }

        public ItemsBase(string text):this(text,null){}

        public ItemsBase(string text,ByteConverter _conver):this()
        {
            Text = text;
            Converter = _conver;
        }

        public string Text { set; get; }

        public virtual int ItemLength { private set; get; }

        public  object DefaultValue
        {
            set; get;
        }

        public  ByteConverter Converter { set; get; }

        public abstract byte[] GetByte(object value);

        public abstract object GetValue(byte[] buf,int srtatIdex);
    }

    public class UInt16Item : ItemsBase
    {
        public UInt16Item(string text,ByteConverter _conver):base(text,_conver){}

        public UInt16Item(string text,UInt16Swap swap):base(text)
        {
            SwapType = swap;
        }

        public UInt16Swap SwapType { set; get; }

        public override byte[] GetByte(object value)
        {
            if (base.Converter != null)
            {
                return Converter.GetBytes((ushort)value);
            }

            return ByteConverter.GetBytes(value.ToUInt16(), this.SwapType);
        }

        public override object GetValue(byte[] buf, int startIndex)
        {
            try
            {
                if (base.Converter != null)
                {
                    return Converter.ToFloat(buf, startIndex);
                }

                return ByteConverter.ToUInt16(buf, startIndex, this.SwapType);
            }
            catch (Exception)
            {
                return default(UInt16);
            }
            
        }

        public override int ItemLength
        {
            get
            {
                return 2;
            }
        }

        /// <summary>
        ///     根据输入的名称列表，批量构造指定名称的UInt16Item
        /// </summary>
        /// <param name="convert">转换器类</param>
        /// <param name="strName">名称列表</param>
        /// <returns></returns>
        public static ItemsBase[] BuildList(ByteConverter convert,string[] strName)
        {
            if (strName != null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new UInt16Item(strName[i], convert));
            }

            return lst.ToArray();
        }

        /// <summary>
        ///     根据输入的名称列表，批量构造指定名称的UInt16Item
        /// </summary>
        /// <param name="swap">ushort的转换类型</param>
        /// <param name="strName">名称列表</param>
        /// <returns></returns>
        public static ItemsBase[] BuildList(UInt16Swap swap, string[] strName)
        {
            if (strName != null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new UInt16Item(strName[i], swap));
            }

            return lst.ToArray();
        }

    }

    public class FloatItem : ItemsBase
    {
        public FloatItem(string text, ByteConverter _conver) : base(text, _conver) { }

        public FloatItem(string text, FloatSwap swap)
            : base(text)
        {
            SwapType = swap;
        }

        public FloatSwap SwapType { set; get; }

        public override int ItemLength
        {
            get
            {
                return 4;
            }
        }

        public override byte[] GetByte(object value)
        {
            if (base.Converter!=null)
            {
                return Converter.GetBytes((float)value);
            }

            return ByteConverter.GetBytes(value.ToFloat(), this.SwapType);
        }

        public override object GetValue(byte[] buf, int startIndex)
        {
            try
            {
                if (base.Converter != null)
                {
                    return Converter.ToFloat(buf, startIndex);
                }

                return ByteConverter.ToFloat(buf, startIndex, this.SwapType);
            }
            catch (Exception)
            {
                return default(float);
            }


        }

        public static ItemsBase[] BuildList(ByteConverter convert, string[] strName)
        {
            if (strName == null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new FloatItem(strName[i], convert));
            }

            return lst.ToArray();
        }

        public static ItemsBase[] BuildList(FloatSwap swap, string[] strName)
        {
            if (strName == null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new FloatItem(strName[i], swap));
            }

            return lst.ToArray();
        }
    }

    public class DateTimeItem : ItemsBase
    {
        public DateTimeItem(string text, ByteConverter _conver) : base(text, _conver) { }

        public DateTimeItem(string text, DateTimeType swap)
            : this(text,swap,UInt16Swap.HightBefore)
        {
            

        }

        public DateTimeItem(string text, DateTimeType swap, UInt16Swap intswap):base(text)
        {
            SwapType = swap;
            UInt16SwapType = intswap;
            base.DefaultValue = DateTime.MinValue;
        }


        public DateTimeType SwapType { set; get; }

        public UInt16Swap UInt16SwapType { set; get; }

        public override int ItemLength
        {
            get
            {
                switch (SwapType)
                {
                    case DateTimeType.Long:
                        return 8;
                    default:
                        return 6;

                }
            }

            
        }

        public override byte[] GetByte(object value)
        {
            byte[] ret = null;

            if (base.Converter != null)
            {
                ret= Converter.GetBytes((DateTime)value);
            }
            else
            {
                ret = ByteConverter.GetBytes(value.ToDateTime(), this.UInt16SwapType, this.SwapType);
            }
            

            return ret;
        }

        public override object GetValue(byte[] buf, int startIndex)
        {
            try
            {

                if (base.Converter != null)
                {
                    return Converter.ToFloat(buf, startIndex);
                }

                return ByteConverter.ToDateTime(buf, startIndex, this.UInt16SwapType, this.SwapType);
            }
            catch (Exception)
            {
                return default(DateTime);
            }


        }

        public static ItemsBase[] BuildList(ByteConverter convert, string[] strName)
        {
            if (strName == null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new DateTimeItem(strName[i], convert));
            }

            return lst.ToArray();
        }

        public static ItemsBase[] BuildList(UInt16Swap swap, DateTimeType type,string[] strName)
        {
            if (strName == null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new DateTimeItem(strName[i],type, swap));
            }

            return lst.ToArray();
        }
    }


    public class DoubleItem : ItemsBase
    {
        public DoubleItem(string text, ByteConverter _conver) : base(text, _conver) { }

        public DoubleItem(string text, DoubleSwap swap)
            : base(text)
        {
            SwapType = swap;
        }

        public DoubleSwap SwapType { set; get; }

        public override int ItemLength
        {
            get
            {
                return 8;
            }
        }

        public override byte[] GetByte(object value)
        {
            if (base.Converter != null)
            {
                return Converter.GetBytes((float)value);
            }

            return ByteConverter.GetBytes(value.ToDouble(), this.SwapType);
        }

        public override object GetValue(byte[] buf, int startIndex)
        {
            try
            {
                if (base.Converter != null)
                {
                    return Converter.ToDouble(buf, startIndex);
                }

                return ByteConverter.ToDouble(buf, startIndex, this.SwapType);
            }
            catch (Exception)
            {
                return default(float);
            }


        }

        public static ItemsBase[] BuildList(ByteConverter convert, string[] strName)
        {
            if (strName== null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new DoubleItem(strName[i], convert));
            }

            return lst.ToArray();
        }

        public static ItemsBase[] BuildList(DoubleSwap swap, string[] strName)
        {
            if (strName == null || strName.Length <= 0)
            {
                return null;
            }

            var lst = new List<ItemsBase>(strName.Length);

            for (int i = 0; i < strName.Length; i++)
            {
                lst.Add(new DoubleItem(strName[i], swap));
            }

            return lst.ToArray();
        }
    }
}