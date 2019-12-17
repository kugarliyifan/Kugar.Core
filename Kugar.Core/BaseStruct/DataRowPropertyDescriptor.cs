using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

namespace Kugar.Core.BaseStruct
{
    public class DataRowPropertyDescriptor : PropertyDescriptor
    {
        private Type _propertyType;

        public DataRowPropertyDescriptor(DataTable tbl, string name)
            : base(name, null)
        {
            _propertyType = tbl.Columns[name].DataType;
        }

        #region Overrides of PropertyDescriptor

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {
            var node = (DataRow)component;
            return node[this.Name];
        }

        public override void ResetValue(object component)
        {

        }

        public override void SetValue(object component, object value)
        {
            var node = (DataRow)component;
            node[this.Name] = value;

            //var node = (DataTableToHierarchicalElemet)component;
            //((DataRow)node.Item)[this.Name] = value;
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
            get { return _propertyType; }
        }

        #endregion
    }
}
