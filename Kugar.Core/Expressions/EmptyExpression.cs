using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;

namespace Kugar.Core.Expressions
{
    [Serializable]
    public class EmptyExpression : Expression, ISerializable
    {
        public string Data;

        public EmptyExpression(SerializationInfo si,StreamingContext context)
        {
            try
            {
                Data = si.GetString("Data");
            }
            catch (Exception)
            {
                
                throw;
            }
            
        }

        public EmptyExpression()
        {
            
        }

        public EmptyExpression(string data):base()
        {
            Data = data;
        }

        #region Implementation of ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Data",Data);

            //throw new NotImplementedException();
        }

        #endregion
    }
}
