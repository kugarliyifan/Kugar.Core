using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Kugar.Core.ExtMethod
{
    public static class DataEncrypt
    {
        public static string JWTEncrypt(this string sourceStr, string key)
        {
            return "";
        }

        /// <summary>
        ///     字符串des加密,以Base64格式返回的加密字符串
        /// </summary>
        /// <param name="pToEncrypt">要加密的字符串</param>
        /// <param name="sKey">密钥，且必须为8位</param>
        /// <returns></returns>
        public static string DesEncrypt(this string pToEncrypt, string sKey)
        {
            if (sKey.Length > 8)
            {
                sKey = sKey.Substring(0, 8);
            }
            else
            {
                sKey = sKey.PadRight(8);
            }

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                byte[] inputByteArray = Encoding.UTF8.GetBytes(pToEncrypt);
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                string str = Convert.ToBase64String(ms.ToArray());
                ms.Close();
                return str;
            }
        }

        /// <summary>
        ///     字符串des解密,配合DesEncrypt函数使用,以Base64格式返回的解密字符串
        /// </summary>
        /// <param name="pToDecrypt">要解密的字符串</param>
        /// <param name="sKey">密钥，且必须为8位</param>
        /// <returns></returns>
        public static string DesDecrypt(this string pToDecrypt, string sKey)
        {
            byte[] inputByteArray = null;

            try
            {
                inputByteArray = Convert.FromBase64String(pToDecrypt);
            }
            catch (Exception)
            {
                inputByteArray = Encoding.UTF8.GetBytes(pToDecrypt);
            }

             

            if (sKey.Length > 8)
            {
                sKey = sKey.Substring(0, 8);
            }
            else
            {
                sKey = sKey.PadRight(8);
            }

            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
                des.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

                using (var ms = new System.IO.MemoryStream())                
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    ms.Position = 0;

                    string str = Encoding.UTF8.GetString(ms.ToArray());

                    return str;

                    //cs.Close();
                }

                
                //ms.Close();
                //return str;
            }
        }

        /// <summary>
        ///     计算指定字符的MD5码（32位）
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="isForce32Length">是否强制补满32个字符,如果为false,则不强制补齐32个字符,如果为true,则强制补齐32个长度的字符</param>
        /// <returns>返回计算后的MD5码字符串</returns>
        public static string MD5_32(this string str,bool isForce32Length=false)
        {
            var b = MD5_32Array(str);

            if (b == null || b.Length <= 0)
            {
                return "";
            }

            if (isForce32Length)
            {
                var s = new StringBuilder(32);

                for (int i = 0; i < b.Length; i++)
                {
                    s.AppendFormat(b[i].ToString("X2"));
                }

                return s.ToString();
            }
            else
            {
                return b.JoinToString();
            }
            
        }

        public static byte[] MD5_32Array(this string str)
        {
            byte[] s = null;

            using (MD5 m = new MD5CryptoServiceProvider())
            {
                s = m.ComputeHash(UnicodeEncoding.UTF8.GetBytes(str));
            }

            return s;
        }

        public static string MD5_16(this string str)
        {
            return MD5_32(str).Left(16);

            var b = MD5_32Array(str);

            if (b == null || b.Length <= 0)
            {
                return "";
            }

            return b.Take(16).JoinToString();
   
            return BitConverter.ToString(b).Substring(0, 16);
        }

        public static ushort GetCRCCode(this byte[] data)
        {
            return GetCRCCode(data,
                              0,
                              data.Length);

        }

        /// <summary>
        ///     计算CRC16校验码
        /// </summary>
        /// <param name="data">源数据区</param>
        /// <param name="start">其实偏移量</param>
        /// <param name="len">计算的长度</param>
        /// <returns></returns>
        public static ushort GetCRCCode(this byte[] data, int start, int len)
        {
            if (data == null || data.Length <= 0 || len > data.Length)
            {
                return 0;
            }

            ushort wCRC16;
            wCRC16 = 0xFFFF;

            for (int i = start; i <start+ len; i++)
            {
                wCRC16 = (ushort)(((wCRC16 >> 8) & 0xFF) ^ CRC16Table[(wCRC16 ^ data[i]) & 0xFF]);
            }

            return wCRC16;
        }

        public static byte[] GetCRCCodeBytes(this byte[] data)
        {
            return GetCRCCodeBytes(data, 0, data.Length);
        }

        public static byte[] GetCRCCodeBytes(this byte[] data,int start,int len)
        {
            var crcCode = GetCRCCode(data, start, len);

            return BitConverter.GetBytes(crcCode);
        }

        public static string SHA256(this string str)
        {
            byte[] SHA256Data = Encoding.Default.GetBytes(str);
            using (SHA256Managed Sha256 = new SHA256Managed())
            {
                byte[] Result = Sha256.ComputeHash(SHA256Data);
                return Convert.ToBase64String(Result); //返回长度为44字节的字符串
            }
        }

        /// <summary>
        ///     计算SHA1的加密值
        /// </summary>
        /// <param name="Input">输入的字符串</param>
        /// <returns></returns>
        public static string SHA1(this string Input)
        {
            if (string.IsNullOrEmpty(Input))
                throw new ArgumentNullException("Input");
            SHA1CryptoServiceProvider SHA1 = new SHA1CryptoServiceProvider();
            byte[] InputArray = System.Text.Encoding.ASCII.GetBytes(Input);
            byte[] HashedArray = SHA1.ComputeHash(InputArray);
            SHA1.Clear();
            return BitConverter.ToString(HashedArray).Replace("-", "");
        }

        #region "CRC16Table"
        private static readonly ushort[] CRC16Table = new ushort[]{   0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280,
		                                                            0xC241, 0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1,
		                                                            0xC481, 0x0440, 0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00,
		                                                            0xCFC1, 0xCE81, 0x0E40, 0x0A00, 0xCAC1, 0xCB81, 0x0B40,
		                                                            0xC901, 0x09C0, 0x0880, 0xC841, 0xD801, 0x18C0, 0x1980,
		                                                            0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40, 0x1E00, 0xDEC1,
		                                                            0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41, 0x1400,
		                                                            0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
		                                                            0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081,
		                                                            0x1040, 0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1,
		                                                            0xF281, 0x3240, 0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501,
		                                                            0x35C0, 0x3480, 0xF441, 0x3C00, 0xFCC1, 0xFD81, 0x3D40,
		                                                            0xFF01, 0x3FC0, 0x3E80, 0xFE41, 0xFA01, 0x3AC0, 0x3B80,
		                                                            0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840, 0x2800, 0xE8C1,
		                                                            0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41, 0xEE01,
		                                                            0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
		                                                            0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681,
		                                                            0x2640, 0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0,
		                                                            0x2080, 0xE041, 0xA001, 0x60C0, 0x6180, 0xA141, 0x6300,
		                                                            0xA3C1, 0xA281, 0x6240, 0x6600, 0xA6C1, 0xA781, 0x6740,
		                                                            0xA501, 0x65C0, 0x6480, 0xA441, 0x6C00, 0xACC1, 0xAD81,
		                                                            0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41, 0xAA01, 0x6AC0,
		                                                            0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840, 0x7800,
		                                                            0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
		                                                            0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81,
		                                                            0x7C40, 0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1,
		                                                            0xB681, 0x7640, 0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101,
		                                                            0x71C0, 0x7080, 0xB041, 0x5000, 0x90C1, 0x9181, 0x5140,
		                                                            0x9301, 0x53C0, 0x5280, 0x9241, 0x9601, 0x56C0, 0x5780,
		                                                            0x9741, 0x5500, 0x95C1, 0x9481, 0x5440, 0x9C01, 0x5CC0,
		                                                            0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40, 0x5A00,
		                                                            0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
		                                                            0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81,
		                                                            0x4A40, 0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0,
		                                                            0x4C80, 0x8C41, 0x4400, 0x84C1, 0x8581, 0x4540, 0x8701,
		                                                            0x47C0, 0x4680, 0x8641, 0x8201, 0x42C0, 0x4380, 0x8341,
		                                                            0x4100, 0x81C1, 0x8081, 0x4040 };

        #endregion
    }
}
