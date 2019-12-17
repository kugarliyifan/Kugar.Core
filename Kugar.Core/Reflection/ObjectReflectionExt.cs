using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Kugar.Core.BaseStruct;
using Kugar.Core.Collections;

namespace Kugar.Core.ExtMethod
{

    public static class ReflectionExt
    {
        public static Attribute GetAttribute(this MemberInfo mi, Type attributeType,bool inhertit=true)
        {
            var lst = mi.GetCustomAttributes(attributeType, inhertit);
            
            if (lst!=null && lst.Length>0)
            {
                return (Attribute)lst[0];
            }
            else
            {
                return null;
            }
        }

        public static T GetAttribute<T>(this MemberInfo mi, bool inhertit=true) where T:Attribute
        {
            return (T)GetAttribute(mi, typeof(T), inhertit);
        }

        public static bool HasAttribute(this MemberInfo mi, Type attributeType, bool inhertit)
        {
            var lst = mi.GetCustomAttributes(attributeType, inhertit);

            if (lst != null && lst.Length > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

	public static class TypeReflectionExt
	{
		private static Type objType=typeof(object);
		private static Dictionary<Type,List<CacheItem>> _cacheCreator=new Dictionary<Type, List<TypeReflectionExt.CacheItem>>();
		
		
		public static object FastCreate(Type type,params object[] argList)
		{
			//var t=new aaaa(ddd,xxx,www);
			
			//格式化参数

            var argTypeList = argList==null?new Type[0]: argList.Select(x => x != null ? x.GetType() : objType).ToArray();
			var f=getCacheFunc(type,argTypeList);
			
			if (f!=null) {
				return f(type,argList);
			}
			
			var constuctorList= type.GetConstructors();
			
			if (constuctorList.Length<0) {
				return null;
			}
			
			var paramExpressions=new Expression[argList.Length];
			
			var isSuccess=true;
			List<Type[]> targetParamTypes=new List<Type[]>();
			List<ConstructorInfo> targetConstr=new List<ConstructorInfo>();
			
			
			
			//判断指定类的构造函数,那些合适
			for (int i = 0; i < constuctorList.Length; i++) {
				var cPList=constuctorList[i].GetParameters();

				if (cPList.Length!=argList.Length) {
					continue;
				}
				
				for (int j = 0; j < cPList.Length; j++) {
					var pType=cPList[j].ParameterType;
					var argValue=argList[j];
					var argType=argTypeList[j];
					
					if ( (argValue==null && (!pType.IsClass || pType.IsValueType)) &&
					    !argType.IsAssignableFromEx(pType) &&
					     pType!=objType
					    ) 
					{
						isSuccess=false;
						break;
					}
				}
				
				if (isSuccess) {
					targetConstr.Add(constuctorList[i]);
					targetParamTypes.Add(cPList.Select(x=>x.ParameterType).ToArray());
				}
			}
			
			if (targetConstr.Count<=0) {
				throw new ArgumentOutOfRangeException("argList");
			}
			
			
			//筛选最合适的构造函数
			var matchIndex=0;
			var maxSum=0;
			
			var matchInc=argList.Length+1;
			
			for (int i = 0; i < targetParamTypes.Count; i++) {
				var tempSum=0;
				for (int j = 0; j < targetParamTypes[i].Length; j++) {
					var paramType=targetParamTypes[i][j];
					
					if (
						!(paramType==objType && argTypeList[j]==objType) && paramType==argTypeList[j] ||
						(paramType==objType && argTypeList[j]==objType)
						)
					{
						tempSum+=matchInc;
					}
					else if(paramType!=objType && argTypeList[j].IsAssignableFromEx(paramType))
					{
						tempSum+=(matchInc/2);
					}
				}
				
				if(tempSum>maxSum)
				{
					matchIndex=i;
				}
			}
			
			// parameters to execute
            ParameterExpression targetTypeParameter =
                Expression.Parameter(typeof(Type), "instance");
            ParameterExpression parametersParameter =
                Expression.Parameter(objType, "parameters");
			

            var changeTypeMehod=typeof(Convert).GetMethod("ChangeType", new Type[] { objType, typeof(Type)});;
            
            for (int i = 0; i < targetParamTypes[matchIndex].Length; i++)
            {
				//(dd)Convert.ChangeType(argList[i]);
				
                BinaryExpression valueObj = Expression.ArrayIndex(
                    parametersParameter, Expression.Constant(i));
                
				Expression valueCast=null;
				
				if (targetParamTypes[matchIndex][i]==argTypeList[i] ||
				    argTypeList[i].IsAssignableFrom(targetParamTypes[matchIndex][i]) ||
				    argTypeList[i].IsSubclassOf(targetParamTypes[matchIndex][i])
				   )
				{
	                valueCast = Expression.Convert(valueObj, targetParamTypes[matchIndex][i]);	
				}
				else
				{
					var v=Expression.Call(changeTypeMehod,valueObj,Expression.Constant(targetParamTypes[matchIndex][i]));
					valueCast = Expression.Convert(v, targetParamTypes[matchIndex][i]);
				}

                paramExpressions[i]=valueCast;
            }
			
			var newExpression=Expression.New(targetConstr[matchIndex],paramExpressions);
			
			UnaryExpression instanceCast = Expression.Convert(newExpression, type);
			
            var lambda =Expression.Lambda<Func<Type, object[], object>>(instanceCast,targetTypeParameter,parametersParameter);

            f=lambda.Compile();
            
            addCacheFunc(type,argTypeList,f);
            
            return f(type,argList);
			
		}
		
		private static void addCacheFunc(Type srcType,Type[] callArgsTypes,Func<Type, object[], object> func)
		{
			List<CacheItem> lst=null;
			
			if (!_cacheCreator.TryGetValue(srcType,out lst)) {
				lst=new List<CacheItem>();
				
				_cacheCreator.Add(srcType,lst);
			}

		    if (lst.Any(x => x.CallArgTypes.ElementsEquals(callArgsTypes)))
			{
				return;
			}
			else	
			{
				lst.Add(new CacheItem(){ CallArgTypes=callArgsTypes,CreatorFunc=func});
			}
		}
		
		private static Func<Type, object[], object> getCacheFunc(Type srcType, Type[] callArgsTypes)
		{
			List<CacheItem> cacheLst=null;
			
			if (_cacheCreator.TryGetValue(srcType,out cacheLst))
			{
			    var temp = cacheLst.FirstOrDefault(x => x.CallArgTypes.ElementsEquals(callArgsTypes));
                return temp.CreatorFunc;
			}
			else
			{
				return null;
			}
		}
		
		private class CacheItem
		{
			public Type[] CallArgTypes{set;get;}
			public Func<Type, object[], object> CreatorFunc{set;get;}
			
			#region Equals and GetHashCode implementation
			public override bool Equals(object obj)
			{
				TypeReflectionExt.CacheItem other = obj as TypeReflectionExt.CacheItem;
				if (other == null)
					return false;
            	
            	return this.CallArgTypes.ElementsEquals(other.CallArgTypes);
						                 
				//return object.Equals(this.CallArgTypes, other.CallArgTypes) && object.Equals(this.CreatorFunc, other.CreatorFunc);
			}
			
			public override int GetHashCode()
			{
				int hashCode = 0;
				unchecked {
					if (CallArgTypes != null)
						hashCode += 1000000007 * CallArgTypes.GetHashCode();
					if (CreatorFunc != null)
						hashCode += 1000000009 * CreatorFunc.GetHashCode();
				}
				return hashCode;
			}
			
			public static bool operator ==(TypeReflectionExt.CacheItem lhs, TypeReflectionExt.CacheItem rhs)
			{
				if (ReferenceEquals(lhs, rhs))
					return true;
				if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
					return false;
				return lhs.Equals(rhs);
			}
			
			public static bool operator !=(TypeReflectionExt.CacheItem lhs, TypeReflectionExt.CacheItem rhs)
			{
				return !(lhs == rhs);
			}
			#endregion

		}
	}

    public static class ObjectReflectionExt
    {
        #region "缓冲cache"

        private static Dictionary<MethodInfo, DynamicMethodExecutor> _cacheMethod = new Dictionary<MethodInfo, DynamicMethodExecutor>();

        private static Dictionary<PropertyInfo,DynamicPropertyExecutor<object >> _cacheProperty=new Dictionary<PropertyInfo, DynamicPropertyExecutor<object>>();

        private static MutileKeysDictionary<Type, string, IDynamicPropertyExecutor<object>> _cacheTypeProperty = new MutileKeysDictionary<Type, string, IDynamicPropertyExecutor<object>>();

        private static MutileKeysDictionary<Type, string, DynamicMethodExecutor> _cacheTypeMethody = new MutileKeysDictionary<Type, string, DynamicMethodExecutor>();

        #endregion

        #region "方法"

        /// <summary>
        ///     快速执行一个MethodInfo对象
        /// </summary>
        /// <typeparam name="T">执行后返回的值的类型</typeparam>
        /// <param name="methodInfo"></param>
        /// <param name="instance">要执行的对象的实例,如果是静态函数,可以传null</param>
        /// <param name="param">函数执行的参数</param>
        /// <returns>函数执行后的返回值</returns>
        public static T FastInvoke<T>(this MethodInfo methodInfo, object instance, params object[] param)
        {

            DynamicMethodExecutor m = GetExecutor(methodInfo);

            if (m!=null)
            {
                return (T)m.Execute(instance, param);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        ///     快速执行一个MethodInfo对象
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="instance">要执行的对象的实例,如果是静态函数,可以传null</param>
        /// <param name="param">函数执行的参数</param>
        /// <returns>函数执行后的返回值</returns>
        public static object FastInvoke(this MethodInfo methodInfo, object instance, params object[] param)
        {
            DynamicMethodExecutor m = GetExecutor(methodInfo);

            if (m != null)
            {
                return m.Execute(instance, param);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     快速执行一个MethodInfo对象
        /// </summary>
        /// <typeparam name="T">执行后返回的值的类型</typeparam>
        /// <param name="instance">要执行的对象的实例,如果是静态函数,可以传null</param>
        /// <param name="methodName">要执行的函数名,大小写敏感</param>
        /// <param name="param">函数执行的参数</param>
        /// <returns>函数执行后的返回值</returns>
        public static T FastInvoke<T>(this object instance, string methodName, params object[] param)
        {

            DynamicMethodExecutor m = GetTypeExecutor(instance.GetType(), methodName);

            if (m != null)
            {
                return (T)m.Execute(instance, param);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        ///     快速执行一个MethodInfo对象
        /// </summary>
        /// <param name="instance">要执行的对象的实例,如果是静态函数,可以传null</param>
        /// <param name="methodName">要执行的函数名,大小写敏感</param>
        /// <param name="param">函数执行的参数</param>
        /// <returns>函数执行后的返回值</returns>
        public static object FastInvoke(this object instance,string methodName, params object[] param)
        {
            DynamicMethodExecutor m = GetTypeExecutor(instance.GetType(), methodName);

            if (m != null)
            {
                return m.Execute(instance, param);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///     快速调用一个函数,并忽略抛出的错误
        /// </summary>
        /// <param name="target"></param>
        /// <param name="methodName"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static object InvokeIgnoreError(this object target, string methodName, params object[] param)
        {
            try
            {
                return FastInvoke(target, methodName,param);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #endregion

        #region "属性"

        /// <summary>
        ///     快速执行一个PropertyInfo对象的Get操作
        /// </summary>
        /// <typeparam name="T">执行后返回的值的类型</typeparam>
        /// <param name="propertyInfo">PropertyInfo对象</param>
        /// <param name="instance">要执行的对象的实例</param>
        /// <returns>属性的值</returns>
        public static T FastGetValue<T>(this PropertyInfo propertyInfo, object instance)
        {

            IDynamicPropertyExecutor<object> m = GetPropertyExecutor(propertyInfo);

            if (m != null)
            {
                return (T)m.GetValue(instance);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        ///     快速执行一个PropertyInfo对象的Get操作
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo对象</param>
        /// <param name="instance">要执行的对象的实例</param>
        /// <returns>属性的值</returns>
        public static object FastGetValue(this PropertyInfo propertyInfo, object instance)
        {
            return FastGetValue<object>(propertyInfo, instance);
        }

        /// <summary>
        ///     快速执行一个PropertyInfo对象的Set操作
        /// </summary>
        /// <param name="propertyInfo">PropertyInfo对象</param>
        /// <param name="instance">要执行的对象的实例</param>
        /// <param name="value">需要设置的新值</param>
        public static void FastSetValue(this PropertyInfo propertyInfo, object instance, object value)
        {
            IDynamicPropertyExecutor<object> m = GetPropertyExecutor(propertyInfo);

            if (m != null)
            {
                m.SetValue(instance, value);
            }
        }

        /// <summary>
        ///     快速执行一个PropertyInfo对象的Get操作
        /// </summary>
        /// <typeparam name="T">执行后返回的值的类型</typeparam>
        /// <param name="instance">要执行的对象的实例</param>
        /// <param name="expression">指定的属性名称,可以为表达式,格式为:"Propery1.Property2.Property3"</param>
        /// <returns>属性的值</returns>
        public static T FastGetValue<T>(this object instance, string expression)
        {

            //DynamicPropertyExecutor<object>[] m = GetTypeProperty(instance.GetType(), expression);
            IDynamicPropertyExecutor<object> m = GetTypeProperty(instance.GetType(), expression);
            T value = default(T);

            if (m!=null)
            {
                value = (T) m.GetValue(instance);
            }

            //if (m != null && m.Length>0)
            //{
            //    if (m.Length>1)
            //    {
            //        var tempInstance = instance;

            //        for (int i = 0; i < m.Length-1; i++)
            //        {
            //            tempInstance = m[i].GetValue(tempInstance);
            //        }

            //        value = (T) m[m.Length - 1].GetValue(tempInstance);
            //    }
            //    else
            //    {
            //        value = (T) m[0].GetValue(instance);
            //    }

            //    //return value;
            //}

            return value;
        }

        /// <summary>
        ///     快速执行一个PropertyInfo对象的Get操作
        /// </summary>
        /// <param name="instance">要执行的对象的实例</param>
        /// <param name="expression">指定的属性名称,可以为表达式,格式为:"Propery1.Property2.Property3"</param>
        /// <returns>属性的值</returns>
        public static object FastGetValue(this object instance, string expression)
        {
            return FastGetValue<object>(instance, expression);
        }

        /// <summary>
        ///     快速执行一个PropertyInfo对象的Set操作
        /// </summary>
        /// <param name="instance">要执行的对象的实例</param>
        /// <param name="expression">设置值的属性名,可以为表达式,格式为:"Propery1.Property2.Property3"</param>
        /// <param name="value">需要设置的新值</param>
        public static void FastSetValue(this object instance, string expression, object value)
        {
            //DynamicPropertyExecutor<object> m = GetTypeProperty(instance.GetType(), propertyName);



            //DynamicPropertyExecutor<object>[] m = GetTypeProperty(instance.GetType(), expression);

            IDynamicPropertyExecutor<object> m = GetTypeProperty(instance.GetType(), expression);

            if (m != null)
            {
                m.SetValue(instance, value);
            }

            //if (m != null)
            //{
            //    value = (T)m.GetValue(instance);
            //}

            ////T value = default(T);

            //if (m != null && m.Length > 0)
            //{
            //    if (m.Length > 1)
            //    {
            //        var tempInstance = instance;

            //        for (int i = 0; i < m.Length - 1; i++)
            //        {
            //            tempInstance = m[i].GetValue(tempInstance);
            //        }

            //        m[m.Length - 1].SetValue(tempInstance, value);

            //    }
            //    else
            //    {
            //        //value = (T)m[0].GetValue(instance);

            //        m[0].SetValue(instance,value);
            //    }

            //    //return value;
            //}
        }

        #endregion

        /// <summary>
        ///     缓存指定Type的所有属性(最大9层递归)
        /// </summary>
        /// <param name="type">将要缓存的Type</param>
        public static void CacheTypePropertyExecutor(this Type type)
        {
            _cacheTypeProperty.Remove(type);

            var temp = getPropertyExecutorListInternal(type,4);


            
            //var propertyList = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

            //var dic = new Dictionary<string, PropertyInfo>(propertyList.Length * 2);

            //int count = 1;

            //foreach (PropertyInfo col in propertyList)
            //{
            //    if (dic.ContainsKey(col.Name))
            //    {
            //        continue;
            //    }

            //    dic.Add(col.Name, col);

            //    if (!col.PropertyType.IsValueType &&
            //        col.PropertyType.IsClass &&
            //        col.PropertyType != typeof(string) &&
            //        !col.PropertyType.IsAssignableFrom(typeof(IEnumerable)) &&
            //        !col.PropertyType.IsAssignableFrom(typeof(IEnumerable<>)))
            //    {
            //        getNextLevelProperty(col, col.Name, dic, ref count, 9);
            //    }
            //}

            if (temp.Count > 0)
            {
                foreach (var p in temp)
                {
                    _cacheTypeProperty.Add(type, p.Key, p.Value);

                    //var pe = GetProperty(p.Value);

                    //_cacheTypeProperty.Add(type, p.Key, pe);
                }
            }
        }

        /// <summary>
        ///     枚举一个对象的所有属性,包括属性类型为对象时,也会深入该对象中
        /// </summary>
        /// <param name="type">将要枚举属性的类型</param>
        /// <param name="depth">递归枚举的最大深度,默认为2级</param>
        /// <param name="getFromCache">是否从缓存读取属性信息</param>
        /// <returns></returns>
        public static Dictionary<string, IDynamicPropertyExecutor<object>> GetPropertyExecutorList(this Type type, int depth = 2, bool getFromCache = false)
        {
            if (getFromCache)
            {
                if (!_cacheTypeProperty.ContainsKey(type))
                {
                    CacheTypePropertyExecutor(type);
                }
                return _cacheTypeProperty[type];
            }
            else
            {
                var tempList = getPropertyExecutorListInternal(type, depth);

                if(tempList!=null)
                {
                    return tempList;
                }
                else
                {
                    return null;
                }

                //return _cacheTypeProperty[type];
            }
        }

        /// <summary>
        ///     获取指定类型的属性列表
        /// </summary>
        /// <param name="type">类型Type对象</param>
        /// <param name="depth">最大枚举深度</param>
        /// <returns>返回值中,Key为属性字符串 如 Property1.Property2.Property3</returns>
        public static Dictionary<string,PropertyInfo> GetPropertyInfoList(this Type type, int depth = 2)
        {
            return getPropertyInfoListInternal(type, depth);
        }

        private static Dictionary<string, IDynamicPropertyExecutor<object>> getPropertyExecutorListInternal(Type type, int depth = 2)
        {

            var dic = getPropertyInfoListInternal(type, depth);

            if (dic!=null && dic.Count > 0)
            {
                var tempList = new Dictionary<string, IDynamicPropertyExecutor<object>>();

                foreach (var p in dic)
                {
                    var pe = GetPropertyExecutor(p.Value);

                    tempList.Add(p.Key, pe);
                }

                return tempList;
            }
            else
            {
                return null;
            }
        }

        private static Dictionary<string,PropertyInfo> getPropertyInfoListInternal(Type type, int depth = 2)
        {
            var propertyList = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

            var dic = new Dictionary<string, PropertyInfo>(propertyList.Length * 2);

            int count = 1;

            foreach (PropertyInfo col in propertyList)
            {
                if (dic.ContainsKey(col.Name))
                {
                    continue;
                }

                dic.Add(col.Name, col);

                var propType = col.PropertyType;

                if (!propType.IsValueType &&
                    propType.IsClass &&
                    propType != typeof(string) &&
                    !IsIEnumerable(propType)
                    )
                {
                    getNextLevelProperty(col, col.Name, dic, ref count, depth);
                }
            }

            return dic;
        }

        private static ConcurrentDictionary<Type, bool> _cacheEnumerableType=new ConcurrentDictionary<Type, bool>(); 
        public static bool IsIEnumerable(this Type type)
        {
            return _cacheEnumerableType.GetOrAdd(type, x =>
            {
                bool ret = false;

                if (type.IsArray)
                {
                    return true;
                }

                Type[] interfaceList = null;

                if (x.IsGenericType)
                {
                    interfaceList = x.GetGenericTypeDefinition().GetInterfaces();
                }
                else
                {
                    interfaceList = x.GetInterfaces();
                }

                foreach (var ifc in interfaceList)
                {
                    if (ifc.IsGenericType)
                    {
                        ret = ifc.GetGenericTypeDefinition() == typeof (IEnumerable<>);
                    }
                    else
                    {
                        ret = ifc.IsAssignableFrom(typeof (IEnumerable));
                    }

                    if (ret)
                    {
                        break;
                    }
                }

                return ret;
            });


        }

        private static void getNextLevelProperty(PropertyInfo parent,string parentName, Dictionary<string, PropertyInfo> lst, ref int currentDepth, int maxGetPropertyDepth)
        {
            foreach (PropertyInfo property in parent.PropertyType.GetProperties())
            {
                var name = string.Format("{0}.{1}", parentName, property.Name);

                if (lst.ContainsKey(name))
                {
                    continue;
                }

                //var destor = new SubsetPropertyDescriptor(parent, property, name);

                lst.Add(name, property);

                var propType = property.PropertyType;

                currentDepth += 1;
                if (!propType.IsValueType &&
                    propType.IsClass &&
                    propType != typeof(string) &&
                    !IsIEnumerable(propType) &&
                    currentDepth <= maxGetPropertyDepth
                    )
                {

                    getNextLevelProperty(property,name, lst, ref currentDepth, maxGetPropertyDepth);

                }
                currentDepth -= 1;
            }
        }



        public static IDynamicPropertyExecutor<object> GetPropertyExecutor(PropertyInfo propertyInfo)
        {
            DynamicPropertyExecutor<object> m = null;

            lock (_cacheProperty)
            {
                if (_cacheProperty.ContainsKey(propertyInfo))
                {
                    m = _cacheProperty[propertyInfo];
                }
                else
                {
                    m = new DynamicPropertyExecutor<object>(propertyInfo);
                    _cacheProperty.Add(propertyInfo, m);
                }
            }

            return m;
        }

        //private static DynamicPropertyExecutor<object>[] GetTypeProperty(Type type,string propertyName)
        //{
        //    if (type==null)
        //    {
        //        throw new TypeAccessException();
        //    }

        //    DynamicPropertyExecutor<object> m = null;

        //    var expression = "";

        //    // expression=obj.     --property1.property2

        //    var propertyNameList = expression.Split('.');

        //    var pList = new List<DynamicPropertyExecutor<object>>(propertyNameList.Length);

        //    var tempType = type;

        //    for (int i = 0; i < propertyNameList.Length; i++)
        //    {
        //        var pName = propertyNameList[i];

        //        var tempDynamic = GetProperty(tempType.GetProperty(pName));

        //        tempType =tempDynamic.TargetPropertyInfo.ReflectedType;

        //        pList.Add(tempDynamic);
        //    }

        //    lock (cacheTypeProperty)
        //    {
        //        if (cacheTypeProperty.ContainsKey(type, propertyName))
        //        {
        //            m = cacheTypeProperty[type, propertyName];
        //        }
        //        else
        //        {
        //            var pi = type.GetProperty(propertyName);

        //            if (pi == null)
        //            {
        //                throw new NullReferenceException("当前对象不存在指定名称的属性");
        //            }

        //            m = GetProperty(pi);

        //            cacheTypeProperty.Add(type, propertyName, m);
        //        }
        //    }
        //    return m;
        //}

        public static IDynamicPropertyExecutor<object> GetTypeProperty(this Type type, string expression)
        {
            if (type == null)
            {
                throw new Exception(@"类型为空");
            }

            IDynamicPropertyExecutor<object> m = null;

            lock (_cacheTypeProperty)
            {
                if (_cacheTypeProperty.ContainsKey(type, expression))
                {
                    m = _cacheTypeProperty[type, expression];
                }
                else
                {
                    var propertyNameList = expression.Split('.');

                    //var pList = new List<DynamicPropertyExecutor<object>>(propertyNameList.Length);

                    var tempType = type;

                    var pList = new List<PropertyInfo>(propertyNameList.Length);

                    for (int i = 0; i < propertyNameList.Length; i++)
                    {
                        var pName = propertyNameList[i];

                        //var tempDynamic = GetProperty(tempType.GetProperty(pName));

                        //tempType = tempDynamic.TargetPropertyInfo.PropertyType;

                        //pList.Add(tempDynamic);

                        var pInfo = tempType.GetProperty(pName);
                        if (pInfo != null)
                        {
                            pList.Add(pInfo);

                            tempType = pInfo.PropertyType;
                        }
                        else
                        {
                            break;
                        }

                    }

                    if (pList.Count!=propertyNameList.Length)
                    {
                        return null;
                    }

                    //m = pList.ToArray();


                    if (propertyNameList.Length>1)
                    {
                        m = new DynamicPropertyExecutor<object>(pList.ToArray());
                    }
                    else
                    {
                        m=new DynamicPropertyExecutor<object>(pList[0]);
                    }
                   

                    _cacheTypeProperty.Add(type, expression, m);
                }
            }
            return m;
        }


        public static DynamicMethodExecutor GetExecutor(MethodInfo methodInfo)
        {
            DynamicMethodExecutor m = null;

            lock (_cacheMethod)
            {
                if (_cacheMethod.ContainsKey(methodInfo))
                {
                    m = _cacheMethod[methodInfo];
                }
                else
                {
                    m = new DynamicMethodExecutor(methodInfo);

                    if (!_cacheMethod.ContainsKey(methodInfo))
                    {
                        _cacheMethod.Add(methodInfo, m);
                    }

                    if (!_cacheTypeMethody.ContainsKey(methodInfo.DeclaringType,methodInfo.Name))
                    {
                        _cacheTypeMethody.Add(methodInfo.DeclaringType, methodInfo.Name, m);
                    }
                }
            }

            return m;
        }

        public static DynamicMethodExecutor GetTypeExecutor(Type type, string methodName)
        {
            DynamicMethodExecutor m = null;

            lock (_cacheTypeMethody)
            {
                if (_cacheTypeMethody.ContainsKey(type, methodName))
                {
                    m = _cacheTypeMethody[type, methodName];
                }
                else
                {
                    var pi = type.GetMethod(methodName,BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (pi == null)
                    {
                        throw new NullReferenceException("当前对象不存在指定名称的方法");
                    }

                    m = GetExecutor(pi);

                    if (m==null)
                    {
                        return null;
                    }

                    if (!_cacheMethod.ContainsKey(m.TargetMethodInfo))
                    {
                        _cacheMethod.Add(m.TargetMethodInfo, m);
                    }

                    if (!_cacheTypeMethody.ContainsKey(type, methodName))
                    {
                        _cacheTypeMethody.Add(type, methodName, m);
                    }

                    //cacheTypeMethody.Add(type, methodName, m);
                    //cacheMethod.Add(m.TargetMethodInfo,m);
                }
            }
            return m;
        }

        
    }

    public interface IDynamicMethodExecutor
    {
        bool IsStaticMethod { get; }
        object Execute(object instance, object[] parameters);
    }

    public class DynamicMethodExecutor : IDynamicMethodExecutor
    {
        private Func<object, object[], object> m_execute;

        public DynamicMethodExecutor(MethodInfo methodInfo)
        {
            if (methodInfo==null)
            {
                throw new NullReferenceException("methodInfo参数为空");
            }

            this.m_execute = GetExecuteDelegate(methodInfo);
            TargetMethodInfo = methodInfo;
            this.IsStaticMethod = methodInfo.IsStatic;
        }

        public MethodInfo TargetMethodInfo { set; get; }

        public bool IsStaticMethod { private set; get; }

        public object Execute(object instance, object[] parameters)
        {
            if (IsStaticMethod)
            {
                return this.m_execute(null, parameters);

            }

            return this.m_execute(instance, parameters);
        }

        private static Func<object, object[], object> GetExecuteDelegate(MethodInfo methodInfo)
        {
            // parameters to execute
            ParameterExpression instanceParameter =
                Expression.Parameter(typeof(object), "instance");
            ParameterExpression parametersParameter =
                Expression.Parameter(typeof(object[]), "parameters");

            // build parameter list
            List<Expression> parameterExpressions = new List<Expression>();
            ParameterInfo[] paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                // (Ti)parameters[i]
                BinaryExpression valueObj = Expression.ArrayIndex(
                    parametersParameter, Expression.Constant(i));
                UnaryExpression valueCast = Expression.Convert(
                    valueObj, paramInfos[i].ParameterType);

                parameterExpressions.Add(valueCast);
            }

            // non-instance for static method, or ((TInstance)instance)
            Expression instanceCast = methodInfo.IsStatic ? null :
                Expression.Convert(instanceParameter, methodInfo.ReflectedType);

            // static invoke or ((TInstance)instance).Method
            MethodCallExpression methodCall = Expression.Call(
                instanceCast, methodInfo, parameterExpressions);

            // ((TInstance)instance).Method((T0)parameters[0], (T1)parameters[1], ...)
            if (methodCall.Type == typeof(void))
            {
                Expression<Action<object, object[]>> lambda =
                    Expression.Lambda<Action<object, object[]>>(
                        methodCall, instanceParameter, parametersParameter);

                Action<object, object[]> execute = lambda.Compile();
                return (instance, parameters) =>
                {
                    execute(instance, parameters);
                    return null;
                };
            }
            else
            {
                UnaryExpression castMethodCall = Expression.Convert(
                    methodCall, typeof(object));
                Expression<Func<object, object[], object>> lambda =
                    Expression.Lambda<Func<object, object[], object>>(
                        castMethodCall, instanceParameter, parametersParameter);

                return lambda.Compile();
            }
        }
    }

    public interface IDynamicPropertyExecutor<T>
    {
        PropertyInfo TargetPropertyInfo { get; }

        T GetValue(object instance);

        void SetValue(object instance, T value);
    }

    //public class DynamicPropertyGroupExecutor<T> : IDynamicPropertyExecutor<T>
    //{
    //    private Func<object, T> m_GetValue;
    //    private Action<object, T> m_SetValue;
    //    private Func<T> m_static_GetValue;
    //    private Action<T> m_static_SetValue;
    //    private bool _isStatic = false;
    //    private PropertyInfo[] _targetPropertyList = null;

    //    public DynamicPropertyGroupExecutor(PropertyInfo[] propertyInfos)
    //    {
    //        _isStatic = propertyInfos[0].IsStaticProperty();

    //        if (_isStatic)
    //        {
    //            m_static_GetValue = GetStaticGetExecutor(propertyInfos);
    //            m_static_SetValue = GetStaticSetExecutor(propertyInfos);
    //        }
    //        else
    //        {
    //            this.m_GetValue = GetGetExecutor(propertyInfos);
    //            this.m_SetValue = GetSetExecutor(propertyInfos);
    //        }


    //        _targetPropertyList = propertyInfos;

    //        //this.m_GetValue = GetGetExecutor(propertyInfos);
    //        //this.m_SetValue = GetSetExecutor(propertyInfos);
    //    }

    //    #region Implementation of IDynamicPropertyExecutor<T>

    //    public PropertyInfo TargetPropertyInfo { get { return _targetPropertyList.Last(); } }

    //    public virtual T GetValue(object instance)
    //    {
    //        if (_isStatic)
    //        {
    //            if (this.m_static_GetValue == null)
    //            {
    //                throw new MethodAccessException("该属性无法读取");
    //            }
    //            else
    //            {
    //                return m_static_GetValue();
    //            }
    //        }
    //        else
    //        {
    //            if (this.m_GetValue == null)
    //            {
    //                throw new MethodAccessException("该属性无法读取");
    //            }

    //            return (T)this.m_GetValue(instance);
    //        }


    //    }

    //    public virtual void SetValue(object instance, T value)
    //    {

    //        if (_isStatic)
    //        {
    //            if (this.m_static_SetValue == null)
    //            {
    //                throw new MethodAccessException("该属性无法设置");
    //            }

    //            m_static_SetValue(value);
    //        }
    //        else
    //        {
    //            if (this.m_SetValue == null)
    //            {
    //                throw new MethodAccessException("该属性无法设置");
    //            }

    //            m_SetValue(instance, value);
    //        }


    //    }

    //    #endregion

    //    public PropertyInfo[] TargetPropertyList { get { return _targetPropertyList; } }

    //    private static Action<object, T> GetSetExecutor(PropertyInfo[] propertyInfos)
    //    {
    //        if (propertyInfos == null || propertyInfos.Length <= 0)
    //        {
    //            return null;
    //        }

    //        for (int i = 0; i < propertyInfos.Length-1; i++)
    //        {
    //            if (!propertyInfos[i].CanRead)
    //            {
    //                return null;
    //            }
    //        }

    //        var lastProperty = propertyInfos.Last();

    //        if (!lastProperty.CanWrite)
    //        {
    //            return null;
    //        }

    //        var setMethod = lastProperty.GetSetMethod();

    //        if (setMethod == null)
    //        {
    //            return null;
    //        }


    //        //(T)( ((TargetType)instance).Property(value) )

    //        //创建参数
    //        var instanceParam = Expression.Parameter(typeof(object), "instance");
    //        var valueParam = Expression.Parameter(typeof(object), "value");

    //        var instanceConvertParam = Expression.Convert(instanceParam, propertyInfos[0].ReflectedType);
    //        var valueConvertParam = Expression.Convert(valueParam, propertyInfos[propertyInfos.Length-1].PropertyType);

    //        //((Target)instance).Property
    //        var propertyExpression = Expression.Property(instanceConvertParam, propertyInfos[0]);

    //        for (int i = 1; i < propertyInfos.Length-1; i++)
    //        {
    //            propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
    //        }

    //        var seterExpression = Expression.Call(propertyExpression, setMethod, valueConvertParam);

    //        var lambdaExpression = Expression.Lambda<Action<object, T>>(seterExpression, instanceParam, valueParam);

    //        return lambdaExpression.Compile();
    //    }

    //    private static Func<object, T> GetGetExecutor(PropertyInfo[] propertyInfos)
    //    {
    //        if (propertyInfos==null || propertyInfos.Length<=0)
    //        {
    //            return null;
    //        }

    //        for (int i = 0; i < propertyInfos.Length; i++)
    //        {
    //            if (!propertyInfos[i].CanRead)
    //            {
    //                return null;
    //            }
    //        }



    //        //(object)( ((TargetType)instance).Property )

    //        UnaryExpression instanceCastExpression = null;
    //        ParameterExpression param = null;

    //        if (!propertyInfos[0].GetGetMethod().IsStatic)
    //        {
    //            //创建参数
    //            param = Expression.Parameter(typeof(object), "instance");
    //            instanceCastExpression = Expression.Convert(param, propertyInfos[0].ReflectedType);
    //        }
            

    //        //(Target)instance
    //        //UnaryExpression instanceCastExpression = Expression.Convert(param, propertyInfos[0].ReflectedType);

    //        //((Target)instance).Property
    //        var propertyExpression = Expression.Property(instanceCastExpression, propertyInfos[0]);
    //        for (int i = 1; i < propertyInfos.Length; i++)
    //        {
    //            propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
    //        }

    //        ////(object)((Target)instance).Property
    //        UnaryExpression castPropertyValue = Expression.Convert(propertyExpression, typeof(T));

    //        Expression<Func<object, T>> propertyLambda = null;

    //        if (param!=null)
    //        {
    //             propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, param);
    //        }
    //        else
    //        {
    //            propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, null);
    //        }

    //        //m_GetValue = propertyLambda.Compile();

    //        return propertyLambda.Compile();
    //    }


    //    private static Action<T> GetStaticSetExecutor(PropertyInfo[] propertyInfos)
    //    {
    //        if (propertyInfos == null || propertyInfos.Length <= 0)
    //        {
    //            return null;
    //        }

    //        for (int i = 0; i < propertyInfos.Length - 1; i++)
    //        {
    //            if (!propertyInfos[i].CanRead)
    //            {
    //                return null;
    //            }
    //        }

    //        var lastProperty = propertyInfos.Last();

    //        if (!lastProperty.CanWrite)
    //        {
    //            return null;
    //        }

    //        var setMethod = lastProperty.GetSetMethod();

    //        if (setMethod == null)
    //        {
    //            return null;
    //        }

    //        if (lastProperty.IsStaticProperty())
    //        {
    //            return DynamicPropertyExecutor<T>.GetStaticSetExecutor(lastProperty);
    //        }


    //        //(T)( ((TargetType)instance).Property(value) )

    //        //创建参数
    //        var valueParam = Expression.Parameter(typeof(object), "value");

    //        var valueConvertParam = Expression.Convert(valueParam, propertyInfos[propertyInfos.Length - 1].PropertyType);

    //        //((Target)instance).Property
    //        var propertyExpression = Expression.Property(null, propertyInfos[0]);

    //        for (int i = 1; i < propertyInfos.Length - 1; i++)
    //        {
    //            propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
    //        }

    //        var seterExpression = Expression.Call(propertyExpression, setMethod, valueConvertParam);

    //        var lambdaExpression = Expression.Lambda<Action<T>>(seterExpression, valueParam);

    //        return lambdaExpression.Compile();




    //        //if (!propertyInfo.CanWrite)
    //        //{
    //        //    return null;
    //        //}

    //        //var setMethod = propertyInfo.GetSetMethod(false);

    //        //if (setMethod == null)
    //        //{
    //        //    return null;
    //        //}

    //        ////(T)( ((TargetType)instance).Property(value) )

    //        ////创建参数
    //        ////var instanceParam = Expression.Parameter(typeof(object), "instance");
    //        //var valueParam = Expression.Parameter(typeof(object), "value");

    //        ////var instanceConvertParam = Expression.Convert(instanceParam, propertyInfo.ReflectedType);
    //        //var valueConvertParam = Expression.Convert(valueParam, propertyInfo.PropertyType);

    //        //var seterExpression = Expression.Call(null, setMethod, valueConvertParam);

    //        //var lambdaExpression = Expression.Lambda<Action<T>>(seterExpression, valueParam);

    //        //return lambdaExpression.Compile();
    //    }

    //    private static Func<T> GetStaticGetExecutor(PropertyInfo[] propertyInfos)
    //    {
    //        if (propertyInfos == null || propertyInfos.Length <= 0)
    //        {
    //            return null;
    //        }

    //        for (int i = 0; i < propertyInfos.Length; i++)
    //        {
    //            if (!propertyInfos[i].CanRead)
    //            {
    //                return null;
    //            }
    //        }

    //        if (propertyInfos.Last().IsStaticProperty())
    //        {
    //            return DynamicPropertyExecutor<T>.GetStaticGetExecutor(propertyInfos.Last());
    //        }

    //        //T.Property
    //        var propertyExpression = Expression.Property(null, propertyInfos[0]);
    //        for (int i = 1; i < propertyInfos.Length; i++)
    //        {
    //            propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
    //        }

    //        ////(object)(T).Property
    //        UnaryExpression castPropertyValue = Expression.Convert(propertyExpression, typeof(T));

    //        var propertyLambda = Expression.Lambda<Func<T>>(castPropertyValue, null);

    //        return propertyLambda.Compile();
    //    }


    //}

    /// <summary>
    ///     基于lambda表达式构建的属性访问器
    /// </summary>
    /// <typeparam name="T">返回或设置的值的类型</typeparam>
    public class DynamicPropertyExecutor<T> : IDynamicPropertyExecutor<T>
    {
        private Func<object,T > m_GetValue;
        private Action<object,T > m_SetValue;
        private Func<T> m_static_GetValue;
        private Action<T> m_static_SetValue;
        private bool _isStatic = false;

        public DynamicPropertyExecutor(PropertyInfo propertyInfo)
        {
            _isStatic = propertyInfo.IsStaticProperty();

            if (_isStatic)
            {
                m_static_GetValue = DynamicMemberExecutorBuilder.GetStaticGetExecutor<T>(propertyInfo);
                m_static_SetValue = DynamicMemberExecutorBuilder.GetStaticSetExecutor<T>(propertyInfo);
            }
            else
            {
                this.m_GetValue = DynamicMemberExecutorBuilder.GetGetExecutor<T>(propertyInfo);
                this.m_SetValue = DynamicMemberExecutorBuilder.GetSetExecutor<T>(propertyInfo);
            }
            
            //this.m_SetValue = GetSetExecutor(propertyInfo);

            //m_static_GetValue = GetStaticGetExecutor(propertyInfo);

            TargetPropertyInfo = propertyInfo;
        }

        public DynamicPropertyExecutor(PropertyInfo[] propertyInfos)
        {
            _isStatic = propertyInfos[0].IsStaticProperty();

            if (_isStatic)
            {
                m_static_GetValue = DynamicMemberExecutorBuilder.GetStaticGetExecutor<T>(propertyInfos);
                m_static_SetValue = DynamicMemberExecutorBuilder.GetStaticSetExecutor<T>(propertyInfos);
            }
            else
            {
                this.m_GetValue = DynamicMemberExecutorBuilder.GetGetExecutor<T>(propertyInfos);
                this.m_SetValue = DynamicMemberExecutorBuilder.GetSetExecutor<T>(propertyInfos);
            }


            TargetPropertyInfo = propertyInfos.Last();
        }

        #region Implementation of IDynamicPropertyExecutor<T>

        public PropertyInfo TargetPropertyInfo { private set; get; }

        public virtual T GetValue(object instance)
        {
            if (_isStatic)
            {
                if (this.m_static_GetValue==null)
                {
                    throw new MethodAccessException("该属性无法读取");
                }
                else
                {
                    return m_static_GetValue();
                }
            }
            else
            {
                if (this.m_GetValue==null)
                {
                    throw new MethodAccessException("该属性无法读取");
                }

                return (T)this.m_GetValue(instance);                
            }


        }

        public virtual void SetValue(object instance, T value)
        {

            if (_isStatic)
            {
                if (this.m_static_SetValue == null)
                {
                    throw new MethodAccessException("该属性无法设置");
                }

                m_static_SetValue(value);
            }
            else
            {
                if (this.m_SetValue == null)
                {
                    throw new MethodAccessException("该属性无法设置");
                }



                m_SetValue(instance,(T)value);
            }


        }

        #endregion

        public bool IsStaticProperty
        {
            get { return _isStatic; }
        }


    }



    public static class DynamicMemberExecutorBuilder
    {
        #region "构建单个属性的执行器"

        public static Action<object, T> GetSetExecutor<T>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            var setMethod = propertyInfo.GetSetMethod(false);

            if (setMethod == null)
            {
                return null;
            }

            //(T)( ((TargetType)instance).Property(value) )

            //创建参数
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");

            var instanceConvertParam = Expression.Convert(instanceParam, propertyInfo.ReflectedType);
            var valueConvertParam = Expression.Convert(valueParam, propertyInfo.PropertyType);

            var seterExpression = Expression.Call(instanceConvertParam, setMethod, valueConvertParam);

            var lambdaExpression = Expression.Lambda<Action<object, T>>(seterExpression, instanceParam, valueParam);

            return lambdaExpression.Compile();
        }

        public static Func<object, T> GetGetExecutor<T>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return null;
            }



            //(object)( ((TargetType)instance).Property )

            //创建参数
            ParameterExpression param = null;

            //(Target)instance
            UnaryExpression instanceCastExpression = null;// = Expression.Convert(param, propertyInfo.ReflectedType);


            param = Expression.Parameter(typeof(object), "instance");
            instanceCastExpression = Expression.Convert(param, propertyInfo.ReflectedType);



            //((Target)instance).Property
            var propertyExpression = Expression.Property(instanceCastExpression, propertyInfo);

            ////(object)((Target)instance).Property
            UnaryExpression castPropertyValue = Expression.Convert(propertyExpression, typeof(T));

            Expression<Func<object, T>> propertyLambda = null;

            propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, param);


            //var propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, param);

            //m_GetValue = propertyLambda.Compile();

            return propertyLambda.Compile();
        }

        public static Action<T> GetStaticSetExecutor<T>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanWrite)
            {
                return null;
            }

            var setMethod = propertyInfo.GetSetMethod(false);

            if (setMethod == null)
            {
                return null;
            }

            //(T)( ((TargetType)instance).Property(value) )

            //创建参数
            //var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");

            //var instanceConvertParam = Expression.Convert(instanceParam, propertyInfo.ReflectedType);
            var valueConvertParam = Expression.Convert(valueParam, propertyInfo.PropertyType);

            var seterExpression = Expression.Call(null, setMethod, valueConvertParam);

            var lambdaExpression = Expression.Lambda<Action<T>>(seterExpression, valueParam);

            return lambdaExpression.Compile();
        }

        public static Func<T> GetStaticGetExecutor<T>(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                return null;
            }

            //(object)( ((TargetType)instance).Property )

            //((Target)instance).Property
            var propertyExpression = Expression.Property(null, propertyInfo);

            ////(object)((Target)instance).Property
            UnaryExpression castPropertyValue = Expression.Convert(propertyExpression, typeof(T));

            Expression<Func<T>> propertyLambda = null;


            propertyLambda = Expression.Lambda<Func<T>>(castPropertyValue, null);


            //var propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, param);

            //m_GetValue = propertyLambda.Compile();

            return propertyLambda.Compile();
        }

        #endregion


        #region "同时构建一连串属性的执行器"

        public static Action<object, T> GetSetExecutor<T>(PropertyInfo[] propertyInfos)
        {
            if (propertyInfos == null || propertyInfos.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < propertyInfos.Length - 1; i++)
            {
                if (!propertyInfos[i].CanRead)
                {
                    return null;
                }
            }

            var lastProperty = propertyInfos.Last();

            if (!lastProperty.CanWrite)
            {
                return null;
            }

            var setMethod = lastProperty.GetSetMethod();

            if (setMethod == null)
            {
                return null;
            }


            //(T)( ((TargetType)instance).Property(value) )

            //创建参数
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var valueParam = Expression.Parameter(typeof(object), "value");

            var instanceConvertParam = Expression.Convert(instanceParam, propertyInfos[0].ReflectedType);
            var valueConvertParam = Expression.Convert(valueParam, propertyInfos[propertyInfos.Length - 1].PropertyType);

            //((Target)instance).Property
            var propertyExpression = Expression.Property(instanceConvertParam, propertyInfos[0]);

            for (int i = 1; i < propertyInfos.Length - 1; i++)
            {
                propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
            }

            var seterExpression = Expression.Call(propertyExpression, setMethod, valueConvertParam);

            var lambdaExpression = Expression.Lambda<Action<object, T>>(seterExpression, instanceParam, valueParam);

            return lambdaExpression.Compile();
        }

        public static Func<object, T> GetGetExecutor<T>(PropertyInfo[] propertyInfos)
        {
            if (propertyInfos == null || propertyInfos.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (!propertyInfos[i].CanRead)
                {
                    return null;
                }
            }



            //(object)( ((TargetType)instance).Property )

            UnaryExpression instanceCastExpression = null;
            ParameterExpression param = null;

            if (!propertyInfos[0].GetGetMethod().IsStatic)
            {
                //创建参数
                param = Expression.Parameter(typeof(object), "instance");
                instanceCastExpression = Expression.Convert(param, propertyInfos[0].ReflectedType);
            }


            //(Target)instance
            //UnaryExpression instanceCastExpression = Expression.Convert(param, propertyInfos[0].ReflectedType);

            //((Target)instance).Property
            var propertyExpression = Expression.Property(instanceCastExpression, propertyInfos[0]);
            for (int i = 1; i < propertyInfos.Length; i++)
            {
                propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
            }

            ////(object)((Target)instance).Property
            UnaryExpression castPropertyValue = Expression.Convert(propertyExpression, typeof(T));

            Expression<Func<object, T>> propertyLambda = null;

            if (param != null)
            {
                propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, param);
            }
            else
            {
                propertyLambda = Expression.Lambda<Func<object, T>>(castPropertyValue, null);
            }

            //m_GetValue = propertyLambda.Compile();

            return propertyLambda.Compile();
        }


        public static Action<T> GetStaticSetExecutor<T>(PropertyInfo[] propertyInfos)
        {
            if (propertyInfos == null || propertyInfos.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < propertyInfos.Length - 1; i++)
            {
                if (!propertyInfos[i].CanRead)
                {
                    return null;
                }
            }

            var lastProperty = propertyInfos.Last();

            if (!lastProperty.CanWrite)
            {
                return null;
            }

            var setMethod = lastProperty.GetSetMethod();

            if (setMethod == null)
            {
                return null;
            }

            if (lastProperty.IsStaticProperty())
            {
                return DynamicMemberExecutorBuilder.GetStaticSetExecutor<T>(lastProperty);
            }


            //(T)( ((TargetType)instance).Property(value) )

            //创建参数
            var valueParam = Expression.Parameter(typeof(object), "value");

            var valueConvertParam = Expression.Convert(valueParam, propertyInfos[propertyInfos.Length - 1].PropertyType);

            //((Target)instance).Property
            var propertyExpression = Expression.Property(null, propertyInfos[0]);

            for (int i = 1; i < propertyInfos.Length - 1; i++)
            {
                propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
            }

            var seterExpression = Expression.Call(propertyExpression, setMethod, valueConvertParam);

            var lambdaExpression = Expression.Lambda<Action<T>>(seterExpression, valueParam);

            return lambdaExpression.Compile();




            //if (!propertyInfo.CanWrite)
            //{
            //    return null;
            //}

            //var setMethod = propertyInfo.GetSetMethod(false);

            //if (setMethod == null)
            //{
            //    return null;
            //}

            ////(T)( ((TargetType)instance).Property(value) )

            ////创建参数
            ////var instanceParam = Expression.Parameter(typeof(object), "instance");
            //var valueParam = Expression.Parameter(typeof(object), "value");

            ////var instanceConvertParam = Expression.Convert(instanceParam, propertyInfo.ReflectedType);
            //var valueConvertParam = Expression.Convert(valueParam, propertyInfo.PropertyType);

            //var seterExpression = Expression.Call(null, setMethod, valueConvertParam);

            //var lambdaExpression = Expression.Lambda<Action<T>>(seterExpression, valueParam);

            //return lambdaExpression.Compile();
        }

        public static Func<T> GetStaticGetExecutor<T>(PropertyInfo[] propertyInfos)
        {
            if (propertyInfos == null || propertyInfos.Length <= 0)
            {
                return null;
            }

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (!propertyInfos[i].CanRead)
                {
                    return null;
                }
            }

            if (propertyInfos.Last().IsStaticProperty())
            {
                return DynamicMemberExecutorBuilder.GetStaticGetExecutor<T>(propertyInfos.Last());
            }

            //T.Property
            var propertyExpression = Expression.Property(null, propertyInfos[0]);
            for (int i = 1; i < propertyInfos.Length; i++)
            {
                propertyExpression = Expression.Property(propertyExpression, propertyInfos[i]);
            }

            ////(object)(T).Property
            UnaryExpression castPropertyValue = Expression.Convert(propertyExpression, typeof(T));

            var propertyLambda = Expression.Lambda<Func<T>>(castPropertyValue, null);

            return propertyLambda.Compile();
        }

        #endregion
    }
}
