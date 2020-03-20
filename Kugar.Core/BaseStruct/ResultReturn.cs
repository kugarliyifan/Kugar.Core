using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Kugar.Core.ExtMethod;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fasterflect;
using Newtonsoft.Json.Serialization;

namespace Kugar.Core.BaseStruct
{
    public interface IEmptyResultReturn
    {
        /// <summary>
        ///     返回代码,自定义
        /// </summary>
        int ReturnCode { set; get; }

        /// <summary>
        ///     返回的文本提示信息
        /// </summary>
        string Message { set; get; }

        /// <summary>
        ///     表明函数执行是否成功
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        ///     执行过程中出现错误时的错误对象
        /// </summary>
        Exception Error { set; get; }
    }

    public interface IResultReturn:IEmptyResultReturn
    {


        /// <summary>
        ///     如果函数有返回值,则设置该属性
        /// </summary>
        Object ReturnData { set; get; }
    }

    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class EmptyResultReturn : IEmptyResultReturn
    {
        private string _message = string.Empty;

        public EmptyResultReturn(bool isSuccess, string message = "", int returnCode = 0, Exception error = null)
        {
            ReturnCode = returnCode;
            _message = message;
            IsSuccess = isSuccess;
            Error = error;
        }

        public EmptyResultReturn(Exception error)
        {
            Message = error.Message;
            IsSuccess = false;
        }

        /// <summary>
        ///     返回代码,自定义
        /// </summary>
        public int ReturnCode { set; get; }

        /// <summary>
        ///     返回的文本提示信息
        /// </summary>
        public string Message
        {
            set { _message = value; }
            get
            {
                if (string.IsNullOrWhiteSpace(_message))
                {
                    if (Error!=null)
                    {
                        return Error.Message;
                    }
                }
                else
                {
                    return _message;
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     表明函数执行是否成功
        /// </summary>
        public bool IsSuccess {protected  set; get; }

        /// <summary>
        ///     执行过程中出现错误时的错误对象
        /// </summary>
        public Exception Error { set; get; }
    }

    /// <summary>
    ///     函数执行后的返回值
    /// </summary>
    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class ResultReturn :/*ISerializable,*/ IResultReturn
    {
        protected object _returnData = null;
        private string _message = string.Empty;

        public ResultReturn(bool isSuccess, object returnData = null, string message = "", int returnCode = 0, Exception error = null)
        {
            ReturnCode = returnCode;
            _message = message;
            IsSuccess = isSuccess;
            Error = error;
            ReturnData = returnData;
            ReturnData = returnData;
        }

        //public ResultReturn(SerializationInfo info, StreamingContext context)
        //{
        //    ReturnCode = info.GetInt32("ReturnCode");
        //    Message = info.GetString("Message");
        //    IsSuccess = info.GetBoolean("IsSuccess");
        //    Error=(Exception)info.GetValue("Error",typeof(object));
        //    ReturnData = info.GetValue("ReturnData", typeof (object));

        //}

        public ResultReturn(Exception error)
        {
            Message = error.Message;
            IsSuccess = false;
        }

        /// <summary>
        ///     返回代码,自定义
        /// </summary>
        public virtual int ReturnCode { set; get; }

        /// <summary>
        ///     返回的文本提示信息
        /// </summary>
        public virtual string Message
        {
            set { _message = value; }
            get
            {
                if (string.IsNullOrWhiteSpace(_message))
                {
                    if (Error!=null)
                    {
                        return Error.Message;
                    }
                }
                else
                {
                    return _message;
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     表明函数执行是否成功
        /// </summary>
        public virtual bool IsSuccess { set; get; }

        /// <summary>
        ///     执行过程中出现错误时的错误对象
        /// </summary>
        public virtual Exception Error { set; get; }

        /// <summary>
        ///     如果函数有返回值,则设置该属性
        /// </summary>
        public virtual Object ReturnData
        {
            set => _returnData = value;
            get => _returnData;
        }

        public static implicit operator bool(ResultReturn d)
        {
            return d.IsSuccess;
        }

        public ResultReturn<T> Cast<T>(T value)
        {
            return new ResultReturn<T>(IsSuccess,value,Message,ReturnCode,Error);
        }

        public ResultReturn<T> Cast<T>(T successValue,T faildValue)
        {
            return new ResultReturn<T>(IsSuccess, IsSuccess?successValue:faildValue, Message, ReturnCode, Error);
        }

        public static ResultReturn Create(bool isSuccess, object returnData = null, string message = "", int returnCode = 0, Exception error = null)
        {
        	return new ResultReturn(isSuccess,returnData,message,returnCode,error);
        }

        //public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("ReturnCode",ReturnCode);
        //    info.AddValue("Message",Message);
        //    info.AddValue("IsSuccess",IsSuccess);
        //    info.AddValue("Error", Error);
        //    info.AddValue("ReturnData", ReturnData);

        //    //ReturnCode = info.GetInt32("ReturnCode");
        //    //Message = info.GetString("Message");
        //    //IsSuccess = info.GetBoolean("IsSuccess");
        //    //Error = (Exception)info.GetValue("Error", typeof(object));
        //    //ReturnData = info.GetValue("ReturnData", typeof(object));

        //}
    }

    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class ResultReturn<T> /*:ISerializable*/
    {
        private string _message = string.Empty;
        protected T _returnData = default(T) ;

        public ResultReturn()
        {
            
        }

        public ResultReturn(bool isSuccess,T returnData=default(T), string message = "", int returnCode = 0, Exception error = null)
        {
            ReturnCode = returnCode;
            ReturnData = returnData;
            IsSuccess = isSuccess;
            Message = message;
            Error = error;

        }

        //public ResultReturn(SerializationInfo info, StreamingContext context)
        //{
            
        //    ReturnCode = info.GetInt32("ReturnCode");
        //    Message = info.GetString("Message");
        //    IsSuccess = info.GetBoolean("IsSuccess");
        //    Error = (Exception)info.GetValue("Error", typeof(object));
        //    ReturnData = (T)info.GetValue("ReturnData", typeof(T));

        //}

        public ResultReturn(Exception error)
        {
            Message = error.Message;
            IsSuccess = false;
        }

        /// <summary>
        ///     返回代码,自定义
        /// </summary>
        public virtual int ReturnCode { set; get; }

        /// <summary>
        ///     返回的文本提示信息
        /// </summary>
        public virtual string Message
        {
            set { _message = value; }
            get
            {
                if (string.IsNullOrWhiteSpace(_message))
                {
                    if (Error != null)
                    {
                        return Error.Message;
                    }
                }
                else
                {
                    return _message;
                }

                return string.Empty;
            }
        }

        /// <summary>
        ///     表明函数执行是否成功
        /// </summary>
        public virtual bool IsSuccess { set; get; }

        /// <summary>
        ///     执行过程中出现错误时的错误对象
        /// </summary>
        public virtual Exception Error { set; get; }

        /// <summary>
        ///     如果函数有返回值,则设置该属性
        /// </summary>
        public virtual T ReturnData
        {
            set => _returnData = value;
            get => _returnData;
        }

        public static implicit operator bool (ResultReturn<T> d)
        {
            return d.IsSuccess;
        }

        public static implicit operator ResultReturn(ResultReturn<T> d)
        {
            return new ResultReturn(d.IsSuccess,returnData: d.ReturnData,message: d._message, returnCode: d.ReturnCode,error: d.Error);
        }

        public ResultReturn<T> Cast<T>(T value)
        {
            return new ResultReturn<T>(IsSuccess, value, Message, ReturnCode, Error);
        }

        public ResultReturn<T> Cast<T>(T successValue, T faildValue)
        {
            return new ResultReturn<T>(IsSuccess, IsSuccess ? successValue : faildValue, Message, ReturnCode, Error);
        }

        public static ResultReturn<T> Create<T>(bool isSuccess, T returnData = default(T), string message = "", int returnCode = 0, Exception error = null)
        {
        	return new ResultReturn<T>(isSuccess,returnData,message,returnCode,error);
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("ReturnCode", ReturnCode);
        //    info.AddValue("Message", Message);
        //    info.AddValue("IsSuccess", IsSuccess);
        //    info.AddValue("Error", Error);
        //    info.AddValue("ReturnData", ReturnData);
        //}
    }

    /// <summary>
    ///     函数执行成功的返回值
    /// </summary>
    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class SuccessResultReturn : ResultReturn
    {
        private static DefaultSuccessResultReturn _defaultValue =new DefaultSuccessResultReturn();
        public SuccessResultReturn() : base(true,null,string.Empty, 0,null)
        {
        }

        public SuccessResultReturn(object returnData):base(true,returnData,string.Empty,0,null){}

        /// <summary>
        /// 返回默认构建的成功标识对象,用于避免不需要返回数据,但每次都重新new一个对象的情况,节省内存及时间,注意使用Default属性返回的对象是全局唯一,对该对象内的属性做修改时,都无效,
        /// </summary>
        public static SuccessResultReturn Default => _defaultValue;

        private class DefaultSuccessResultReturn: SuccessResultReturn
        {
            /// <summary>
            /// 重载输出值,防止该值被修改
            /// </summary>
            public override object ReturnData { get { return null; } set {} }

            public override Exception Error { get { return null; } set {} }

            public override bool IsSuccess { get { return true; } set {} }

            public override string Message { get {return String.Empty;} set {} }

            public override int ReturnCode { get { return 0; } set {} }
        }
    }

#if NET45
    /// <summary>
    /// 该类主要用于兼容之前的代码,禁止使用,请使用SuccessReturn
    /// </summary>
    [JsonConverter(typeof(ResultReturnConverter)), Obsolete]
    public class SuccessResuleReturn : SuccessResultReturn { }

    /// <summary>
    /// 该类主要用于兼容之前的代码,禁止使用,请使用SuccessReturn
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [JsonConverter(typeof(ResultReturnConverter)), Obsolete]
    public class SuccessResuleReturn<T>: SuccessResultReturn<T> { }

#endif


    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class  SuccessResultReturn<T>: ResultReturn<T>
    {
        private static DefaultSuccessResultReturn<T> _default=new DefaultSuccessResultReturn<T>();

        public SuccessResultReturn():base(true)
        {
            base.ReturnData = default(T);
        }

        public SuccessResultReturn(T returnData) : base(true,returnData)
        {
            //base.ReturnData = returnData;
        }

        //public new T ReturnData { set; get; }

        public static DefaultSuccessResultReturn<T> Default => _default;

        
        public class DefaultSuccessResultReturn<T1>:SuccessResultReturn<T1>
        {
            /// <summary>
            /// 重载输出值,防止该值被修改
            /// </summary>
            public new T1 ReturnData { get { return default(T1); } set {} }

            public override Exception Error { get { return null; } set {} }

            public override bool IsSuccess { get { return true; } set {} }

            public override string Message { get {return String.Empty;} set {} }

            public override int ReturnCode { get { return 0; } set {} }
        }
    }




    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class FailResultReturn : ResultReturn
    {
        //private static FailResultReturn _defaultValue = new FailResultReturn("");
        public FailResultReturn(Exception error)
            : base(false, error, string.Empty, 0, null)
        {
        }

        public FailResultReturn(string errorMessage):base(false,null,errorMessage,0,new Exception(errorMessage))
        {}

        //public FailResultReturn Default { get { return _defaultValue; } } 

    }

    [JsonConverter(typeof(ResultReturnConverter))]
    //[Serializable]
    public class FailResultReturn<T> : ResultReturn<T>
    {
        //private static FailResultReturn _defaultValue = new FailResultReturn("");
        public FailResultReturn(Exception error)
            : base(false,default(T),  string.Empty, 0, error)
        {
        }

        public FailResultReturn(string errorMessage,int errorCode=0)
            : base(false, default(T), errorMessage, errorCode, new Exception(errorMessage))
        { }

        //public FailResultReturn Default { get { return _defaultValue; } }
    }



    public class ResultReturnConverter : JsonConverter
    {
        //private static Dictionary<Type, MethodInfo> _dic = new Dictionary<Type, MethodInfo>();
        private static Type _checkerType = typeof(ResultReturn<>);
        private static Type _checkerType1 = typeof(ResultReturn);
        //private static ConcurrentDictionary<string,Type> _cacheDic=new ConcurrentDictionary<string, Type>(); 
        private bool _isGeneric = false;
        private static Type _jObjectType = typeof (JObject);
        private static Type _objectType = typeof (object);
        private static object _emptyObj=new object();

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {

            var error = (Exception)value.GetPropertyValue("Error");
            var isSuccess = (bool)value.GetPropertyValue("IsSuccess");
            var message = (string)value.GetPropertyValue("Message");
            var returnCode = (int)value.GetPropertyValue("ReturnCode");
            var returnData = value.GetPropertyValue("ReturnData");

            //var isCamel = serializer.ContractResolver is CamelCasePropertyNamesContractResolver;

            writer.WriteStartObject();

            var c = serializer.ContractResolver as DefaultContractResolver;

            writer.WritePropertyName(c?.GetResolvedPropertyName("IsSuccess")??"IsSuccess");
            writer.WriteValue(isSuccess);

            writer.WritePropertyName(c?.GetResolvedPropertyName("ReturnData")??"ReturnData");

            Type type = null;

            if (_isGeneric)
            {
                type = value.GetType().GetGenericArguments()[0];

                //if (type.Name.Contains("Anonymous"))
                //{
                //    type = typeof (JObject);
                    
                //    writer.WritePropertyName(typeof(JObject).FullName);
                ////}
                if (returnData==null)
                {
                    serializer.Serialize(writer,_emptyObj, type);
                }
                else
                {
                    serializer.Serialize(writer, returnData, type);
                }
                //if (type==typeof(JObject))
                //{
                //    JObject.FromObject(returnData).WriteTo(writer);
                //}
                //else
                //{
                    
                //}
            }
            else
            {
                if (returnData==null)
                {
                    writer.WriteStartObject();
                    writer.WriteEndObject();

                    //writer.WriteStartArray();
                    //writer.WriteEndArray();
                    //serializer.Serialize(writer, returnData);
                }
                else
                {
                    type = value.GetType();
                    serializer.Serialize(writer, returnData,type);
                    
                    //var typeName = returnData.GetType().AssemblyQualifiedName;

                    //writer.WritePropertyName("ReturnDataType");

                    //writer.WriteValue(typeName);
                }
                

            }


            writer.WritePropertyName(c?.GetResolvedPropertyName("Error") ?? "Error");

            if (error == null)
            {
                writer.WriteNull();
            }
            else
            {
                //writer.WriteValue(error);
                serializer.Serialize(writer, error);
            }
            

            writer.WritePropertyName(c?.GetResolvedPropertyName("Message") ?? "Message");

            if (message==null)
            {
                writer.WriteNull();
            }
            else
            {
                writer.WriteValue(message);
            }
            

            writer.WritePropertyName(c?.GetResolvedPropertyName("ReturnCode") ?? "ReturnCode");
            writer.WriteValue(returnCode);

            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartObject)
            {
                var s = (JObject)serializer.Deserialize(reader);

                var isCamel = serializer.ContractResolver is CamelCasePropertyNamesContractResolver;


                var isSuccess = s.GetBool(isCamel? "isSuccess" : "IsSuccess");
                Type type = null;
                object returnData = null;
                Exception error = null;
                var typeName = s.GetString("ReturnDataType");

                //if (!string.IsNullOrWhiteSpace(typeName) )
                //{
                //    //if (typeName.Contains("Anonymous"))
                //    //{
                //    //    type = _jObjectType;
                //    //}
                //    //else if (string.IsNullOrWhiteSpace(typeName))
                //    //{
                //    //    type = Type.GetType(typeName, true);
                //    //}
                //    //else 
                //    if (objectType.IsGenericType)
                //    {
                //        type=objectType.GetGenericArguments()[0];
                //    }
                //    else
                //    {
                        
                //        type = _objectType;
                //    }
                //}
                //else 
                if (objectType.IsGenericType)
                {
                    type = objectType.GetGenericArguments()[0];
                }
                else
                {
                    type = _jObjectType;
                }

                if (type == _objectType)
                {
                    type = _jObjectType;
                }
                
                if (isSuccess)
                {
                    var returnDataJson = s.GetValue(isCamel ? "returnData" : "ReturnData");

                    if (returnDataJson.Type== JTokenType.Null)
                    {
                        returnData = null;
                    }
                    else
                    {
                        returnData = serializer.Deserialize(returnDataJson.CreateReader(), type);
                    }
                    
                    if (type.IsArray && !(returnData is Array))
                    {
                        returnData = getMethodInvoker(type)(null, returnData);
                    }
                }
                else
                {
                    returnData = type.GetDefaultValue();
                    error = (Exception)serializer.Deserialize(s.GetValue(isCamel ? "error": "Error").CreateReader(), typeof(Exception));
                }

                var returnCode = s.GetInt(isCamel ? "returnCode": "ReturnCode");
                var message = s.GetString(isCamel ? "message": "Message");

                //var isSuccess = (bool)JToken.ReadFrom(reader);

                //var type = objectType.GetGenericArguments()[0];
                //var returnData = serializer.Deserialize(reader, type);
                //var error = serializer.Deserialize<Exception>(reader);
                //var message = reader.ReadAsString();
                //var returnCode = reader.ReadAsInt32().GetValueOrDefault();

                //return objectType.Create(isSuccess, returnData, message, returnCode, error);

                //Fasterflect.ConstructorExtensions.CreateInstance()

                var value = Activator.CreateInstance(objectType, isSuccess, returnData, message, returnCode, error);

                //value.FastSetValue("IsSuccess", isSuccess);
                //value.FastSetValue("ReturnData", returnData);
                //value.FastSetValue("Error", error);
                //value.FastSetValue("Message", message);
                //value.FastSetValue("ReturnCode", returnCode);

                return value;
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            if (objectType == _checkerType1)
            {
                _isGeneric = false;

                return true;
            }
            else if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == _checkerType)
            {
                _isGeneric = true;

                return true;
            }

            return false;
        }

        private static ConcurrentDictionary<Type, MethodInvoker> _cacheInvoker = new ConcurrentDictionary<Type, MethodInvoker>();

        private MethodInvoker getMethodInvoker(Type arrayType)
        {
            return _cacheInvoker.GetOrAdd(arrayType, x =>
            {
                var el = arrayType.GetElementType();
                return typeof(Enumerable).DelegateForCallMethod(new[] { el }, "ToArray",
                    typeof(IEnumerable<>).MakeGenericType(el));

            });
        }
    }
}
