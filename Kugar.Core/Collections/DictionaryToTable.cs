using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Kugar.Core.Collections
{
    /// <summary>
    ///     提供将Dictionary[string,object]类型数据转换为可供绑定的表格模式
    /// 
    ///     需在绑定之前提供 列名及列数据类型  列名与键的映射关系表
    /// </summary>
    /// <example>
    ///        //设置指定控件的绑定字段
    ///        ColName.DataPropertyName = "ItemName";
    ///        ColCurrentZ.DataPropertyName = "CurrentValueZ";
    /// 
    ///        //定义列信息
    ///        var column = new Dictionary[string, Type]{ {"ItemName", typeof (string)},{"CurrentValueZ", typeof (decimal)}};
    ///        var mapping = new Dictionary[string, string][2];
    /// 
    ///         //定义列与键的映射关系（一个Dictionary为一行的映射关系）
    ///                                     ["映射列名","实际Key名"]
    ///        mapping[0] = new Dictionary[string, string]{{"ItemName", "ItemName_Flux"},{"CurrentValueZ", "Sum_FluxZ"} }
    ///        mapping[1] = new Dictionary[string, string]{{"ItemName", "ItemName_J"},{"CurrentValueZ", "Sum_JZ"} };
    /// 
    ///      //构造数据
    ///       
    ///       //第一行数据
    ///       datas["ItemName_Flux"] = "流量信息";
    ///       datas["Sum_FluxZ"] =20;
    /// 
    ///        //第二行数据
    ///       datas["ItemName_J"] = "能量流量";
    ///       datas["Sum_JZ"] =50;
    ///
    ///         //创建数据源
    ///       var s = new DictionaryToTableDataSource(datas, column,mapping);
    /// 
    ///        //刷新数据源,强制重新生成行
    ///      s.Refresh();
    /// 
    ///       //设置dataGridView1的数据源
    ///       dataGridView1.DataSource = s;
    /// 
    ///</example>
    public class DictionaryToTableDataSource : BindingList<DictionaryToTableDataSource_Row>, ITypedList,INotifyPropertyChanged
    {
        private DictionaryToTableDataSource()
        {
            RowIndexColumnName = "RowIndex";
            IsAutoInsertRowIndex = false;
            RowIndexValueFormatting = "{0}";
            RowIndexStartIndex = 0;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="src">为数据绑定源提供数据的DictionaryEx类</param>
        /// <param name="_clumns">列名以及列的数据类型关系表,格式为:{列名,类型}</param>
        /// <param name="_mappingRelation">列名及key的映射关系表,格式为:{列名,key}</param>
        public DictionaryToTableDataSource(DictionaryEx<string, object> src,
                                                                Dictionary<string, Type> _clumns,
                                                                IEnumerable<Dictionary<string, string>> _mappingRelation):this()
        {
            if (src == null)
            {
                throw new ArgumentNullException(@"src",@"传递的数据源参数不能为空");
            }

            Items = src;

            Columns = new Dictionary<string, Type>(_clumns);

            MappingRelation = new List<Dictionary<string, string>>(_mappingRelation);

            //Rows = new List<DictionaryToTableDataSource_Row>(MappingRelation.Count);

            src.DictionaryChanged += src_DictionaryChanged;

            this.Refresh();

        }

        void src_DictionaryChanged(object sender, DictionaryChangedEventArgs<string> e)
        {
            switch (e.DictionaryChangedType)
            {
                case DictionaryChangedType.ItemAdded:
                case DictionaryChangedType.ItemDeleted:
                case DictionaryChangedType.Reset:
                    this.Refresh();
                    break;
            }
        }



        /// <summary>
        ///     返回指定的列配置关系,格式为:{列名,key}
        /// </summary>
        public Dictionary<string, Type> Columns { get; private set; }

        /// <summary>
        ///     与该对象绑定的字典数据
        /// </summary>
        public DictionaryEx<string, object> Items { get; private set; }

        public IList<DictionaryToTableDataSource_Row> Rows
        {
            get
            {
                return base.Items;
            }
        }

        /// <summary>
        ///     每行的映射关系列表,键值对为:  {导出列名,对应key名}
        /// </summary>
        public List<Dictionary<string, string>> MappingRelation { get; private set; }

        /// <summary>
        ///     获取指定行上,指定列对应的是数据源中的哪个关键字的值,返回的关键字的值为实际字典类中的值
        /// </summary>
        /// <param name="rowIndex">行号,从0开始</param>
        /// <param name="colName">要获取的列名</param>
        /// <returns>key的名称</returns>
        public string GetCellRealKeyName(int rowIndex,string colName)
        {
            if (rowIndex>MappingRelation.Count || rowIndex>base.Count)
            {
                //return string.Empty;
                throw new ArgumentException(@"指定行序号超过行上限","rowIndex");
            }

            if (!Columns.ContainsKey(colName))
            {
                return string.Empty;
                //throw new ArgumentException(@"不存在指定列名","colName");
            }

            var row = base[rowIndex];

            return row.GetCellRealKeyName(colName);
        }



        /// <summary>
        ///     判断当前行中,指定列与otherRow参数指定的目标中相同名称的列是否引用自相同数据源的相同key
        /// </summary>
        /// <param name="otherTable">用于比较的Table</param>
        /// <param name="rowIndex">将用于比较的行序号</param>
        /// <param name="colName">将用于判断的列名</param>
        /// <param name="isIgonDataSource">是否忽略引用的数据源而只判断列名,如果该值为true时,当出现对比的两个Row对象的Items属性所引用的数据源不是同一个时,自动忽略该错误</param>
        /// <returns>如果都引用自相同的数据源的相同列,则返回true,否则返回false</returns>
        public bool IsReferenceFromSameKey(DictionaryToTableDataSource otherTable, int rowIndex, string colName, bool isIgonDataSource)
        {
            if (this.Items!=otherTable.Items && !isIgonDataSource)
            {
                return false;
            }

            if (rowIndex<0 || rowIndex > this.Count || rowIndex > otherTable.Count)
            {
                throw new ArgumentException(@"指定行序号超过行上限", "rowIndex");
            }

            return base[rowIndex].IsReferenceFromSameKey(otherTable.Rows[rowIndex], colName, isIgonDataSource);
        }

        /// <summary>
        ///     设置是否自动插入一列作为行Index,默认列名为RowIndex,如需修改,请修改RowIndexColumnName属性
        /// </summary>
        public bool IsAutoInsertRowIndex { set; get; }

        /// <summary>
        ///     设置默认作为Index列的列名
        /// </summary>
        public string RowIndexColumnName { set; get; }

        /// <summary>
        ///     行序号的值格式化字符串,默认为 "{0}",可格式化的值只有一个,所以字符串中只能出现{0}
        /// </summary>
        public string RowIndexValueFormatting { set; get; }

        /// <summary>
        ///     行序号的起始值
        /// </summary>
        public int RowIndexStartIndex { set; get; }

        #region "绑定源行的重新生成"

        /// <summary>
        ///     根据传入的MappingRelation属性,重新组织数据源的行
        /// </summary>
        public void Refresh()
        {
            base.Clear();

            foreach (var mappingPair in MappingRelation)
            {
                var temp = new DictionaryToTableDataSource_Row(this.Items, mappingPair);

                temp.Table = this;

                this.Add(temp);
            }

            OnDataSourceRefresh(this, null);
        }

        public event EventHandler DataSourceRefresh;

        protected void OnDataSourceRefresh(object sender, EventArgs e)
        {
            if (DataSourceRefresh != null)
            {
                DataSourceRefresh(sender, e);
            }
        }

        #endregion

        #region "ITypedList接口处理"

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "DictionaryToTableDataSource";
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            var lst = new List<DictionaryToTableDataSource_CustomPropertyDescriptor>();

            //根据设定的列返回相应的属性名称
            foreach (var column in Columns)
            {
                var temp = new DictionaryToTableDataSource_CustomPropertyDescriptor(this.Items, column.Key,column.Value,null);

                lst.Add(temp);
            }

            if (IsAutoInsertRowIndex)
            {
                var t = new DictionaryToTableDataSource_CustomPropertyDescriptor(this.Items, RowIndexColumnName, typeof(int), null);

                lst.Add(t);
            }

            return new PropertyDescriptorCollection(lst.ToArray());
        }

        #endregion


        protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (e.ListChangedType==ListChangedType.ItemAdded)
            {
                var temp = base[e.NewIndex];

                if (temp == null)
                {
                    return;
                }

                temp.Table = this;

                temp.PropertyChanged += OnPropertyChanged;
            }
            else if(e.ListChangedType==ListChangedType.ItemDeleted)
            {
                var temp = base[e.OldIndex];

                if (temp == null)
                {
                    return;
                }

                temp.Table = this;

                temp.PropertyChanged -= OnPropertyChanged;
            }

            base.OnListChanged(e);
        }
            
        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    ///     字典类转换后的数据行,用于存放 列映射配置,,处理属性修改的事件通知,,以及作为属性调用的源对象,为属性调用提供字典类数据
    /// </summary>
    public class DictionaryToTableDataSource_Row : BindingList<DictionaryToTableDataSource_Row>, INotifyPropertyChanged, IDisposable, ITypedList
    {

        public DictionaryToTableDataSource_Row(DictionaryEx<string,object> _src):this(_src,null,null){}

        public DictionaryToTableDataSource_Row(DictionaryEx<string,object> _src,Dictionary<string, string> _mappingRelation):this(_src,null,_mappingRelation){}

        public DictionaryToTableDataSource_Row(DictionaryEx<string, object> _src, Dictionary<string,Type> columnType, Dictionary<string, string> _mappingRelation)
        {
            if (_src==null)
            {
                throw new ArgumentNullException("数据源不能为空");
            }

            if (_mappingRelation == null || _mappingRelation.Count<=0)
            {
                MappingRelation = new Dictionary<string, string>();

                foreach (var o in _src)
                {
                    MappingRelation.Add(o.Key, o.Key);
                }
            }
            else
            {
                MappingRelation = _mappingRelation;
            }

            

            if (columnType==null || columnType.Count<=0)
            {
                Columns = new Dictionary<string, Type>();

                foreach (var v in MappingRelation)
                {
                    Columns.Add(v.Key, v.Value.GetType());
                }
            }
            else
            {
                Columns = columnType;
            }

            _src.DictionaryChanged += OnPropertyChanged;

            Items = _src;

            this.Add(this);
        }

        /// <summary>
        ///     映射关系,键值对为:  {导出列名,对应key名}
        /// </summary>
        public Dictionary<string, string> MappingRelation { set; get; }

        /// <summary>
        ///     列类型
        /// </summary>
        public Dictionary<string, Type> Columns { set; get; }

        /// <summary>
        ///     关联到该类的字典数据
        /// </summary>
        public DictionaryEx<string, object> Items { set; get; }

        /// <summary>
        ///     当前行的序列
        /// </summary>
        public int RowIndex
        {
            get
            {
                if (Table==null)
                {
                    return -1;
                }

                var t = Table.IndexOf(this);

                if (t<0)
                {
                    return t;
                }

                return t+Table.RowIndexStartIndex;
            }
        }

        /// <summary>
        ///     当前行所关联的表对象
        /// </summary>
        public DictionaryToTableDataSource Table { internal set; get; }

        /// <summary>
        ///     获取指定行上,指定列对应的是数据源中的哪个关键字的值,返回的关键字的值为实际字典类中的值
        /// </summary>
        /// <param name="rowIndex">行号,从0开始</param>
        /// <param name="colName">要获取的列名</param>
        /// <returns>key的名称</returns>
        public string GetCellRealKeyName(string colName)
        {
            if (!Columns.ContainsKey(colName))
            {
                return string.Empty;
                //throw new ArgumentNullException("colName", @"不存在指定列名");
            }

            return MappingRelation[colName];
        }


        /// <summary>
        ///     判断当前行中,指定列与otherRow参数指定的目标中相同名称的列是否引用自相同数据源的相同key
        /// </summary>
        /// <param name="otherRow">用于比较的row</param>
        /// <param name="colName">将用于判断的列名</param>
        /// <param name="isIgonDataSource">是否忽略引用的数据源而只判断列名,如果该值为true时,当出现对比的两个Row对象的Items属性所引用的数据源不是同一个时,自动忽略该错误</param>
        /// <returns>如果都引用自相同的数据源的相同列,则返回true,否则返回false</returns>
        public bool IsReferenceFromSameKey(DictionaryToTableDataSource_Row otherRow,string colName,bool isIgonDataSource)
        {
            if (!Columns.ContainsKey(colName) || !otherRow.Columns.ContainsKey(colName))
            {
                return false;
                //throw new ArgumentNullException("colName", @"不存在指定列名");
            }

            if (this.Items!=otherRow.Items && !isIgonDataSource)
            {
                return false;
            }

            var srcKeyName = this.GetCellRealKeyName(colName);
            var devKeyName = otherRow.GetCellRealKeyName(colName);

            return srcKeyName == devKeyName;
        }


        protected void OnPropertyChanged(object sender, DictionaryChangedEventArgs<string> e)
        {
            if (e.DictionaryChangedType==DictionaryChangedType.ItemChanged)
            {
                if (PropertyChanged != null)
                {
                    lock (MappingRelation)
                    {
                        foreach (var map in MappingRelation)
                        {
                            if (map.Value == e.EffectKey)
                            {
                                PropertyChanged(this, new PropertyChangedEventArgs(map.Key));
                            }
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region "析构函数以及Dispose接口处理"

        private bool _isDisposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    this.Items.DictionaryChanged-= OnPropertyChanged;
                    this.Items = null;
                    _isDisposed = true;
                }
            }
        }

        ~DictionaryToTableDataSource_Row()
        {
            this.Dispose();
        }

        #endregion

        #region "处理ITypedList接口"

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return "DictionaryToTableDataSource_RowList";
        }

        public virtual PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            var lst = new List<DictionaryToTableDataSource_CustomPropertyDescriptor>();

            //根据设定的列返回相应的属性名称
            foreach (var column in Columns)
            {
                var temp = new DictionaryToTableDataSource_CustomPropertyDescriptor(this.Items, column.Key, column.Value, null);

                lst.Add(temp);
            }

            return new PropertyDescriptorCollection(lst.ToArray());
        }


        #endregion
    }

    /// <summary>
    ///     虚拟的属性调用类,用于处理属性值的获取以及设置
    /// </summary>
    public class DictionaryToTableDataSource_CustomPropertyDescriptor : CustomPropertyDescriptor
    {
        public DictionaryToTableDataSource_CustomPropertyDescriptor(IDictionary<string, object> src, string propName)
            : this(src, propName, typeof(object), null)
        {
        }

        public DictionaryToTableDataSource_CustomPropertyDescriptor(IDictionary<string, object> src, string propName, Type _valueType, Attribute[] attr)
            : base(src, propName, _valueType, attr)
        {
        }

        /// <summary>
        ///     根据属性名,返回相应的键值
        /// </summary>
        /// <param name="component">被调用该属性的对象,一般为DictionaryToTableDataSource_Row,该函数在传入的调用类中,读取需要的键值</param>
        /// <returns></returns>
        public override object GetValue(object component)
        {
            if (!(component is DictionaryToTableDataSource_Row))
            {
                return null;
            }

            var temp = (DictionaryToTableDataSource_Row)component;

            if (temp.Table!=null && 
                temp.Table.IsAutoInsertRowIndex && 
                !string.IsNullOrEmpty(temp.Table.RowIndexColumnName) && 
                base.Name==temp.Table.RowIndexColumnName)
            {
                try
                {
                    var str = string.Format(temp.Table.RowIndexValueFormatting, temp.RowIndex);
                    return str;
                }
                catch (Exception)
                {
                    return "";
                }

            }

            if (temp == null || !temp.MappingRelation.ContainsKey(base.Name))
            {
                return null;
            }

            string itemname = string.Empty;

            lock (temp.MappingRelation)
            {
                itemname = temp.MappingRelation[base.Name];
            }

            if (temp.Items.ContainsKey(itemname))
            {
                lock (temp.Items)
                {
                    return temp.Items[itemname];
                }
                
            }
            else
            {
                return null;
            }

        }


        public override void SetValue(object component, object value)
        {
            if (!(component is DictionaryToTableDataSource_Row))
            {
                return;
            }

            var temp = (DictionaryToTableDataSource_Row)component;

            if (temp == null || !temp.MappingRelation.ContainsKey(base.Name))
            {
                return;
            }

            string itemname = string.Empty;

            lock (itemname)
            {
                itemname = temp.MappingRelation[base.Name];
            }

            lock (temp.Items)
            {
                if (temp.Items.ContainsKey(itemname))
                {
                    temp.Items[itemname] = value;
                }
                else
                {
                    temp.Items.Add(itemname, value);
                }
            }




        }

    }

    /// <summary>
    ///     构件一个自定义的属性描述类,用于虚拟出给数据绑定控件用的属性
    /// </summary>
    public abstract class CustomPropertyDescriptor : PropertyDescriptor
    {
        protected string _propName = null;

        protected IDictionary<string, object> DataItems;

        protected Type _propertyType = typeof(object);


        internal CustomPropertyDescriptor(IDictionary<string, object> src, string propName)
            : this(src, propName, typeof(object), null)
        {
        }

        internal CustomPropertyDescriptor(IDictionary<string, object> src, string propName, Type _valuetype, Attribute[] attr)
            : base(propName, attr)
        {
            _propName = propName;
            DataItems = src;
            _propertyType = _valuetype;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        //该函数需要另行处理,返回值时,在列表中查询
        public override object GetValue(object component)
        {
            return null;
        }

        public override void ResetValue(object component)
        {
            //throw new NotImplementedException();
        }

        //该函数需要另行处理,设置值时,在列表中修改
        public override void SetValue(object component, object value)
        {

        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return this.GetType(); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return _propertyType; }
        }
    }
}
