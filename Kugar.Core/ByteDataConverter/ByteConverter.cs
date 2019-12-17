using System;
using System.Collections.Generic;

namespace Kugar.Core.ByteDataConverter
{
    /// <summary>
    ///     带高低位转换的Byte转换类
    /// </summary>
    public class ByteConverter
    {
        public  ByteConverter()
        {
            DoubleSwapType = DoubleSwap.CNormal;
            FloatSwapType = FloatSwap.CNormal;
            DateTimeSwapType = DateTimeType.Long;
            UInt16SwapType = UInt16Swap.HightBefore;
            Int32SwapType = Int32Swap.C1032;
        }

        public FloatSwap FloatSwapType { set; get; }

        public UInt16Swap UInt16SwapType { set; get; }

        public DateTimeType DateTimeSwapType { set; get; }

        public DoubleSwap DoubleSwapType { set; get; }

        public Int32Swap Int32SwapType { set; get; }


        public static byte[] GetBytes(float value, FloatSwap IsSwap)
        {
            var t = BitConverter.GetBytes(value);
            byte temp;

            switch (IsSwap)
            {
                case FloatSwap.CNormal:
     
                    temp = t[0];    //1,2交换
                    t[0] = t[1];
                    t[1] = temp;

                    temp = t[2];    //3,4交换
                    t[2] = t[3];
                    t[3] = temp;
                    break;

                case FloatSwap.CSwap:
                    Array.Reverse(t);
                    break;
            }

            return t;
        }

        public byte[] GetBytes(float value)
        {
            return GetBytes(value, this.FloatSwapType);
        }


        public static byte[] GetBytes(double value, DoubleSwap IsSwap)
        {
            var t = BitConverter.GetBytes(value);

            switch (IsSwap)
            {
                case DoubleSwap.C76543210:
                    Array.Reverse(t);
                    break;
            }

            return t;
        }

        public byte[] GetBytes(double value)
        {
            return GetBytes(value, this.DoubleSwapType);
        }


        public static byte[] GetBytes(UInt16 value, UInt16Swap swap)
        {
            byte[] r = null;
            switch (swap)
            {
                case UInt16Swap.LowBeforce:
                    r = BitConverter.GetBytes(value);
                    break;
                case UInt16Swap.HightBefore:
                    r = new byte[2];
                    r[0] = (byte)(value / 256);
                    r[1] = (byte)(value % 256);
                    break;
            }

            return r;
        }

        public byte[] GetBytes(UInt16 value)
        {
            return GetBytes(value, this.UInt16SwapType);
        }


        public static byte[] GetBytes(UInt32 value)
        {
            var t = BitConverter.GetBytes(value);

            var t1 = new byte[4];

            t1[0] = t[3];
            t1[1] = t[2];
            t1[2] = t[1];
            t1[3] = t[0];

            return t1;
        }

        
        public static byte[] GetBytes(Int32 value, Int32Swap longSwap)
        {
            byte[] t = null;

            t = BitConverter.GetBytes(value);

            switch (longSwap)
            {
                case Int32Swap.C3210:
                    //本身转换过后的值就是低位在前
                    break;
                case Int32Swap.C0123:
                    var t1 = new byte[4];

                    t1[0] = t[3];
                    t1[1] = t[2];
                    t1[2] = t[1];
                    t1[3] = t[0];

                    t = t1;
                    break;
            }

            return t;
        }

        public static byte[] GetBytes(Int32 value)
        {
            return GetBytes(value, Int32Swap.C0123);

            //var t = BitConverter.GetBytes(value);

            //var t1 = new byte[4];

            //t1[0] = t[3];
            //t1[1] = t[2];
            //t1[2] = t[1];
            //t1[3] = t[0];

            //return t1;
        }


