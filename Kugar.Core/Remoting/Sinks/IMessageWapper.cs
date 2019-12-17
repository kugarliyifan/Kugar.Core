using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization;
using System.Text;
using Kugar.Core.Collections;
using Kugar.Core.Exceptions;

namespace Kugar.Core.Remoting.Sinks
{
    public class MessageWapper : IMethodCallMessage, IMethodMessage, IMessage, ISerializable
    {
        private MessageDictionary _dic = null;
        private object[] _args = null;
        private object[] _inArgs = null;
        private string _methodName = "";
        private string[] _argName = null;
        private string[] _inArgName = null;
        private bool _hasVarArgs = false;
        private LogicalCallContext _logicalCallContext = null;
        private Type[] _methodSignature = null;
        private MethodBase _methodBase = null;
        private string _typeName;
        private string _uri = null;


        private IMethodCallMessage _srcMethodCallMessage = null;
        private IMethodMessage _srcMethodMessage = null;
        private IMessage _srcMessage = null;
        private ISerializable _srcSerial = null;


        public MessageWapper(SerializationInfo si, StreamingContext context)
        {

            _dic = new MessageDictionary(this);

            _args = (object[]) si.GetValue("_args", typeof (object[]));
            _methodName =si.GetString("_methodName");
            _hasVarArgs = si.GetBoolean("_hasVarArgs");
            _logicalCallContext = (LogicalCallContext)si.GetValue("_logicalCallContext", typeof(LogicalCallContext));
            _typeName = si.GetString("_typeName"); 

            var type = Type.GetType(_typeName, true, false);

            if (type==null)
            {
                throw new NullReferenceException(_typeName);
            }

            _methodBase = type.GetMethod(_methodName);

            var methodParams = _methodBase.GetParameters();
            if (methodParams!=null && methodParams.Length>0)
            {
                _methodSignature=new Type[methodParams.Length];
                _argName = new string[methodParams.Length];
                var tempInArgName = new List<string>(methodParams.Length);
                var tempInArgValue = new List<object>(methodParams.Length);

                for (int i = 0; i < methodParams.Length; i++)
                {
                    var item = methodParams[i];

                    _methodSignature[i] = item.ParameterType;
                    _argName[i] = item.Name;

                    if (item.IsIn)
                    {
                        tempInArgName.Add(item.Name);
                        tempInArgValue.Add(_args[i]);
                    }

                    _inArgs = tempInArgValue.ToArray();
                    _inArgName = tempInArgName.ToArray();
                }
            }
        }

        public MessageWapper(IMessage src)
        {
            _srcMethodCallMessage = (IMethodCallMessage) src;
            _srcMethodMessage = (IMethodMessage) src;
            _srcMessage = src;
            _srcSerial = (ISerializable) src;


            _dic = new MessageDictionary(this);

            _methodName = _srcMethodCallMessage.MethodName;
            _hasVarArgs = _srcMethodCallMessage.HasVarArgs;
            _logicalCallContext = _srcMethodCallMessage.LogicalCallContext;
            _methodBase = _srcMethodCallMessage.MethodBase;
            _typeName = _srcMethodCallMessage.TypeName;
            _uri = _srcMethodCallMessage.Uri;

            var tempTypeList = (Type[]) _srcMethodCallMessage.MethodSignature;

            if (tempTypeList!=null && tempTypeList.Length>0)
            {
                _methodSignature=new Type[tempTypeList.Length];
                tempTypeList.CopyTo(_methodSignature,0);
            }

            _args=new object[_srcMethodCallMessage.ArgCount];
            _argName=new string[_srcMethodCallMessage.ArgCount];

            for (int i = 0; i < _srcMethodCallMessage.ArgCount; i++)
            {
                _args[i] = _srcMethodCallMessage.GetArg(i);
                _argName[i] = _srcMethodCallMessage.GetArgName(i);
            }

            _inArgs = new object[_srcMethodCallMessage.InArgCount];
            _inArgName=new string[_srcMethodCallMessage.InArgCount];

            for (int i = 0; i < _srcMethodCallMessage.ArgCount; i++)
            {
                _inArgs[i] = _srcMethodCallMessage.GetInArg(i);
                _inArgName[i] = _srcMethodCallMessage.GetInArgName(i);
            }
        }

        public void SetValue(string key,object value)
        {
            if (_dic.ContainsKey(key))
            {
                _dic[key] = value;
            }
            else
            {
                _dic.Add(key, value);
            }
        }


        public IDictionary Properties
        {
            get { return _dic; }
        }

        #region Implementation of IMethodMessage

        public string GetArgName(int index)
        {
            return _argName[index];
        }

        public object GetArg(int argNum)
        {
            return _args[argNum];
        }

        public string Uri
        {
            get { return _uri; }
        }

