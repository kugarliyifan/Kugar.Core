using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.Printer
{
    /// <summary>
    ///     通用的打印结构
    /// </summary>
    public class UniversalPrintSchema
    {
        private Lazy<UniversalBlock> _reportHead = null;
        private Lazy<UniversalBlock> _pageHead = null;
        private Lazy<List<UniversalDetailTable>> _details = null;
        private Lazy<UniversalBlock> _pageFooter = null;
        private Lazy<UniversalBlock> _reportFooter = null;

        public UniversalPrintSchema()
        {
            _reportHead = new Lazy<UniversalBlock>(buildBlock);
            _pageHead = new Lazy<UniversalBlock>(buildBlock);
            _details = new Lazy<List<UniversalDetailTable>>(buildBlockList);
            _pageFooter = new Lazy<UniversalBlock>(buildBlock);
            _reportFooter = new Lazy<UniversalBlock>(buildBlock);

        }

        public UniversalBlock AddField(PrintElementLocation location, PrintElementItem element)
        {
            var block = new UniversalBlock();

            switch (location)
            {
                case PrintElementLocation.ReportHead:
                    this.ReportHead.Fields.Add(element);
                    break;
                case PrintElementLocation.PageHead:
                    this.PageHead.Fields.Add(element);
                    break;
                case PrintElementLocation.PageFooter:
                    this.PageFooter.Fields.Add(element);
                    break;
                case PrintElementLocation.ReportFooter:
                    this.ReportFooter.Fields.Add(element);
                    break;
            }

            return block;
        }

        public UniversalBlock AddDetailTable(string bindingName,string detailName, PropertyInfo property)
        {
            var block = new UniversalDetailTable();
            block.BindingProperty = bindingName;
            block.DetailName = detailName;
            

            var gType = property.PropertyType.GetGenericArguments().FirstOrDefault();

            if (gType != typeof(object))
            {
                var p = gType.GetPropertyInfoList(2);

                foreach (var propertyExecutor in p)
                {
                    var attr = propertyExecutor.Value.GetAttribute<PrintElementAttribute>(true);

                    if (attr == null)
                    {
                        continue;
                    }

                    var pe = buildFromAttribute(propertyExecutor.Key, propertyExecutor.Value.PropertyType, attr);

                    block.Fields.Add(pe);

                    if (propertyExecutor.Value.PropertyType == typeof(DynamicColumn))
                    {
                        var dycolumnAttribute = propertyExecutor.Value.GetAttribute<PrintDynamicColumnAttribute>(true);

                        if (dycolumnAttribute==null)
                        {
                            continue;
                        }

                        pe.DataType = dycolumnAttribute.DataType;
                        pe.DisplayType = PrintElementDisplayType.Text;
                        pe.Name = dycolumnAttribute.GroupName;
                        pe.IsDynamicColumn = true;
                        block.DynamicColumnConfig.Add(pe);
                    }

                    
                }
            }

            this.Details.Add(block);

            return block;


        }

        private PrintElementItem buildFromAttribute(string bindingName, Type propertyInfo, PrintElementAttribute attr)
        {
            //var attr = propertyExecutor.Value.TargetPropertyInfo.GetAttribute<PrintElementAtrribute>(true);

            var pe = new PrintElementItem();

            pe.BindingColumn = bindingName;// propertyExecutor.Key;
            pe.DataType = propertyInfo;// propertyExecutor.Value.TargetPropertyInfo.PropertyType;
            pe.HeaderText = attr.DisplayText;
            pe.DisplayType = attr.DisplayType;

            return pe;
        }


        public UniversalBlock ReportHead
        {
            get { return _reportHead.Value; }
        }
        public UniversalBlock PageHead { get { return _pageHead.Value; } }
        public List<UniversalDetailTable> Details { get { return _details.Value; } }
        public UniversalBlock PageFooter { get { return _pageFooter.Value; } }
        public UniversalBlock ReportFooter { get { return _reportFooter.Value; } }

        public bool HasReportHead { get { return _reportHead.IsValueCreated && _reportHead.Value.Fields.Count > 0; } }
        public bool HasPageHead { get { return _pageHead.IsValueCreated && _pageHead.Value.Fields.Count > 0; } }
        public bool HasDetails { get { return _details.IsValueCreated && _details.Value.Count > 0; } }
        public bool HasPageFooter { get { return _pageFooter.IsValueCreated && _pageFooter.Value.Fields.Count > 0; } }
        public bool HasReportFooter { get { return _reportFooter.IsValueCreated && _reportFooter.Value.Fields.Count > 0; } }


        public static UniversalPrintSchema LoadFrom(Type type)
        {
            var temp = new UniversalPrintSchema();

            var pList = type.GetPropertyExecutorList(2);

            foreach (var executorse in pList)
            {
                var prop = executorse.Value.TargetPropertyInfo;

                var elementAttr = prop.GetAttribute<PrintElementAttribute>(true);

                if (elementAttr != null)
                {
                    if (
                        !prop.PropertyType.IsValueType &&
                        prop.PropertyType != typeof(string) &&
                        prop.PropertyType.IsClass &&
                        prop.PropertyType.IsIEnumerable() &&
                        elementAttr.DisplayText == "" &&
                        prop.PropertyType.IsAssignableFrom(typeof(DetailWithDynamicColumnsCollection<>))
                        )
                    {
                        continue;
                    }


                    var printElement = new PrintElementItem();

                    printElement.BindingColumn = executorse.Key;
                    printElement.DataType = prop.PropertyType;
                    printElement.HeaderText = elementAttr.DisplayText;
                    printElement.DisplayType = elementAttr.DisplayType;

                    temp.AddField(elementAttr.Location, printElement);
                }
                else if(prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition().IsAssignableFrom(typeof(DetailWithDynamicColumnsCollection<>)))
                {
                    //var printElement = new PrintElementItem();
                    //printElement.DataType = prop.PropertyType;
                    //printElement.DisplayType = PrintElementDisplayType.Auto;
                    //printElement.BindingColumn = executorse.Key;
                    //printElement.Name = executorse.Key;

                    var attr = prop.GetAttribute<PrintElementDetailListAttribute>(true);

                    if (attr==null)
                    {
                        continue;
                    }



                    temp.AddDetailTable(executorse.Key,attr.DetailName, prop);

                }
            }

            return temp;
        }

        private static UniversalBlock buildBlock()
        {
            return new UniversalBlock();
        }

        private static List<UniversalDetailTable> buildBlockList()
        {
            return new List<UniversalDetailTable>();
        }
    }
}