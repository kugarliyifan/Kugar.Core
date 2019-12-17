using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using Kugar.Core.Collections;

namespace Kugar.Core.List
{
    
    public class DictionaryExCodeDomSerializer : CodeDomSerializer
    {
        public override object Serialize(IDesignerSerializationManager manager, object value)
        {
            var baseSerializer = (CodeDomSerializer)manager.GetSerializer(typeof(DictionaryEx<,>).BaseType, typeof(CodeDomSerializer));

            object codeObject = baseSerializer.Serialize(manager, value);

            if (codeObject is CodeStatementCollection)
            {
                var statements =(CodeStatementCollection)codeObject;

                var targetObject = base.SerializeToReferenceExpression(manager, value);
         
                var objCreate=new CodeObjectCreateExpression();

                statements.Insert(0,new CodeCommentStatement("This is a custom comment added by a custom serializer on " +DateTime.Now.ToLongDateString()));

                var d=new CodeMethodInvokeExpression(targetObject,"Add",);
            }

            return base.Serialize(manager, value);
        }
    }
}