        public static byte[] GetBytes(DateTime dt, UInt16Swap uInt16Swap, DateTimeType dateTimeType)
        {
            ushort year, moth, day, hour, min, sec;

            year = (ushort)dt.Year;
            moth = (ushort)dt.Month;
            day = (ushort)dt.Day;
            hour = (ushort)dt.Hour;
            min = (ushort)dt.Minute;
            sec = (ushort)dt.Second;


            switch (dateTimeType)
            {
                case DateTimeType.Short:
                    year = (ushort)(year - 2000);
                    break;
            }

            var b = new List<byte>();

            b.AddRange(GetBytes(year, uInt16Swap));
            b.AddRange(GetBytes(moth, uInt16Swap));
            b.AddRange(GetBytes(day, uInt16Swap));
            b.AddRange(GetBytes(hour, uInt16Swap));
            b.AddRange(GetBytes(min, uInt16Swap));
            b.AddRange(GetBytes(sec, uInt16Swap));

            return b.ToArray();

        }

        public byte[] GetBytes(DateTime dt)
        {
            return GetBytes(dt, this.UInt16SwapType, this.DateTimeSwapType);
        }


        public static float ToFloat(byte[] data, int start, FloatSwap isSwap)
        {
            if (start < 0)
                return 0;

            if (data.Length - start < 4)
                return 0;

            byte[] temp = new byte[4];

            switch (isSwap)
            {
                case FloatSwap.CNormal:

                    temp[0] = data[start + 1];
                    temp[1] = data[start + 0];

                    temp[2] = data[start + 3];
                    temp[3] = data[start + 2];
                    break;

                case FloatSwap.C3210:
                    //for (int i = 0; i < 4; i++)
                    //{
                    //    temp[i] = data[start + i];
                    //}

                    temp[0] = data[start + 3];
                    temp[1] = data[start + 2];
                    temp[2] = data[start + 1];
                    temp[3] = data[start + 0];

                    break;

                case FloatSwap.CSwap:
                    
                    //temp[0] = data[start + 3];
                    //temp[1] = data[start + 2];
                    //temp[2] = data[start + 1];
                    //temp[3] = data[start + 0];

                    for (int i = 0; i < 4; i++)
                    {
                        temp[i] = data[start + i];
                    }

                    Array.Reverse(temp);

                    break;
            }

            //if(isSwap)
            //{

            //}
            //else
            //{
            //    for(int i=0;i<4;i++)
            //    {
            //        temp[i]=data[start + i];
            //    }
            //}

            try
            {
                return BitConverter.ToSingle(temp,
                                             0);
            }
            catch (Exception)
            {

                return 0;
            }


        }

        public float ToFloat(byte[] data, int start)
        {
            return ToFloat(data,
                           start,
                           this.FloatSwapType);
        }



        public static UInt16 ToUInt16(byte[] data, int start, UInt16Swap swap)
        {
            ushort r = 0;

            switch (swap)
            {
                case UInt16Swap.LowBeforce:
                    r = BitConverter.ToUInt16(data, start);
                    break;
                case UInt16Swap.HightBefore:
                    r = (ushort)(data[start] * 256 + data[start + 1]);
                    break;
            }

            return r;
        }

        public UInt16 ToUInt16(byte[] data, int start)
        {
            return ToUInt16(data, start, this.UInt16SwapType);
        }



        public static UInt32 ToUInt32(byte[] data, int start)
        {
            var t1 = new byte[4];

            t1[0] = data[3 + start];
            t1[1] = data[2 + start];
            t1[2] = data[1 + start];
            t1[3] = data[0 + start];

            return BitConverter.ToUInt32(t1, 0);

        }



        public static Int32 ToInt32(byte[] data, int start, Int32Swap swap)
        {
            var t1 = new byte[4];

            switch (swap)
            {
                case Int32Swap.C0123:
                    t1[0] = data[3 + start];
                    t1[1] = data[2 + start];
                    t1[2] = data[1 + start];
                    t1[3] = data[0 + start];
                    break;
                case Int32Swap.C3210:
                    t1[0] = data[0 + start];
                    t1[1] = data[1 + start];
                    t1[2] = data[2 + start];
                    t1[3] = data[3 + start];
                    break;
                case Int32Swap.C1032:
                    t1[0] = data[1 + start];
                    t1[1] = data[0 + start];
                    t1[2] = data[3 + start];
                    t1[3] = data[2 + start];
                    break;
            }



            return BitConverter.ToInt32(t1, 0);
        }

