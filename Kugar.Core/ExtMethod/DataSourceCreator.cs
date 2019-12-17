using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;


namespace Kugar.Core.ExtMethod
{
    /// <summary>
    ///     引用自:http://www.simple-talk.com/dotnet/.net-framework/dynamically-generating--typed-objects-in-.net/
    ///     从IDictionary中,创建可用于绑定的动态类型
    /// </summary>
    public static class DataSourceCreator
    {
        private static readonly Regex PropertNameRegex =
                new Regex(@"^[A-Za-z]+[A-Za-z0-9_]*$", RegexOptions.Singleline);

        //用于缓冲类型
        private static readonly Dictionary<string, Type> _typeBySigniture = new Dictionary<string, Type>();

        #region "IDictionary To DataSource"

        public static object ToDataSource(this IDictionary dic)
        {
            var temp = ToDataSource(new[] { dic });

            if (temp != null)
            {
                return null;
            }
            else
            {
                object obj = null;

                foreach (var v in temp)
                {
                    if (v != null)
                    {
                        obj = v;
                    }
                }

                return obj;
            }
        }

        /// <summary>
        ///     从IDictionary中构建类型,用于作为属性名的Key值,必须为英文字母加数字
        ///     要求可枚举列表中,第一个IDictionary类不能为null,否则会抛出错误,生成的类的属性个数由可枚举列表中第一个IDictionary决定
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable ToDataSource(this IEnumerable<IDictionary> list)
        {
            IDictionary firstDict = null;
            bool hasData = false;
            foreach (IDictionary currentDict in list)
            {
                hasData = true;
                firstDict = currentDict;
                break;
            }
            if (!hasData)
            {
                return new object[] { };
            }
            if (firstDict == null)
            {
                throw new ArgumentException("IDictionary entry cannot be null");
            }

            string typeSigniture = GetTypeSigniture(firstDict);

            Type objectType = GetTypeByTypeSigniture(typeSigniture);


            if (objectType == null)
            {
                TypeBuilder tb = GetTypeBuilder(list.GetHashCode());

                ConstructorBuilder constructor =
                            tb.DefineDefaultConstructor(
                                        MethodAttributes.Public |
                                        MethodAttributes.SpecialName |
                                        MethodAttributes.RTSpecialName);

                foreach (DictionaryEntry pair in firstDict)
                {
                    if (PropertNameRegex.IsMatch(Convert.ToString(pair.Key), 0))
                    {
                        CreateProperty(tb,
                                        Convert.ToString(pair.Key),
                                        pair.Value == null ?
                                                    typeof(object) :
                                                    pair.Value.GetType());
                    }
                    else
                    {
                        throw new ArgumentException(
                                    @"Each key of IDictionary must be 
                                alphanumeric and start with character.");
                    }
                }
                objectType = tb.CreateType();

                _typeBySigniture.Add(typeSigniture, objectType);
            }
            return GenerateEnumerable(objectType, list, firstDict);
        }

        private static IEnumerable GenerateEnumerable(
                 Type objectType, IEnumerable<IDictionary> list, IDictionary firstDict)
        {
            var listType = typeof(List<>).MakeGenericType(new[] { objectType });
            var listOfCustom = Activator.CreateInstance(listType);

            foreach (var currentDict in list)
            {
                if (currentDict == null)
                {
                    throw new ArgumentException("IDictionary entry cannot be null");
                }
                var row = Activator.CreateInstance(objectType);
                foreach (DictionaryEntry pair in firstDict)
                {
                    if (currentDict.Contains(pair.Key))
                    {
                        PropertyInfo property =
                            objectType.GetProperty(Convert.ToString(pair.Key));
                        property.SetValue(
                            row,
                            Convert.ChangeType(
                                    currentDict[pair.Key],
                                    property.PropertyType,
                                    null),
                            null);
                    }
                }
                listType.GetMethod("Add").Invoke(listOfCustom, new[] { row });
            }
            return listOfCustom as IEnumerable;
        }

        private static string GetTypeSigniture(IDictionary firstDict)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DictionaryEntry pair in firstDict)
            {
                sb.AppendFormat("_{0}_{1}", pair.Key, GetValueType(pair.Value));
            }
            return sb.ToString().GetHashCode().ToString().Replace("-", "Minus");
        }


        #endregion

        internal static TypeBuilder GetTypeBuilder(int code)
        {
            AssemblyName an = new AssemblyName("TempAssembly" + code);
            AssemblyBuilder assemblyBuilder = null;

#if NET45
      assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(
                    an, AssemblyBuilderAccess.Run);
#endif
#if NETCOREAPP2_0
            assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
#endif


                  ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");

            TypeBuilder tb = moduleBuilder.DefineType("TempType" + code
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , typeof(object));
            return tb;
        }

        internal static void CreateProperty(
                        TypeBuilder tb, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName,
                                                        propertyType,
                                                        FieldAttributes.Private);


            PropertyBuilder propertyBuilder =
                tb.DefineProperty(
                    propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr =
                tb.DefineMethod("get_" + propertyName,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName |
                    MethodAttributes.HideBySig,
                    propertyType, Type.EmptyTypes);

            ILGenerator getIL = getPropMthdBldr.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr =
                tb.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
                  null, new Type[] { propertyType });

            ILGenerator setIL = setPropMthdBldr.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }


        internal static Type GetTypeByTypeSigniture(string typeSigniture)
        {
            Type type;
            return _typeBySigniture.TryGetValue(typeSigniture, out type) ? type : null;
        }


        internal static Type GetValueType(object value)
        {
            return value == null ? typeof(object) : value.GetType();
        }


    }
}
