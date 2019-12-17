using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

#if !NETCOREAPP
      using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
#endif


namespace Kugar.Core.ExtMethod
{
    public static class XmlExtMethod
    {
    	/// <summary>
    	/// 	保存XML文档到字符串
    	/// </summary>
    	/// <param name="document"></param>
    	/// <returns></returns>
    	public static string SaveToString(this XmlDocument document)
    	{
    		if (document==null || !document.HasChildNodes) {
    			return string.Empty;
    		}
    		
    		var sb=new StringBuilder();
    	    string s = null;
            using (var strWrite = new System.IO.StringWriter(sb))
    	    {
                document.Save(strWrite);
                 s = sb.ToStringEx();
                 sb.Clear();
    	    }

    		return s;
    		
    	}
    	
    	
        public static XmlNode GetFirstElementsByTagName(this XmlDocument document,string name)
        {
            var el = document.GetElementsByTagName(name);

            if (el==null || el.Count<=0)
            {
                return null;
            }
            else
            {
                return el[0];
            }
        }

        public static XmlNode GetFirstElementsByTagName(this XmlNode parentNode, string name)
        {
            var nodeList = parentNode.ChildNodes;

            foreach (XmlNode n in nodeList)
            {
                if (n.Name==name)
                {
                    return n;
                }
            }

            return null;
        }

        public static XmlNode[] GetElementsByTagName(this XmlNode parentNode, string name,bool isContainChild=false)
        {
            //var nodeList = parentNode.ChildNodes;

            IEnumerable<XmlNode> nodes = null;

            if (isContainChild)
            {
                nodes = parentNode.EnumChildNodes();
            }
            else
            {
                nodes=parentNode.ChildNodes.AsEnumerable<XmlNode>();
            }

            return nodes.Where(x => x.Name == name).ToArray();
            

            //var list = new List<XmlNode>(16);

            //foreach (XmlNode n in nodeList)
            //{
            //    if (n.Name == name)
            //    {
            //        list.Add(n);
            //    }
            //}


            //return list.ToArray();
        }

        public static XmlElement CreateElement(this XmlDocument document,string name,string value)
        {
            return CreateElement(document, null, name, value);
        }

        public static XmlElement CreateElement(this XmlDocument document,XmlElement parent,string name,string value)
        {
            var tempElement = document.CreateElement(name);

            tempElement.InnerText = value;

            if (parent!=null)
            {
                parent.AppendChild(tempElement);
            }

            return tempElement;
        }

        public static string GetChildNodeInnerText(this XmlNode parentNode, string nodeName)
        {
            return GetChildNodeInnerText(parentNode, nodeName, string.Empty);
        }

