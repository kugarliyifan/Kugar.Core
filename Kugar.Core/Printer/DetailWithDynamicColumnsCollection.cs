using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kugar.Core.Collections;

namespace Kugar.Core.Printer
{
    public class DetailWithDynamicColumnsCollection<T> : BindingListEx<T>, IDetailWithDynamicColumnsCollection where T : class 
    {
        Dictionary<string, ColumnConfig> _dynamicColumns = new Dictionary<string, ColumnConfig>();

        public Dictionary<string, ColumnConfig> DynamicColumns { get { return _dynamicColumns; } }

        public ColumnConfig this[string dynamicColumnName]
        {
            get { return _dynamicColumns[dynamicColumnName]; }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            addItemEvent(item);
        }

        protected override void RemoveItem(int index)
        {
            var oldItem = base[index];

            base.RemoveItem(index);

            if (oldItem != null)
            {
                removeItemEvent(oldItem);
            }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = base[index];

            base.SetItem(index, item);

            removeItemEvent(oldItem);

            addItemEvent(item);
        }

        protected override void ClearItems()
        {
            foreach (var item in base.Items)
            {
                removeItemEvent(item);
            }

            base.ClearItems();
        }

        protected void addItemEvent(T item)
        {
            if (item == null)
            {
                return;
            }

             
            var propertylist = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

            foreach (var prop in propertylist)
            {
                if(prop.PropertyType ==typeof(DynamicColumn))
                {
                    var v = (DynamicColumn)prop.GetValue(item, null);

                    v.Collection = this;

                    v.ValueChanged += item_DynamicColumnValueChanged;

                    var groupName = v.GroupName;// v.FastGetValue<string>("GroupName");

                    ColumnConfig column;

                    if (_dynamicColumns.TryGetValue(groupName,out column))
                    {
                        column.IncrementRefCount();//.RefCount += 1;
                    }
                    else
                    {
                        column=new ColumnConfig();
                        column.HeaderText = v.HeaderText;
                        //column.RefCount = 1;
                        _dynamicColumns.Add(groupName,column);

                        //引发新增动态列的事件
                        OnDynamicColumnChanged(DynamicColumnChangedAction.AddColumn, groupName);
                    }

                    var isNeedRefreshColumn = false;
                    foreach (var header in v.Headers)
                    {
                    	column.Increment(header);
                    	
//                        if (!column.HeaderTextList.ContainsKey(header))
//                        {
//                            column.Add(header);
//                            isNeedRefreshColumn = true;
//                        }
//                        else
//                        {
//                        	column.Increment(header);
//                        }
                    }

                    if (isNeedRefreshColumn)
                    {
                        //引发第二层表头修改事件
                        OnDynamicColumnChanged(DynamicColumnChangedAction.RefreshChildColumn, groupName);
                    }
                }
            }
        }

        protected void removeItemEvent(T item)
        {
            if (item == null)
            {
                return;
            }

            var propertylist = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance);

            foreach (var prop in propertylist)
            {
                if (prop.PropertyType == typeof(DynamicColumn))
                {
                    var v = (DynamicColumn)prop.GetValue(item, null);

                    v.ValueChanged -= item_DynamicColumnValueChanged;

                    var groupName = v.GroupName;

                    ColumnConfig columnConfig;
                    if (!_dynamicColumns.TryGetValue(groupName, out columnConfig))
                    {
                        return;
                    }

                    columnConfig.DecrementRefCount();//.RefCount -= 1;

                    if (columnConfig.RefCount <= 0)
                    {
                        _dynamicColumns.Remove(groupName);

                        //引发移除动态列事件
                        OnDynamicColumnChanged(DynamicColumnChangedAction.RemoveColumn, groupName);
                        //return;
                    }

                    foreach (var headerText in v.Headers)
                    {
                    	columnConfig.Decrement(headerText);
                        //columnConfig.HeaderTextList[headerText] -= 1;
                    }

                    var emptyList = columnConfig.HeaderTextList.Where(x => x.Value == 0).ToArray();

                    bool isNeedRefresh = false;

                    if (emptyList != null && emptyList.Any())
                    {
                        isNeedRefresh = true;

                        foreach (var pair in emptyList)
                        {
                            columnConfig.Remove(pair.Key);
                        }
                    }

                    if (isNeedRefresh)
                    {
                        //引发第二层表头修改事件
                        OnDynamicColumnChanged(DynamicColumnChangedAction.RefreshChildColumn, groupName);
                    }
                }
            }