        public Int32 ToInt32(byte[] data, int start)
        {
            //var t1 = new byte[4];

            //t1[0] = data[3 + start];
            //t1[1] = data[2 + start];
            //t1[2] = data[1 + start];
            //t1[3] = data[0 + start];

            //return BitConverter.ToInt32(t1, 0);

            return ByteConverter.ToInt32(data, start, this.Int32SwapType);
        }


        public static DateTime ToDateTime(byte[] data, int start, UInt16Swap int16Swap, DateTimeType dateTimeType)
        {
            ushort year, moth, day, hour, min, sec;

            year = ToUInt16(data, start, int16Swap);
            moth = ToUInt16(data, start + 2, int16Swap);
            day = ToUInt16(data, start + 4, int16Swap);
            hour = ToUInt16(data, start + 6, int16Swap);
            min = ToUInt16(data, start + 8, int16Swap);
            sec = ToUInt16(data, start + 10, int16Swap);

            if (year <= 99)
            {
                year += 2000;
            }

            try
            {
                return new DateTime(year, moth, day, hour, min, sec);
            }
            catch (Exception)
            {
                return DateTime.MinValue;
            }

        }

        public DateTime ToDateTime(byte[] data, int start)
        {
            return ToDateTime(data, start, this.UInt16SwapType, this.DateTimeSwapType);
        }


        public static double ToDouble(byte[] data, int start, DoubleSwap swap)
        {
            var temp = new byte[8];

            for (int i = 0; i < 8; i++)
            {
                temp[i] = data[start + i];
            }

            switch (swap)
            {
                case DoubleSwap.C76543210:
                    Array.Reverse(temp);
                    break;
                case DoubleSwap.CNormal:
                    temp[0] = data[start + 1];
                    temp[1] = data[start + 0];
                    temp[2] = data[start + 3];
                    temp[3] = data[start + 2];

                    temp[4] = data[start + 5];
                    temp[5] = data[start + 4];
                    temp[6] = data[start + 7];
                    temp[7] = data[start + 6];

                    break;
            }

            return BitConverter.ToDouble(temp, 0);
        }

        public double ToDouble(byte[] data, int start)
        {
            return ToDouble(data, start, this.DoubleSwapType);
        }


        //public static DateTime ToDateTimeShort(byte[] data, int start, UInt16Swap swap)
        //{
        //    int year, month, day, hour, min, sec;
        //    DateTime temp = DateTime.MinValue;

        //    try
        //    {
        //        year = BitChange.ToUInt16(data, start, swap) + 2000;
        //        month = BitChange.ToUInt16(data, start + 2, swap);
        //        day = BitChange.ToUInt16(data, start + 4, swap);
        //        hour = BitChange.ToUInt16(data, start + 6, swap);
        //        min = BitChange.ToUInt16(data, start + 8, swap);
        //        sec = BitChange.ToUInt16(data, start + 10, swap);

        //        temp = new DateTime(year, month, day, hour, min, sec);
        //    }
        //    catch (Exception)
        //    {
        //    }

        //    return temp;
        //}

        //public static DateTime ToDateTimeShort(byte[] data, int start)
        //{
        //    return ToDateTimeShort(data, start, UInt16Swap.LowBeforce);

        //}

        //public static DateTime ToDateTimeLong(byte[] data, int start)
        //{
        //    int year, month, day, hour, min, sec;
        //    DateTime temp = DateTime.MinValue;

        //    try
        //    {
        //        year = BitChange.ToUInt16(data, start.);
        //        month = BitChange.ToUInt16(data, start + 2);
        //        day = BitChange.ToUInt16(data, start + 4);
        //        hour = BitChange.ToUInt16(data, start + 6);
        //        min = BitChange.ToUInt16(data, start + 8);
        //        sec = BitChange.ToUInt16(data, start + 10);

        //        temp = new DateTime(year, month, day, hour, min, sec);
        //    }
        //    catch (Exception)
        //    {
        //    }

        //    return temp;

        //}







    }
}