        public static string GetChildNodeInnerText(this XmlNode parentNode, string nodeName, string defaultValue)
        {
            if (parentNode == null || string.IsNullOrEmpty(nodeName))
            {
                return defaultValue;
            }

            var childItem = parentNode[nodeName];

            if (childItem != null)
            {
                return childItem.InnerText;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string GetChildNodeValue(this XmlNode parentNode,string nodeName)
        {
            return GetChildNodeValue(parentNode, nodeName, string.Empty);
        }

        public static string GetChildNodeValue(this XmlNode parentNode,string nodeName,string defaultValue)
        {
            if (parentNode==null ||  string.IsNullOrEmpty(nodeName))
            {
                return defaultValue;
            }

            var childItem = parentNode[nodeName];

            if (childItem!=null)
            {
            	if (childItem is XmlElement) {
            		return childItem.GetAttribute("value");
            	}
            	else{
            		return childItem.Value;
            	}
            }
            else
            {
                return defaultValue;
            }
        }

        public static string GetChildNodeAttributeValue(this XmlNode parentNode, string nodeName, string attributeName, string defaultValue = "")
        {
            if (parentNode == null || string.IsNullOrEmpty(nodeName))
            {
                return defaultValue;
            }

            var childItem = parentNode[nodeName];

            if (childItem != null)
            {
                return childItem.GetAttribute(attributeName);
            }
            else
            {
                return defaultValue;
            } 
        }

        public static XmlNode GetFirstNodeByTagName(this XmlNode parentNode, string tagName,bool isAutoCreate=false,
            bool isContainsChild = false)
        {
            XmlNode node=GetFirstChild(parentNode, x => x.Name.CompareTo(tagName, true), isContainsChild);

            if (node==null && isAutoCreate)
            {
                node = parentNode.AppendChild(tagName);
            }

            return node;
        }

        public static XmlNode GetFirstChild(this XmlNode parentNode, Predicate<XmlNode> checker,bool isContainsChild=false)
        {
            return GetChildNodes(parentNode, checker, isContainsChild).FirstOrDefault();
        }

        public static string GetStringByName(this XmlAttributeCollection collection,string name,string defaultValue="")
        {
            return collection[name].Value.ToStringEx(defaultValue);
        }

        public static int GetIntByName(this XmlAttributeCollection collection, string name, int defaultValue = 0)
        {
            return collection[name].Value.ToStringEx("0").ToInt();
        }

        public static XmlNode AppendChild(this XmlNode parentNode, string nodeName, string value="")
        {
            if (parentNode==null)
            {
                throw new ArgumentNullException("parentNode");
            }

            if (string.IsNullOrWhiteSpace(nodeName))
            {
                throw new ArgumentNullException("nodeName");
            }

            XmlDocument document=null;
            
            if (parentNode is XmlDocument) {
            	document=(XmlDocument)parentNode;
            }
            else{
            	document=parentNode.OwnerDocument;
            }
            
            var node=document.CreateElement(nodeName);
            
            if (!string.IsNullOrWhiteSpace(value)) {
            	node.SetAttribute("value",value);
            }
            
        	parentNode.AppendChild(node);
        	
        	
        	return node;
            
            

            
        }

        public static XmlElement AppendChild(this XmlElement parentNode, string nodeName, string value="")
        {
            if (parentNode == null)
            {
                throw new ArgumentNullException("parentNode");
            }

            if (string.IsNullOrWhiteSpace(nodeName))
            {
                throw new ArgumentNullException("nodeName");
            }
            
            var node = parentNode.OwnerDocument.CreateElement(nodeName);
            
            //node.Value = value;
			
            if (!string.IsNullOrWhiteSpace(value)) {
            	node.SetAttribute("value", value);
            }
            
            parentNode.AppendChild(node);

            return node;
        }
        
        public static XmlNode SetAttribute(this XmlNode node, string name, string value)
        {
            if (node==null)
            {
                throw new ArgumentNullException("node");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!(node is XmlElement)) {
            	throw new ArrayTypeMismatchException("node");
            }
            
            ((XmlElement)node).SetAttribute(name, value);

            return node;
        }

        public static string GetAttribute(this XmlNode node, string name)
        {
            if (node==null)
            {
                throw new ArgumentNullException("node");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!(node is XmlElement)) {
            	throw new ArrayTypeMismatchException("node");
            }
            
            return ((XmlElement)node).GetAttribute(name);

            //return node;
        }

        public static bool ExistAttribute(this XmlNode node, string name)
        {
            if (node==null)
            {
                throw new ArgumentNullException("node");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("name");
            }

            if (!(node is XmlElement)) {
            	throw new ArrayTypeMismatchException("node");
            }

            return ((XmlElement) node).HasAttribute(name);
        }
        
        public static bool SafeLoadXML(this XmlDocument document,string xmlText)
        {
        	try {
        		document.LoadXml(xmlText);
        		
        		return true;
        	} catch (Exception) {
        		return false;
        	}
        }
        
        public static bool SafeLoad(this XmlDocument document,string xmlFilePath)
        {
        	if (System.IO.File.Exists(xmlFilePath)) {
        		try {
        			document.Load(xmlFilePath);
        			return true;
        		} catch (Exception) {
        			return false;
        		}
        	}
        	else{
        		return false;
        	}
        }
        
        public static bool SafeLoad(this XmlDocument document,System.IO.Stream stream)
        {
        	if (stream==null) {
        		return false;
        	}
        	
        	if (stream.CanSeek && stream.Length<=0) {
        		return false;
        	}
        	
        	if (!stream.CanRead) {
        		return false;
        	}
        	
        	try {
        		document.Load(stream);
        		return true;
        	} catch (Exception) {
        		return false;
        	}
        }
        
        public static bool SafeSave(this XmlDocument document,string filePath)
        {
            if (filePath.Contains('/'))
            {
                var dir = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(dir))
                {
                    try
                    {
                        Directory.CreateDirectory(dir);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            


        	
        	try {
        		document.Save(filePath);
        		return true;
        	} catch (Exception) {
        		return false;
        	}
        }

        public static IEnumerable<XmlNode> EnumChildNodes(this XmlNode parentNode)
        {
            if (parentNode != null)
            {
                yield return parentNode;

                if (parentNode.HasChildNodes)
                {
                    foreach (var node in parentNode.ChildNodes)
                    {
                        if (node is XmlNode || node is XmlElement)
                        {
                            foreach (var ctrl in EnumChildNodes((XmlNode)node))
                            {
                                yield return ctrl;
                            }
                        }
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        public static IEnumerable<XmlNode> GetChildNodes(this XmlNode node, Predicate<XmlNode> nodeChecker, bool isContainsChild = false)
        {
            if (nodeChecker == null) throw new ArgumentNullException("nodeChecker");

            IEnumerable<XmlNode> collection = null;

            if (isContainsChild)
            {
                collection = node.EnumChildNodes();
            }
            else
            {
                collection = node.ChildNodes.AsEnumerable<XmlNode>();
            }

            foreach (var xmlNode in collection)
            {
                if (nodeChecker(xmlNode))
                {
                    yield return xmlNode;
                }
            }
        }

        public static IEnumerable<XmlNode> GetChildNodeByTagName(this XmlNode node, string tagName, bool isContainsChild = false)
        {
            return GetChildNodes(node, x => x.Name.CompareTo(tagName, true), isContainsChild);
            //IEnumerable<XmlNode> collection = null;

            //if (isContainsChild)
            //{
            //    collection = node.EnumChildNodes();
            //}
            //else
            //{
            //    collection = node.ChildNodes.AsEnumerable<XmlNode>();
            //}

            //foreach (var xmlNode in collection)
            //{
            //    if (xmlNode.Name.CompareTo(tagName,false))
            //    {
            //        yield return xmlNode;
            //    }
            //}
        }

        /// <summary>
        /// 将xml节点转换为json字符串
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ToJson(this XmlNode node)
        {
            return JsonConvert.SerializeXmlNode(node);
        }

        /// <summary>
        /// 从json字符串转换为xmlnode
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static XmlNode ToXMLByJson(this string json)
        {
            return JsonConvert.DeserializeXmlNode(json);
        }


        #region XmlAttributeCollection

        /// <summary>
        /// 检查是否包含指定名称的Attribute
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool Contains(this XmlAttributeCollection collection, string name)
        {
            foreach (XmlAttribute attr in collection)
            {
                if (attr.Name.CompareTo(name,true))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        //public static IEnumerable<XmlNode> AsEnumerable(this XmlNodeList list)
        //{
        //    return new XmlNodeListEnumerable(list);
        //}

        //private class XmlNodeListEnumerable : IEnumerable<XmlNode>
        //{
        //    XmlNodeList _list;

        //    public XmlNodeListEnumerable(XmlNodeList list)
        //    {
        //        _list = list;
        //    }

        //    IEnumerator<XmlNode> IEnumerable<XmlNode>.GetEnumerator()
        //    {
        //        foreach (XmlNode node in _list)
        //        {
        //            yield return node;
        //        }
        //    }

        //    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        //    {
        //        foreach (XmlNode node in _list)
        //        {
        //            yield return node;
        //        }
        //    }
        //}
    }
}
