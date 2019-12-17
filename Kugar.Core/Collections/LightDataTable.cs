using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Kugar.Core.BaseStruct
{
    //16+32*列数
    public class LightDataTable : IEnumerable<DataRow>, ITypedList
    {
        private PropertyDescriptorCollection cachePropertyDescriptorCollection = null;  //4+32*列数
        private object lockObject = new object();  //4+

        public ColumnCollection Columns { private set; get; }  //4+

        public LightDataTable()
        {
            Columns = new ColumnCollection(this);
            //Columns.CollectionChanged += columns_CollectionChanged;

            Rows = new RowCollection(this);
        }

        public DataRow NewRow()
        {
            return new DataRow(this);
        }

        public RowCollection Rows { private set; get; } //4+

        //void columns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    switch (e.Action)
        //    {
        //        case KNotifyCollectionChangedAction.Add:
        //            var newItemCount = e.NewStartingIndex;
        //            for (int i = newItemCount; i < e.NewItems.Count; i++)
        //            {
        //                ((LightDataColumn) e.NewItems[i]).Table = this;
        //            }
        //            break;
        //        case KNotifyCollectionChangedAction.Remove:
        //        case KNotifyCollectionChangedAction.Reset:
        //            foreach (var item in e.OldItems)
        //            {
        //                ((LightDataColumn)item).Table = null;
        //            }

        //            foreach (var column in Columns)
        //            {
        //                column.Table = null;
        //            }

        //            break;
        //    }

        //    //lock (lockObject)
        //    //{
        //    //    RefreshPropertyDescriptors();
        //    //}
        //}

        public void RefreshPropertyDescriptors()
        {
            var lst = new List<DataColumnPropertyDescriptor>(Columns.Count);

            foreach (var column in Columns)
            {
                lst.Add(new DataColumnPropertyDescriptor(column));

            }

            if (cachePropertyDescriptorCollection != null)
            {
                lock (cachePropertyDescriptorCollection)
                {
                    cachePropertyDescriptorCollection.Clear();
                }
            }

            cachePropertyDescriptorCollection = new PropertyDescriptorCollection(lst.ToArray());
            lst.Clear();
        }

        public static implicit operator DataTable(LightDataTable tbl)
        {
            if (tbl == null)
            {
                return null;
            }

            var datatable = new DataTable();

            foreach (var column in tbl.Columns)
            {
                datatable.Columns.Add(column.Name, column.ColumnDataType);
            }

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                var row = datatable.NewRow();

                foreach (var column in tbl.Columns)
                {
                    row[column.Name] = tbl.Rows[i][column.Name];
                }

                datatable.Rows.Add(row);
            }

            return datatable;
        }

        #region "IEnumerator接口"

        public IEnumerator<DataRow> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region "ITypedList"

        private const string listName = @"Column";
        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return listName;
        }

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (cachePropertyDescriptorCollection == null)
            {
                lock (lockObject)
                {
                    RefreshPropertyDescriptors();
                }

            }

            return cachePropertyDescriptorCollection;
        }

        #endregion
    }


    public class LightDataColumn
    {

        public LightDataColumn(string name, Type dataType, object defaultValue)
        {
            Name = name;
            ColumnDataType = dataType;
            DefaultValue = defaultValue;
            //ValueList = new List<object>();
        }

        public LightDataColumn(string name, Type dataType) : this(name, dataType, null) { }

        public string Name { set; get; }

        //internal List<object> ValueList { private set; get; }

        public LightDataTable Table { internal set; get; }

        public Type ColumnDataType { set; get; }

        public object DefaultValue { set; get; }

        public bool Equals(LightDataColumn other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (LightDataColumn)) return false;
            return Equals((LightDataColumn) obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        ~LightDataColumn()
        {
            //ValueList.Clear();
            //ValueList = null;
        }
    }

    public class DataRow : IEnumerable
    {
        //internal Dictionary<string, object> cacheValue = null;
        //internal Hashtable cacheValue = null;
        internal ListDictionary cacheValue = null;
        //static ColumnCompare compare=new ColumnCompare();
        internal DataRow(LightDataTable table)
        {
            Table = table;
            RowIndex = -1;
            //cacheValue = new Dictionary<string, object>();
            //cacheValue=new Hashtable();
            cacheValue = new ListDictionary();
        }

        public object this[string columnName]
        {
            get
            {
                return GetValueByColumnName(columnName);
            }
            set
            {
                SetValueByColumnName(columnName, value);
            }
        }

        public object this[int columnIndex]
        {
            get { return GetValueByColumnIndex(columnIndex); }
            set
            {
                SetValueByColumnIndex(columnIndex, value);
            }
        }

        public object this[LightDataColumn column]
        {
            get { return GetValueByColumn(column); }
            set { SetValueByColumn(column, value); }
        }

        public int RowIndex { get; internal set; }

        public object GetValueByColumnIndex(int columnIndex)
        {
            if (columnIndex>Table.Columns.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return GetValueByColumn(Table.Columns[columnIndex]);
        }

        public object GetValueByColumnName(string columnName)
        {
            var column = Table.Columns.GetColumnByName(columnName);
            return GetValueByColumn(column);
        }

        public object GetValueByColumn(LightDataColumn column)
        {
            var colName = column.Name;
            if (cacheValue.Contains(colName))
            {
                return cacheValue[colName];
            }
            else
            {
                if (!Table.Columns.Contains(column))
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    return column.DefaultValue;
                }
            }


            //if (RowIndex == -1)
            //{
            //    return cacheValue[column.Name];
            //}
            //else
            //{
            //    if (column.ValueList.Count < RowIndex)
            //    {
            //        return column.DefaultValue;
            //    }
            //    else
            //    {
            //        return column.ValueList[RowIndex];
            //    }
            //}

        }

        public void SetValueByColumnIndex(int columnIndex, object value)
        {
            if (columnIndex > Table.Columns.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            SetValueByColumn(Table.Columns[columnIndex], value);

            //var targetType = Columns[columnIndex].ColumnDataType;

            //if (value is DBNull)
            //{
            //    value = null;
            //}

            //if (value!=null && value.GetType() != targetType)
            //{
            //    try
            //    {
            //        var v = Convert.ChangeType(value, targetType);
            //    }
            //    catch (Exception)
            //    {
            //        throw;
            //    }
            //}

            //if (this.RowIndex==-1)
            //{
            //    cacheValue[Columns[columnIndex]] = value;
            //}
            //else
            //{
            //    Columns[columnIndex].ValueList[this.RowIndex] = value;
            //}

        }

        public void SetValueByColumnName(string columnName, object value)
        {
            var column = Table.Columns.GetColumnByName(columnName);

            SetValueByColumn(column, value);
        }

        public void SetValueByColumn(LightDataColumn column, object value)
        {
            var targetType = column.ColumnDataType;

            if (value is DBNull)
            {
                value = null;
            }

            //if (column.ValueList.Count < RowIndex)
            //{
            //    throw new ArgumentOutOfRangeException();
            //}

            if (value != null && value.GetType() != targetType)
            {
                //try
                //{
                Convert.ChangeType(value, targetType);
                //}
                //catch (Exception)
                //{
                //    throw;
                //}
            }

            var colName = column.Name;
            if (cacheValue.Contains(colName))
            {
                cacheValue[colName] = value;
            }
            else
            {
                if (!Table.Columns.Contains(column))
                {
                    throw new ArgumentOutOfRangeException();
                }
                else
                {
                    cacheValue.Add(colName,value);
                }
            }

            //if (this.RowIndex == -1)
            //{
            //    cacheValue[column.Name] = value;
            //}
            //else
            //{
            //    column.ValueList[this.RowIndex] = value;
            //}
        }

        public LightDataTable Table { internal set; get; }

        public IEnumerator GetEnumerator()
        {
            return cacheValue.GetEnumerator();
            //foreach (var column in Table.Columns)
            //{
            //    yield return column.ValueList[RowIndex];
            //}
            //return new DataRowDataEnumerator(this);
        }

        public class DataRowDataEnumerator : IEnumerator
        {
            private DataRow _row = null;
            private int colIndex = -1;
            public DataRowDataEnumerator(DataRow row)
            {
                _row = row;
            }

            public bool MoveNext()
            {
                if (_row == null || _row.Table.Columns.Count <= 0)
                {
                    return false;
                }

                if (colIndex < _row.Table.Columns.Count)
                {
                    colIndex++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                colIndex = -1;
            }

            public object Current
            {
                get
                {
                    return _row[colIndex];
                }
            }
        }

        //public class ColumnCompare:IComparer
        //{
        //    public int Compare(object x, object y)
        //    {
        //        var x1 = (string) x;
        //        var y1 = (string)y;

        //        return string.Compare(x1, y1,true);
        //    }
        //}
    }

    public class RowCollection : ObservableCollection<DataRow>, ITypedList
    {
        internal RowCollection(LightDataTable table)
        {
            Table = table;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);

            var refreshBeginIndex = -1;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    refreshBeginIndex = e.NewStartingIndex;
                    break;
                case NotifyCollectionChangedAction.Move:
                    refreshBeginIndex = Math.Min(e.NewStartingIndex, e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    refreshBeginIndex = e.OldStartingIndex;
                    break;

                case NotifyCollectionChangedAction.Reset:
                    refreshBeginIndex = 0;
                    break;
            }

            if (refreshBeginIndex == -1)
            {
                return;
            }

            var colCount = base.Count;

            for (int i = refreshBeginIndex; i < colCount; i++)
            {
                Items[i].RowIndex = i;
            }
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            return Table.GetListName(listAccessors);
        }

        public LightDataTable Table { internal set; get; }

        //protected override void InsertItem(int index, DataRow item)
        //{
        //    base.InsertItem(index, item);

        //    lock (Table.Columns)
        //    {
        //        var columnCount = Table.Columns.Count;
        //        var columns = Table.Columns;
                

        //        //for (int i = 0; i < columnCount; i++)
        //        //{
        //        //    columns[i].ValueList.Insert(index, item.cacheValue[columns[i].Name]);
        //        //}

        //        //foreach (var column in Table.Columns)
        //        //{
        //        //    column.ValueList.Insert(index, item.cacheValue[column]);
        //        //}
        //    }

        //    item.cacheValue.Clear();
        //    //item.cacheValue = null;
        //}

        //protected override void RemoveItem(int index)
        //{

        //    base.RemoveItem(index);

        //    lock (Table.Columns)
        //    {
        //        foreach (var column in Table.Columns)
        //        {
        //            column.ValueList.RemoveAt(index);
        //        }
        //    }

        //}

        //protected override void MoveItem(int oldIndex, int newIndex)
        //{
        //    base.MoveItem(oldIndex, newIndex);

        //    lock (Table.Columns)
        //    {
        //        foreach (var column in Table.Columns)
        //        {
        //            column.ValueList.Move(oldIndex, newIndex);
        //        }
        //    }


        //}

        protected override void ClearItems()
        {

            foreach (var row in Items)
            {
                lock (row)
                {
                    row.cacheValue.Clear();
                }
            }

            base.ClearItems();

            //lock (Table.Columns)
            //{
            //    foreach (var column in Table.Columns)
            //    {
            //        column.ValueList.Clear();
            //    }
            //}


        }

        //protected override void SetItem(int index, DataRow item)
        //{
        //    base.SetItem(index, item);

        //    lock (Table.Columns)
        //    {
        //        foreach (var column in Table.Columns)
        //        {
        //            column.ValueList[index] = item[column.Name];
        //        }
        //    }
        //}

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            return Table.GetItemProperties(listAccessors);
        }


    }



    public class ColumnCollection : ObservableCollection<LightDataColumn>
    {
        private Dictionary<string ,LightDataColumn> columnCache=new Dictionary<string, LightDataColumn>();
        public ColumnCollection(LightDataTable dataTable)
        {
            Table = dataTable;
        }

        public int GetColumnNameIndex(string Name)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                if (item.Name.Equals(Name))
                {
                    return i;
                }
            }

            return -1;
        }

        public LightDataTable Table { private set; get; }

        protected override void InsertItem(int index, LightDataColumn item)
        {
            if (item == null)
            {
                throw new NoNullAllowedException("插入的列对象不允许为空");
            }

            if (GetColumnByName(item.Name) != null)//columnCache.ContainsKey(item.Name))
            {
                throw new ArgumentException("不允许添加相同名称的列");
            }

            item.Table = Table;
            //item.ValueList.Clear();

            base.InsertItem(index, item);

            //for (int i = 0; i < Table.Rows.Count; i++)
            //{
            //    item.ValueList.Add(item.DefaultValue);
            //}

            columnCache.Add(item.Name,item);
        }

        protected override void RemoveItem(int index)
        {
            var col = base.Items[index];

            base.RemoveItem(index);

            //col.ValueList.Clear();

        }

        protected override void ClearItems()
        {
            foreach (var column in Items)
            {
                //column.ValueList.Clear();
            }

            base.ClearItems();

            //columnCache.Clear();
        }

        protected override void SetItem(int index, LightDataColumn item)
        {
            if (GetColumnByName(item.Name) != null)
            {
                throw new ArgumentException("不能添加相同名称的列");
            }

            //var oldItem = Items[index];

            base.SetItem(index, item);

            //oldItem.ValueList.Clear();
        }

        public LightDataColumn GetColumnByName(string name)
        {
            var colCount = Items.Count;

            //return Items.FirstOrDefault(p => p.Name == name);

            //if (columnCache.ContainsKey(name))
            //{
                //return columnCache[name];
            //}

                for (int i = 0; i < colCount; i++)
                {
                    if (string.Compare(Items[i].Name, name, StringComparison.Ordinal) == 0)
                    {
                        return Items[i];
                    }
                }

            //foreach (var column in Items)
            //{
            //    if (column.Name == name)
            //    {
            //        return column;
            //    }
            //}

            return null;
        }
    }

    public class DataColumnPropertyDescriptor : PropertyDescriptor
    {
        private LightDataColumn column = null;

        public DataColumnPropertyDescriptor(LightDataColumn src)
            : base(src.Name, null)
        {
            column = src;
        }

        public override bool CanResetValue(object component)
        {
            return true;
        }

        public override object GetValue(object component)
        {
            if (component is DataRow)
            {
                var row = (DataRow)component;
                return row[this.Name];
            }

            return null;
        }

        public override void ResetValue(object component)
        {
            SetValue(component, column.DefaultValue);
        }

        public override void SetValue(object component, object value)
        {
            if (component is DataRow)
            {
                //if (value.GetType() != column.ColumnDataType)
                //{
                //    try
                //    {
                //        var v = Convert.ChangeType(value, PropertyType);
                //    }
                //    catch (Exception)
                //    {
                //        throw;
                //    }
                //}

                var row = (DataRow)component;

                row[this.Name] = value;
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override Type ComponentType
        {
            get { return typeof(DataRow); }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override Type PropertyType
        {
            get { return column.ColumnDataType; }
        }
    }

    public static class DataAdpterExt
    {
        public static void Fill(System.Data.Common.DbDataAdapter da, LightDataTable tbl)
        {
            if (da == null || da.SelectCommand == null || tbl == null)
            {
                throw new NoNullAllowedException();
            }

            using (var dr = da.SelectCommand.ExecuteReader(CommandBehavior.SingleResult))
            {
                for (int i = 0; i < dr.FieldCount; i++)
                {
                    tbl.Columns.Add(new LightDataColumn(dr.GetName(i), dr.GetFieldType(i)));
                }

                if (!dr.HasRows)
                {
                    return;
                }

                while (dr.Read())
                {
                    var row = tbl.NewRow();

                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        row[i] = dr.GetValue(i);
                    }

                    tbl.Rows.Add(row);
                }
            }
        }

    }
}
