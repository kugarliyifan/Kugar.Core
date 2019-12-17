using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections;
using Kugar.Core.BaseStruct;
using Kugar.Core.Collections;
using System.Xml;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.BaseStruct
{
    public class Tree : IEnumerable<TreeNode>
    {
        private System.Collections.Concurrent.ConcurrentDictionary<string, TreeNode> _cacheKey = new System.Collections.Concurrent.ConcurrentDictionary<string, TreeNode>();
        private ITree_DataProvider _provider = null;
        private TreeNode _root = null;
        private bool _isLazyLoad = false;
        private bool _isAutoSync = false;
        private object _lockObj = new object();
        private bool _isLoading = false;

        /// <summary>
        ///     
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="isLazyLoad"></param>
        /// <param name="isAutoSync">当节点发生修改时,是否自动同步到provider中</param>
        public Tree(ITree_DataProvider provider=null, bool isLazyLoad = false, bool isAutoSync = false)
        {
            _root =this.CreateNode();
            _root.Parent = null;
            _root.Tree = this;
            _isLazyLoad = isLazyLoad;
            _provider = provider;
            _isAutoSync = isAutoSync;

            reloadInternal();
        }

        #region 公共属性

        /// <summary>
        ///     是否使用延迟加载
        /// </summary>
        public bool LazyLoad { get { return _isLazyLoad; } }

        /// <summary>
        ///     是否自动同步
        /// </summary>
        public bool AutoSync { get { return _isAutoSync; } }

        public TreeNodeChildCollection Nodes
        {
            get { return _root.Nodes; }
        }

        #endregion

        #region 公有函数

        public void Clear()
        {
            lock (_lockObj)
            {
                foreach (var node in _cacheKey)
                {
                    node.Value.ChildNodeCollectionChanged -= node_ChildNodeCollectionChanged;
                    node.Value.ChildNodeCollectionChanging -= node_ChildNodeCollectionChanging;
                    node.Value.ChildNodeLoading -= node_ChildNodeLoading;
                    node.Value.PropertyChanging -= node_PropertyChanging;
                    node.Value.PropertyChanged -= node_PropertyChanged;
                }

                _cacheKey.Clear();
                _root.Nodes.Clear();
            }

        }

        public TreeNode GetNodeByKey(string key)
        {
            TreeNode node = null;

            if (_cacheKey.TryGetValue(key, out node))
            {
                return node;
            }
            else
            {
                return null;
            }
        }

        public TreeNode GetNodeByPath(string path)
        {
            var pathList = path.Split('\\');

            //TODO：根据路径返回节点
            return null;
        }

        public TreeNode CreateNode(string text = "", string key = "", object value = null)
        {
            var node = new TreeNode() { Key = key, Text = text, Value = value, Tree = this };

            if (_isLazyLoad)
            {
                node.ChildNodeLoading += node_ChildNodeLoading;
            }

            node.ChildNodeCollectionChanged += node_ChildNodeCollectionChanged;
            node.ChildNodeCollectionChanging += node_ChildNodeCollectionChanging;
            node.PropertyChanged += node_PropertyChanged;
            node.PropertyChanging += node_PropertyChanging;

            return node; // new TreeNode(this) { Key = key, Text = text, Value = value };
        }

        public void ReLoad()
        {
            reloadInternal();
        }

        public void Save()
        {
            if (_provider != null)
            {
                lock (_lockObj)
                {
                    _provider.Save(this);
                }
            }
        }

        public TreeNode[] GetParent(TreeNode node)
        {
            var lst = new List<TreeNode>();

            getParentInternal(node, lst);

            return lst.ToArray();
        }

        #endregion

        #region 公开事件

        public event EventHandler<NotifyCollectionChangedEventArgs<TreeNode>> Node_NodeCollectionChanged;
        public event EventHandler<PropertyChangedEventArgs> Node_PropertyChanged;

        #endregion

        #region 枚举器

        public IList<TreeNode> GetAllChild(TreeNode parentNode)
        {
            if (parentNode.IsLeaf)
            {
                return null;
            }

            List<TreeNode> _lst = new List<TreeNode>();

            foreach (var node in Nodes)
            {
                _lst.Add(node);
                //yield return node;

                foreach (var node1 in internalGetNode(node))
                {
                    _lst.Add(node1);
                    //yield return node1;
                }
            }

            return _lst;
        }

        public IEnumerator<TreeNode> AllNodes()
        {
            foreach (var node in Nodes)
            {
                yield return node;

                foreach (var node1 in internalGetNode(node))
                {
                    yield return node1;
                }
            }
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            foreach (var node in Nodes)
            {
                yield return node;

                //foreach (var node1 in internalGetNode(node))
                //{
                //    yield return node1;
                //}
            }

        }

        private IEnumerable<TreeNode> internalGetNode(TreeNode node)
        {
            //var count = VisualTreeHelper.GetChildrenCount(parent);
            if (!node.IsLeaf)
            {
                foreach (var childNode in node.Nodes)
                {
                    yield return childNode;

                    var childList = internalGetNode(childNode);

                    foreach (var treeNode in childList)
                    {
                        yield return treeNode;
                    }
                }

                //for (var i = 0; i < count; i++)
                //{
                //    var child = VisualTreeHelper.GetChild(parent, i);
                //    var t = child as T;
                //    if (t != null)
                //        yield return t;

                //    var children = FindChildren<T>(child);
                //    foreach (var item in children)
                //        yield return item;
                //}
            }

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        [Browsable(false)]
        [Obsolete]
        public TreeNode RootNode { get { return _root; } }        

        internal bool HasKey(string key)
        {
            return _cacheKey.ContainsKey(key);
        }

        internal void AddNewKey(string key, TreeNode value)
        {
            _cacheKey.TryAdd(key, value);
        }

        internal TreeNode RemoveKey(string key)
        {
            TreeNode node;

            if (_cacheKey.TryRemove(key, out node))
            {
                return node;
            }
            else
            {
                return null;
            }
        }



        private void node_ChildNodeLoading(object sender, TreeNodesLazyLoadingEventArgs e)
        {
            lock (sender)
            {
                var lst = _provider.GetChildCategory(((TreeNode)sender).Key);

                if (lst != null && lst.Count > 0)
                {
                    for (int i = 0; i < lst.Count; i++)
                    {
                        e.ChildList.Add((TreeNode)lst[i]);
                    }
                }
            }

        }

        private void node_ChildNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs<TreeNode> e)
        {
            if (_isAutoSync && _provider != null)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:

                        if (!_isLoading)
                        {
                            if (e.NewItems != null && e.NewItems.Count > 0)
                            {
                                foreach (var node in e.NewItems)
                                {
                                    _provider.Add((TreeNodeForStorage)node);
                                }
                            }
                        }

                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (e.OldItems != null && e.OldItems.Count > 0)
                        {
                            foreach (var node in e.OldItems)
                            {
                                node.ChildNodeCollectionChanged -= node_ChildNodeCollectionChanged;
                                node.ChildNodeLoading -= node_ChildNodeLoading;
                                _provider.Delete(node.Key);
                            }
                        }

                        break;
                    case NotifyCollectionChangedAction.Reset:

                        if (e.OldItems != null && e.OldItems.Count > 0)
                        {
                            foreach (var node in e.OldItems)
                            {
                                node.ChildNodeCollectionChanged -= node_ChildNodeCollectionChanged;
                                node.ChildNodeLoading -= node_ChildNodeLoading;
                                _provider.Delete(node.Key);
                            }
                        }

                        if (e.NewItems != null && e.NewItems.Count > 0)
                        {
                            foreach (var node in e.NewItems)
                            {
                                _provider.Add((TreeNodeForStorage)node);
                            }
                        }
                        break;
                }
            }

            if (Node_NodeCollectionChanged != null)
            {
                Node_NodeCollectionChanged(sender, e);
            }
        }

        private void node_ChildNodeCollectionChanging(object sender, NotifyCollectionChangingEventArgs<TreeNode> e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (_provider != null)
                {
                    try
                    {
                        _provider.AddCheck(e.NewItems[0]);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (_provider != null)
                {
                    try
                    {
                        _provider.DeleteCheck(e.NewItems[0]);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        private void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_isLoading && _isAutoSync && _provider != null)
            {
                _provider.Update((TreeNodeForStorage)sender);
            }

            if (Node_PropertyChanged!=null)
            {
                Node_PropertyChanged(sender, e);
            }
        }

        private void node_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (!_isLoading && _isAutoSync && _provider != null)
            {
                try
                {
                    _provider.Update((TreeNodeForStorage)sender);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }



        private void getParentInternal(TreeNode node, List<TreeNode> parentNodeList)
        {
            if (node.Parent != null)
            {
                parentNodeList.Add(node.Parent);
                getParentInternal(node.Parent, parentNodeList);
            }
        }

        //重新构造树
        private void reloadInternal()
        {
            _isLoading = true;

            this.Clear();

            if (_provider != null)
            {
                lock (_lockObj)
                {
                    if (_isLazyLoad)
                    {
                        var lst = _provider.GetChildCategory(null);
                        if (lst != null && lst.Count > 0)
                        {
                            this.Nodes.AddRange(lst.Cast(x => (TreeNode)x));
                        }
                    }
                    else
                    {
                        _provider.LoadTree(this);
                    }
                }
            }

            _isLoading = false;
        }


        #region 过期代码

        [Obsolete]
        public void Load(XmlDocument document)
        {
            if (!document.HasChildNodes)
            {
                return;
            }

            var rootXML = document.ChildNodes[0];

            buildTreeNode(rootXML, _root);

            //foreach (XmlNode xmlNode in rootXML.ChildNodes)
            //{
            //    buildTreeNode(xmlNode, _root);
            //}
        }

        [Obsolete]
        public XmlDocument ToXML()
        {
            XmlDocument document = new XmlDocument();

            var root = document.CreateNode(XmlNodeType.Element, "root", "");

            document.AppendChild(root);

            buildXMLNode(root, _root, document);

            return document;
        }



        private void buildXMLNode(XmlNode parent, TreeNode parentNode, XmlDocument document)
        {
            foreach (var treenode in parentNode.Nodes)
            {
                var xmlNode = document.CreateNode(XmlNodeType.Element, "Node", "");
                var nameAttribute = document.CreateAttribute("Text");
                var keyAttribute = document.CreateAttribute("Key");

                nameAttribute.Value = treenode.Text;
                keyAttribute.Value = treenode.Key;



                xmlNode.Attributes.Append(nameAttribute);
                xmlNode.Attributes.Append(keyAttribute);
                xmlNode.InnerXml = treenode.Value.ToStringEx();

                parent.AppendChild(xmlNode);
                if (!treenode.IsLeaf)
                {
                    buildXMLNode(xmlNode, treenode, document);
                }
            }
        }

        private void buildTreeNode(XmlNode node, TreeNode parentNode)
        {
            foreach (XmlNode xmlNode in node.ChildNodes)
            {
                var treeNode = new TreeNode();

                treeNode.Text = xmlNode.Attributes.GetStringByName("Text");
                treeNode.Key = xmlNode.Attributes.GetStringByName("Key");

                parentNode.Nodes.Add(treeNode);

                if (!xmlNode.HasChildNodes)
                {
                    if (!string.IsNullOrWhiteSpace(xmlNode.InnerText))
                    {
                        treeNode.Value = SerialObjectExt.DeserializeToObject(xmlNode.InnerText.ToStringEx());
                    }
                }
                else
                {
                    buildTreeNode(xmlNode, treeNode);
                }

            }

            //foreach (var xmlNode in node.ChildNodes)
            //{
            //    var xmlNode = document.CreateNode(XmlNodeType.Element, "Node", "");
            //    var nameAttribute = document.CreateAttribute("Text");
            //    var keyAttribute = document.CreateAttribute("Key");

            //    nameAttribute.Value = treenode.Text;
            //    keyAttribute.Value = treenode.Key;

            //    xmlNode.Attributes.Append(nameAttribute);
            //    xmlNode.Attributes.Append(keyAttribute);
            //    xmlNode.InnerXml = treenode.Value.ToStringEx();

            //    parent.AppendChild(xmlNode);
            //    if (!treenode.IsLeaf)
            //    {
            //        buildXMLNode(xmlNode, treenode, document);
            //    }
        }

        #endregion
    }

    public class TreeNode : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private Lazy<TreeNodeChildCollection> _nodes = null;
        private string _text = null;
        private string _key = null;
        private object _value = null;
        private object _tag = null;
        private string _tagString = null;
        private ITree_DataProvider _provider;


        internal TreeNode()
        {
            _nodes = new Lazy<TreeNodeChildCollection>(buildCollection, true);
        }

        public virtual TreeNodeChildCollection Nodes
        {
            get { return _nodes.Value; }
        }

        /// <summary>
        ///     是否为叶子节点
        /// </summary>
        public virtual bool IsLeaf
        {
            get { return !_nodes.IsValueCreated || _nodes.Value.Count <= 0; }
        }

        public Tree Tree { get; internal set; }

        public TreeNode Parent
        {
            get;
            internal set;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (!value.Equals(_text) && OnPropertyChanging("Text"))
                {
                    _text = value;

                    OnPropertyChanged("Text");
                }
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                if (!value.Equals(_key) && OnPropertyChanging("Key"))
                {
                    _key = value;

                    OnPropertyChanged("Key");
                }
            }
        }

        /// <summary>
        ///     序列化存储时,存储该属性的ToString函数返回的值
        /// </summary>
        public virtual object Value
        {
            get { return _value; }
            set
            {
                if (_value == null && value != null && OnPropertyChanging("Value"))
                {
                    _value = value;

                    OnPropertyChanged("Value");
                }

                if (_value != null && !_value.Equals(value) && OnPropertyChanging("Value"))
                {
                    _value = value;

                    OnPropertyChanged("Value");

                    //TODO;通知更改
                }
            }

        }

        public virtual string TagString
        {
            set
            {
                if (!_tagString.SafeEquals(value) && OnPropertyChanging("TagString"))
                {
                    _tagString = value;

                    OnPropertyChanged("TagString");

                }
            }
            get { return _tagString; }
        }

        /// <summary>
        ///     该值不进行序列化
        /// </summary>
        public virtual object Tag
        {
            set
            {
                if (!value.Equals(_tag) && OnPropertyChanging("Tag"))
                {
                    _tag = value;

                    OnPropertyChanged("Tag");
                }
            }
            get { return _tag; }
        }

       
        /// <summary>
        ///     指定该节点的子节点列表是否已经初始化(只有在tree的LazyLoad为true时,该属性才有意义)
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                return Tree.LazyLoad?this._nodes.IsValueCreated:true;
            }
        }

        public override string ToString()
        {
            return Text;
        }

        public virtual string Path
        {
            get { return Parent == null ? "\\" : Parent.Path + "\\" + this.Key; }
        }

        public static implicit operator TreeNode(TreeNodeForStorage item)
        {
            var category = new TreeNode();

            category.Key = item.Key;
            category.Text = item.Text;
            category.Value = item.Value;//.DeserializeToObject(null);

            return category;
        }

        private TreeNodeChildCollection buildCollection()
        {
            var collection = new TreeNodeChildCollection(this);
            collection.CollectionChanging += collection_CollectionChanging;
            collection.CollectionChanged += collection_CollectionChanged;

            if (ChildNodeLoading != null)
            {
                var e = new TreeNodesLazyLoadingEventArgs();
                ChildNodeLoading(this, e);
                if (e.HasChildren)
                {
                    collection.AddRange(e.ChildList);
                }
            }

            return collection;
        }

        void collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs<TreeNode> e)
        {
            if (ChildNodeCollectionChanged != null)
            {
                ChildNodeCollectionChanged(this, e);
            }
        }

        void collection_CollectionChanging(object sender, NotifyCollectionChangingEventArgs<TreeNode> e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (!e.NewItems[0].IsLeaf)
                {
                    var lst = this.Tree.GetAllChild(e.NewItems[0]);

                    foreach (var node in lst)
                    {
                        if (node == this)
                        {
                            e.IsEnable = false;
                            return;
                        }
                    }
                }
            }

            if (ChildNodeCollectionChanging != null)
            {
                ChildNodeCollectionChanging(this, e);
            }
        }

        private static TreeNode _empty = new TreeNode();

        public static TreeNode Empty
        {
            get { return _empty; }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propName);

                PropertyChanged(this, e);
            }
        }
        #endregion

        #region Implementation of INotifyPropertyChanging

        public event PropertyChangingEventHandler PropertyChanging;
        protected bool OnPropertyChanging(string propName)
        {
            if (PropertyChanging != null)
            {
                var e = new PropertyChangingEventArgs(propName);

                try
                {
                    PropertyChanging(this, e);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }

            }
            return true;
        }


        #endregion

        public event NotifyCollectionChangedEventHandler<TreeNode> ChildNodeCollectionChanged;
        public event NotifyCollectionChangingEventHandler<TreeNode> ChildNodeCollectionChanging;
        internal event EventHandler<TreeNodesLazyLoadingEventArgs> ChildNodeLoading;
    }

    public class TreeNodesLazyLoadingEventArgs : EventArgs
    {
        private Lazy<IList<TreeNode>> _childList = new Lazy<IList<TreeNode>>(() => new List<TreeNode>());

        public IList<TreeNode> ChildList { get { return _childList.Value; } }

        public bool HasChildren { get { return _childList.IsValueCreated && _childList.Value.Count > 0; } }
    }


    public class TreeNodeChildCollection : ListEx<TreeNode>
    {
        private TreeNode _node = null;

        public new TreeNode this[int index]
        {
            get { return base[index]; }
            set
            {
                throw new Exception("不允许直接替换节点");
            }
        }

        /// <summary>
        ///     该节点容器，从属于哪个节点
        /// </summary>
        /// <param name="pTree"></param>
        /// <param name="node"></param>
        internal TreeNodeChildCollection(TreeNode node)
        {
            _node = node;
        }

        protected override void InsertItem(int index, TreeNode item)
        {

            if (_node.Tree.HasKey(item.Key))
            {
                throw new ArgumentOutOfRangeException("item", "已包含指定Key的节点");
            }

            item.Parent = this._node;
            item.Tree = this._node.Tree;
            _node.Tree.AddNewKey(item.Key, item);


            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            try
            {
                var node = base[index];

                base.RemoveItem(index);
                _node.Tree.RemoveKey(node.Key);
            }
            catch (Exception)
            {
                throw;
            }


        }

    }

    public enum TreeOperationAction
    {
        Add,
        Delete,
        Modify
    }

    public class TreeNodeForStorage
    {
        public string Key;
        public string ParentKey;
        public object Value;
        public string Text;
        public string Path;

        public static implicit operator TreeNodeForStorage(TreeNode item)
        {
            var storage = new TreeNodeForStorage();

            storage.Key = item.Key;
            storage.Text = item.Text;
            storage.Value = item.Value;
            storage.ParentKey = item.Parent == null ? "" : item.Parent.Key;
            storage.Path = item.Path.ToStringEx();
            return storage;
        }
    }


    public interface ITree_DataProvider
    {
        bool Add(TreeNodeForStorage item);
        bool Update(TreeNodeForStorage item);
        bool Delete(string key);

        /// <summary>
        ///     将指定Tree中所有数据保存入DataProvider
        /// </summary>
        /// <param name="srcTree">源Tree结构</param>
        void Save(Tree srcTree);

        /// <summary>
        ///     将DataProvider中所有节点加载到指定的Tree中
        /// </summary>
        /// <param name="targetTree">目标Tree</param>
        void LoadTree(Tree targetTree);

        bool AddCheck(TreeNodeForStorage item);

        bool UpdateCheck(TreeNodeForStorage item);

        bool DeleteCheck(TreeNodeForStorage item);

        /// <summary>
        ///     获取指定路径下的所有节点,,路径格式为 \AAA\BBB
        /// </summary>
        /// <param name="path">路径</param>
        /// <returns>返回查询到的数据</returns>
        IList<TreeNodeForStorage> GetChildCategoryByPath(string path);

        /// <summary>
        ///     获取指定节点下的节点(只查询一层)
        /// </summary>
        /// <param name="parentKey"></param>
        /// <returns></returns>
        IList<TreeNodeForStorage> GetChildCategory(string parentKey);
    }


    #region "错误"
    public class TreeException_KeyExists : ApplicationException
    {
        public TreeException_KeyExists()
            : base("指定TreeNode的Key已存在相同的键")
        { }
    }

    public class TreeException_KeyNotExists : ApplicationException
    {
        public TreeException_KeyNotExists()
            : base("指定TreeNode的Key的键不存在")
        { }
    }

    public class TreeException_ParentNotExists : ApplicationException
    {
        public TreeException_ParentNotExists()
            : base("指定TreeNode的父节点不存在")
        { }
    }



    #endregion
}