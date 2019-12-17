using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Kugar.Core.ExtMethod;
using LinqKit;

namespace Kugar.Core.Linq
{
    /// <summary>
    /// Linq生成的动态查询条件表达式
    /// </summary>
    public class DynamicLinqQueryBuilder
    {
        internal static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });

        /// <summary>
        /// 构建动态查询表达式
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="beforceQuery"></param>
        /// <param name="queryLst">查询表达式列表</param>
        /// <returns></returns>
        public static Expression<Func<TModel, bool>> Build<TModel>(Expression<Func<TModel, bool>> beforceQuery, params DynamicLinqQuery[] queryLst) where TModel : class
        {
            if (queryLst.HasData())
            {
                if (beforceQuery == null)
                {
                    beforceQuery = PredicateBuilder.New<TModel>(true);//ExpressionOptHelper.True<TModel>();
                }

                foreach (var query in queryLst)
                {
                    beforceQuery = PredicateBuilder.And(beforceQuery, Build<TModel>(query));
                }

                return beforceQuery;
            }
            else
            {
                return beforceQuery;
            }
        }


        /// <summary>
        /// 构建动态查询表达式
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="beforceQuery"></param>
        /// <param name="queryLst">查询表达式列表</param>
        /// <returns></returns>
        public static IQueryable<TModel> Build<TModel>(IQueryable<TModel> collectionQuery, params DynamicLinqQuery[] queryLst) where TModel : class
        {
            if (queryLst.HasData())
            {
                var q = PredicateBuilder.New<TModel>(true);//ExpressionOptHelper.True<TModel>();

                foreach (var query in queryLst)
                {
                    q = q.And(Build<TModel>(query));// ExpressionOptHelper.And(q, );
                }

                return collectionQuery.Where(q);
            }
            else
            {
                return collectionQuery;
            }
        }

        /// <summary>
        /// 构建动态查询表达式
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="query">单个查询表达式</param>
        /// <returns></returns>
        public static Expression<Func<TModel, bool>> Build<TModel>(DynamicLinqQuery query) where TModel : class
        {
            var value = query.Value;

            if (value == null)
            {
                return null;
            }

            var instantExp = Expression.Parameter(typeof(TModel), "x");

            var propertyExp = buildPropertyExp(instantExp, query.ArgName);// Expression.Property(instantExp, query.ArgName);

            if (propertyExp.Member.MemberType != MemberTypes.Property)
            {
                return null;
            }

            var prop = (PropertyInfo)propertyExp.Member;

            var propertyValueType = prop.PropertyType;

            Expression<Func<TModel, bool>> queryExp = null;

            if (propertyValueType == typeof(string) && !string.IsNullOrWhiteSpace(value.ToStringEx()))
            {
                var tmp = Expression.Call(propertyExp, containsMethod, Expression.Constant(value));

                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            else if ((propertyValueType == typeof(DateTime) || propertyValueType == typeof(DateTime?)) &&
                     (value is DateTime? || value is DateTime || IsDate(value.ToStringEx())))
            {
                var valueExp = convertToDate(propertyValueType, value);
                Expression tmp = buildDateOrNumericExp(propertyExp, valueExp, query.OptType);
                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            else if (propertyValueType.IsNumericType() || value.ToStringEx().IsNumeric())
            {
                var valueExp = convertToNumeric(propertyValueType, value);
                Expression tmp = buildDateOrNumericExp(propertyExp, valueExp, query.OptType);
                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            else if ((propertyValueType == typeof(bool) || propertyValueType == typeof(bool?)) & (value is bool || string.IsNullOrWhiteSpace(value.ToStringEx())))
            {
                Expression tmp = buildDateOrNumericExp(propertyExp, Expression.Constant(value.ToBoolNullable(), propertyValueType), query.OptType);
                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            return queryExp;
        }

        internal static MemberExpression buildPropertyExp(ParameterExpression instantExp, string propertyList)
        {
            var properties = propertyList.Split('.');

            Type t = instantExp.Type;
            ParameterExpression parameter = instantExp;
            Expression expression = parameter;

            for (int i = 0; i < properties.Count(); i++)
            {
                expression = Expression.Property(expression, t, properties.ElementAt(i));
                t = expression.Type;
            }

            // var lambdaExpression = Expression.Lambda(expression, parameter);

            return (MemberExpression)expression;
        }

        internal static bool IsDate(string strSource)
        {
            bool bValid = Regex.IsMatch(strSource, @"^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-))$");
            bool bValid1 = Regex.IsMatch(strSource, @"^((((1[6-9]|[2-9]\d)\d{2}).(0?[13578]|1[02]).(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2}).(0?[13456789]|1[012]).(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2}).0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00)).0?2-29-))$");
            return ((bValid1 || bValid) && strSource.CompareTo("1753-01-01") >= 0);
        }

        internal static Expression convertToDate(Type descType, object value)
        {
            if (value is DateTime?)
            {
                return Expression.Constant((DateTime?)value);
            }
            else
            {
                var v = value.ToStringEx();

                if (descType == typeof(DateTime?))
                {
                    var dt = v.ToDateTimeNullable("yyyy-MM-dd");

                    return Expression.Constant(dt);
                }
                else
                {
                    var dt = v.ToDateTime("yyyy-MM-dd");

                    return Expression.Constant(dt);
                }
            }
        }

        internal static Expression buildDateOrNumericExp(Expression propertyExp, Expression valueExp, DynamicLinqOptTypeEnum opt)
        {
            valueExp = Expression.Convert(valueExp, propertyExp.Type);

            Expression tmp = null;
            switch (opt)
            {
                case DynamicLinqOptTypeEnum.Like:
                case DynamicLinqOptTypeEnum.Equal:
                    tmp = Expression.Equal(propertyExp, valueExp);
                    break;
                case DynamicLinqOptTypeEnum.GT:
                    tmp = Expression.GreaterThan(propertyExp, valueExp);
                    break;
                case DynamicLinqOptTypeEnum.GTE:
                    tmp = Expression.GreaterThanOrEqual(propertyExp, valueExp);
                    break;
                case DynamicLinqOptTypeEnum.LT:
                    tmp = Expression.LessThan(propertyExp, valueExp);
                    break;
                case DynamicLinqOptTypeEnum.LTE:
                    tmp = Expression.LessThanOrEqual(propertyExp, valueExp);
                    break;
            }

            return tmp;
        }

        internal static Expression convertToNumeric(Type descType, object value)
        {

            if (descType == typeof(int) || descType == typeof(int?))
            {
                return Expression.Constant(value.ToIntNullable(), descType);
            }
            else if (descType == typeof(decimal) || descType == typeof(decimal?))
            {
                return Expression.Constant(value.ToDecimalNullable(), descType);
            }
            else if (descType == typeof(double) || descType == typeof(double?))
            {
                return Expression.Constant(value.ToDoubleNullable(), descType);
            }
            else if (descType == typeof(float) || descType == typeof(float?))
            {
                return Expression.Constant(value.ToFloatNullable(), descType);
            }
            else if (descType == typeof(long) || descType == typeof(long?))
            {
                return Expression.Constant(value.ToLong(), descType);
            }

            return null;

            //if (value is int?)
            //{
            //    return Expression.Constant((DateTime?)value);
            //}
            //else if (value is DateTime)
            //{
            //    return Expression.Constant((DateTime)value);
            //}
            //else
            //{
            //    var v = value.ToStringEx();

            //    if (descType == typeof(DateTime?))
            //    {
            //        var dt = v.ToDateTimeNullable("yyyy-MM-dd");

            //        return Expression.Constant(dt);
            //    }
            //    else
            //    {
            //        var dt = v.ToDateTime("yyyy-MM-dd");

            //        return Expression.Constant(dt);
            //    }
            //}
        }
    }

    public static class DynamicLinqQueryBuilderExtMethod
    {
        public static IQueryable<IGrouping<TKey, TModel>> Where<TModel, TKey, TResult>(this IQueryable<IGrouping<TKey, TModel>> groupQuery,
    Expression<Func<IGrouping<TKey, TModel>, TResult>> exp, DynamicLinqQuery query) where TModel : class
        {
            var value = query.Value;

            if (value == null)
            {
                return null;
            }

            var instantExp = Expression.Parameter(typeof(IGrouping<TKey, TModel>), "x");

            var propertyExp = Expression.Invoke(exp, instantExp);//Expression.Lambda(.Body, );// Expression.Lambda<Func<IGrouping<TKey, TModel>,TResult>>(exp,);// buildPropertyExp(instantExp, query.ArgName);// Expression.Property(instantExp, query.ArgName);

            //if (propertyExp.Member.MemberType != MemberTypes.Property)
            //{
            //    return null;
            //}

            //var prop = (PropertyInfo)propertyExp.Member;

            //var propertyValueType = typeof(TResult);// prop.PropertyType;

            var exp1 = buildExpression<IGrouping<TKey, TModel>>(propertyExp, instantExp, query);

            return groupQuery.Where(exp1);
        }
        
        public static IQueryable<TModel> Where<TModel, TResult>(IQueryable<TModel> queryExpression,
            Expression<Func<TModel, TResult>> exp, DynamicLinqQuery query) where TModel : class
        {
            var value = query.Value;

            if (value == null)
            {
                return null;
            }

            var instantExp = Expression.Parameter(typeof(TModel), "x");

            var propertyExp = Expression.Invoke(exp, instantExp);//Expression.Lambda(.Body, );// Expression.Lambda<Func<IGrouping<TKey, TModel>,TResult>>(exp,);// buildPropertyExp(instantExp, query.ArgName);// Expression.Property(instantExp, query.ArgName);

            //if (propertyExp.Member.MemberType != MemberTypes.Property)
            //{
            //    return null;
            //}

            //var prop = (PropertyInfo)propertyExp.Member;

            //var propertyValueType = typeof(TResult);// prop.PropertyType;

            var exp1 = buildExpression<TModel>(propertyExp, instantExp, query);

            return queryExpression.Where(exp1);
        }

        public static IQueryable<IGrouping<TKey, TModel>> WhereIf<TModel, TKey, TResult>(
            this IQueryable<IGrouping<TKey, TModel>> groupQuery, bool check,
            Expression<Func<IGrouping<TKey, TModel>, TResult>> exp, DynamicLinqQuery query) where TModel : class
        {
            if (check)
            {
                return Where(groupQuery, exp, query);
            }
            else
            {
                return groupQuery;
            }

        }

        public static IQueryable<TModel> WhereIf<TModel, TResult>(
            this IQueryable<TModel> groupQuery, bool check,
            Expression<Func<TModel, TResult>> exp, DynamicLinqQuery query) where TModel : class
        {
            if (check)
            {
                return Where(groupQuery, exp, query);
            }
            else
            {
                return groupQuery;
            }

        }

        private static Expression<Func<TModel, bool>> buildExpression<TModel>(Expression propertyExp, ParameterExpression instantExp, DynamicLinqQuery query)
        {
            var value = query.Value;
            var propertyValueType = propertyExp.Type;

            Expression<Func<TModel, bool>> queryExp = null;

            if (propertyValueType == typeof(string) && !String.IsNullOrWhiteSpace(value.ToStringEx()))
            {
                var tmp = Expression.Call(propertyExp, DynamicLinqQueryBuilder.containsMethod, Expression.Constant(value));

                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            else if ((propertyValueType == typeof(DateTime) || propertyValueType == typeof(DateTime?)) &&
                     (value is DateTime? || value is DateTime || DynamicLinqQueryBuilder.IsDate(value.ToStringEx())))
            {
                var valueExp = DynamicLinqQueryBuilder.convertToDate(propertyValueType, value);
                Expression tmp = DynamicLinqQueryBuilder.buildDateOrNumericExp(propertyExp, valueExp, query.OptType);
                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            else if (propertyValueType.IsNumericType() || value.ToStringEx().IsNumeric())
            {
                var valueExp = DynamicLinqQueryBuilder.convertToNumeric(propertyValueType, value);
                Expression tmp = DynamicLinqQueryBuilder.buildDateOrNumericExp(propertyExp, valueExp, query.OptType);
                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }
            else if ((propertyValueType == typeof(bool) || propertyValueType == typeof(bool?)) & (value is bool || String.IsNullOrWhiteSpace(value.ToStringEx())))
            {
                Expression tmp = DynamicLinqQueryBuilder.buildDateOrNumericExp(propertyExp, Expression.Constant(value.ToBoolNullable(), propertyValueType), query.OptType);
                queryExp = Expression.Lambda<Func<TModel, bool>>(tmp, instantExp);
            }

            return queryExp;
        }

    }

    /// <summary>
    /// 动态查询条件语句
    /// </summary>
    public class DynamicLinqQuery
    {
        public string QueryName { set; get; }

        /// <summary>
        /// 可以是级联多个属性比如 order.Birthday.Data
        /// </summary>
        public string ArgName { set; get; }

        /// <summary>
        /// 操作符
        /// </summary>
        public DynamicLinqOptTypeEnum OptType { set; get; }

        /// <summary>
        /// 操作数据值
        /// </summary>
        public object Value { set; get; }
    }

    /// <summary>
    /// 表达式判断操作类型
    /// </summary>
    public enum DynamicLinqOptTypeEnum
    {
        /// <summary>
        /// 对应SQL的like操作
        /// </summary>
        Like = 0,

        /// <summary>
        /// 判断相等
        /// </summary>
        Equal,

        /// <summary>
        /// 大于
        /// </summary>
        GT,

        /// <summary>
        /// 大于或等于
        /// </summary>
        GTE,

        /// <summary>
        /// 小于
        /// </summary>
        LT,

        /// <summary>
        /// /小于或等于
        /// </summary>
        LTE
    }
}