        public string MethodName
        {
            get { return _methodName; }
            
        }

        public string TypeName
        {
            get { return _srcMethodMessage.TypeName; }
        }

        public object MethodSignature
        {
            get { return _methodSignature; }
        }

        public int ArgCount
        {
            get { return _args.Length; }
        }

        public object[] Args
        {
            get { return _args; }
        }

        public bool HasVarArgs
        {
            get { return _hasVarArgs; }
        }

        public LogicalCallContext LogicalCallContext
        {
            get { return _logicalCallContext; }
        }

        public MethodBase MethodBase
        {
            get { return _methodBase; }
        }

        #endregion

        #region Implementation of IMethodCallMessage

        public string GetInArgName(int index)
        {
            return _srcMethodCallMessage.GetInArgName(index);
        }

        public object GetInArg(int argNum)
        {
            return _inArgs[argNum];

            //return _srcMethodCallMessage.GetInArg(argNum);
        }

        public int InArgCount
        {
            get { return _inArgs!=null && _inArgs.Length>0?_inArgs.Length:0; }
        }

        public object[] InArgs
        {
            get { return _inArgs; }
        }

        #endregion

        #region Implementation of ISerializable

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_arg",this.Args);
            info.AddValue("_methodName ", this._methodName);
            info.AddValue("_argName", this._argName);
            info.AddValue("_hasVarArgs", this._hasVarArgs);
            info.AddValue("_logicalCallContext", this._logicalCallContext);
            info.AddValue("_typeName", this._typeName);
        }

        #endregion


        private class MessageDictionary:IDictionary
        {
            private static string[] keyList = new string[] { "__Uri", "__CallContext", "__MethodName", "__MethodSignature", "__TypeName", "__Args" };
            private MessageWapper _src=null;

            #region Implementation of IEnumerable

            public MessageDictionary(MessageWapper src)
            {
                _src = src;
            }

            public bool Contains(object key)
            {
                if (!(key is string))
                {
                    throw new KeyNotFoundException("key");
                }

                return keyList.Contains((string) key);
            }

            public void Add(object key, object value)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public IDictionaryEnumerator GetEnumerator()
            {
                return null;
                //foreach (var s in keyList)
                //{
                //    yield return new DictionaryEntry() {Key = s, Value = this[s]};
                //}
            }

            public void Remove(object key)
            {
                throw new NotImplementedException();
            }

            public object this[object key]
            {
                get
                {
                    if (!Contains(key))
                    {
                        throw new KeyNotFoundException("key");
                    }

                    switch ((string)key)
                    {
                        case "__Uri":
                            return _src.Uri;
                        case "__CallContext":
                            return _src.LogicalCallContext;
                        case "__MethodName":
                            return _src.MethodName;
                        case "__MethodSignature":
                            return _src.MethodSignature;
                        case "__TypeName":
                            return _src.TypeName;
                        case "__Args":
                            return _src.Args;
                        
                    }

                    throw new KeyNotFoundException();
                }
                set
                {
                    if (!Contains(key))
                    {
                        throw new KeyNotFoundException("key");
                    }

                    throw new KeyNotFoundException();
                }
            }

            public ICollection Keys
            {
                get { return keyList; }
            }

            public ICollection Values
            {
                get 
                { 
                    var valueList = new object[keyList.Length];

                    //"__Uri", "__CallContext", "__MethodName", "__MethodSignature", "__TypeName", "__Args", "__CallContext"

                    valueList[0] = _src.Uri;
                    valueList[1] = _src.LogicalCallContext;
                    valueList[2] = _src.MethodName;
                    valueList[3] = _src.MethodSignature;
                    valueList[4] = _src.TypeName;
                    valueList[5] = _src.Args;

                    return valueList;
                }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool IsFixedSize
            {
                get { return true; }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

            #region Implementation of ICollection

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public int Count
            {
                get { return keyList.Length; }
            }

            public object SyncRoot
            {
                get { throw new NotImplementedException(); }
            }

            public bool IsSynchronized
            {
                get { return false; }
            }

            #endregion

            public bool ContainsKey(string key)
            {
                return keyList.Contains(key);
            }

            private class MessageDictioanryEnumerator:IDictionaryEnumerator
            {
                #region Implementation of IEnumerator

                public bool MoveNext()
                {
                    throw new NotImplementedException();
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }

                public object Current
                {
                    get { throw new NotImplementedException(); }
                }

                #endregion

                #region Implementation of IDictionaryEnumerator

                public object Key
                {
                    get { throw new NotImplementedException(); }
                }

                public object Value
                {
                    get { throw new NotImplementedException(); }
                }

                public DictionaryEntry Entry
                {
                    get { throw new NotImplementedException(); }
                }

                #endregion
            }
        }
    }
}