            //var groupName = item.FastGetValue<string>("GroupName");

            //var ei = item.GetType().GetEvent("ValueChanged");

            ////取消订阅事件
            //ei.RemoveEventHandler(item,Delegate.CreateDelegate(ei.EventHandlerType,this,method));

            //ColumnConfig columnConfig;
            //if (!_dynamicColumns.TryGetValue(groupName,out columnConfig))
            //{
            //    return;
            //}

            //columnConfig.RefCount -= 1;

            //if (columnConfig.RefCount<=0)
            //{
            //    _dynamicColumns.Remove(groupName);

            //    //引发移除动态列事件
            //    OnDynamicColumnChanged(DynamicColumnChangedAction.RemoveColumn, groupName);
            //    return;
            //}

            //var headersList = item.FastGetValue<IEnumerable<string>>("Headers");

            //foreach (var headerText in headersList)
            //{
            //    columnConfig.HeaderTextList[headerText] -= 1;
            //}

            //var emptyList = columnConfig.HeaderTextList.Where(x => x.Value == 0);

            //if (emptyList!=null && emptyList.Any())
            //{
            //    foreach (var pair in emptyList)
            //    {
            //        columnConfig.HeaderTextList.Remove(pair.Key);
            //    }

            //    //引发第二层表头修改事件
            //    OnDynamicColumnChanged(DynamicColumnChangedAction.RefreshChildColumn, groupName);
            //}



        }


        private void item_DynamicColumnValueChanged(object sender, DynamicColumnValueChangedEventArgs e)
        {
            if (e.Column==null || !_dynamicColumns.ContainsKey(e.Column.GroupName))
            {
                return;
            }

            ColumnConfig columnConfig;

            if (!_dynamicColumns.TryGetValue(e.Column.GroupName,out columnConfig))
            {
                return;
            }

            if (e.Action== DynamicColumnValueChangedAction.Add)
            {
            	columnConfig.Increment(e.HeaderText);
            	
//                if(!columnConfig.ContainsKey(e.HeaderText))
//                {
//                    columnConfig.Add(e.HeaderText,1);
//
//                    //引发第二层表头修改事件
//                }
//                else
//                {
//                    columnConfig.HeaderTextList[e.HeaderText] += 1;
//                }
            }
            else if(e.Action== DynamicColumnValueChangedAction.Remove)
            {
            	columnConfig.Decrement(e.HeaderText);
            	
//                if (columnConfig.HeaderTextList.ContainsKey(e.HeaderText))
//                {
//                    columnConfig.HeaderTextList[e.HeaderText] -= 1;
//                }
            }
            
            //引发数据刷新事件
        }

        private void OnDynamicColumnChanged(DynamicColumnChangedAction action,string colName )
        {
            if (DynamicColumnChanged!=null)
            {
                DynamicColumnChanged(this,new DynamicColumnChangedEventArgs<DynamicColumn>(action,colName));
            }
        }



        /// <summary>
        ///     当动态列中的数据有变化时引发
        /// </summary>
        public event DynamicColumnValueChanged DynamicColumnValueChanged;

        /// <summary>
        ///     当新增的行中,有未添加的动态列组,则引发该事件
        /// </summary>
        public event DynamicColumnChanged<DynamicColumn> DynamicColumnChanged ;


    }
}