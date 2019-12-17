using System;
using System.Collections.Generic;

#if NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0
using System.Drawing;
#endif
#if NET45
   using System.Drawing;
#endif


using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Kugar.Core.ExtMethod;
using Kugar.Core.ExtMethod.Serialization;


namespace Kugar.Core.ExtMethod
{
    public static class FontExt
    {
        private static  XmlDocument _shareDocument=new XmlDocument();

        public static void SerializeToXML(this Font font, XmlNode node)
        {
            if (font==null)
            {
                return;
            }
            node.SetAttribute("Name", font.Name);
            node.SetAttribute("Size", font.Size.ToString(CultureInfo.InvariantCulture));
            node.SetAttribute("Bold", font.Bold.ToString());
            node.SetAttribute("Italic", font.Italic.ToString());
            bool strikeout = false;
#if NET45 || NETCOREAPP2_0
            strikeout = font.Strikeout;
#endif
#if NETSTANDARD2_0
            strikeout = font.Strikethrough;
#endif

            node.SetAttribute("Strikeout", strikeout.ToString());
            node.SetAttribute("Underline", font.Underline.ToString());

        }

        public static string SerializeToString(this Font font)
        {
            if (font==null)
            {
                return string.Empty;
            }

            var node = _shareDocument.CreateElement("Font");

            SerializeToXML(font, node);

            return node.OuterXml.ToBase64();

            //return SerializationExt.SerializeToString(node);
        }

        public static Font DeserialFontFromXML(XmlElement node)
        {
            if (node==null || node.FirstChild==null)
            {
                return null;
            }

            node=(XmlElement)node.FirstChild;

            string familyName = node.GetAttribute("Name");

            if (string.IsNullOrWhiteSpace(familyName))
            {
                return null;
            }

            float size = float.Parse(node.GetAttribute("Size"), CultureInfo.InvariantCulture);

            bool bold = bool.Parse(node.GetAttribute("Bold"));

            bool italic = bool.Parse(node.GetAttribute("Italic"));

            bool strikeout = bool.Parse(node.GetAttribute("Strikeout"));

            bool underline = bool.Parse(node.GetAttribute("Underline"));

            

            FontStyle fontStyle = FontStyle.Regular;

            if (bold) fontStyle |= FontStyle.Bold;

            if (italic) fontStyle |= FontStyle.Italic;

            if (strikeout) fontStyle |= FontStyle.Strikeout;

            if (underline) fontStyle |= FontStyle.Underline;

            Font font = new Font(familyName, size, fontStyle);

            return font;
        }

        public static Font DeserialFontFromString(string fontString)
        {
            if (string.IsNullOrWhiteSpace(fontString))
            {
                return null;
            }

            var xmlStr = fontString.FromBase64();

            var node = _shareDocument.CreateElement("Font");

            node.InnerXml = xmlStr;

            return DeserialFontFromXML(node);
        }
    }
}
