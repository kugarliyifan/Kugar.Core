using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LinqKit;

namespace Kugar.Core.ExtMethod
{
    public static class ExpressionExtMethod
    {
        public static string GetPropertyName<T, TResult>(this Expression<Func<T, TResult>> expression)
        {
            string propertyName = string.Empty;    //返回的属性名
            //对象是不是一元运算符


            if (expression.Body is UnaryExpression)
            {
                propertyName = ((MemberExpression)((UnaryExpression)expression.Body).Operand).Member.Name;
            }
            //对象是不是访问的字段或属性
            else if (expression.Body is MemberExpression)
            {
                propertyName = ((MemberExpression)expression.Body).Member.Name;
            }
            //对象是不是参数表达式
            else if (expression.Body is ParameterExpression)
            {
                propertyName = ((ParameterExpression)expression.Body).Type.Name;  
            }

            return propertyName;
        }

        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> exp, Func<bool> check,
    Expression<Func<T, bool>> expr2)
        {
            if (check())
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.And(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }

        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> exp, Func<bool> check,
            Expression<Func<T, bool>> expr2)
        {
            if (check())
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.Or(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }

        public static Expression<Func<T, bool>> AndIf<T>(this ExpressionStarter<T> exp, Func<bool> check,
            Expression<Func<T, bool>> expr2)
        {
            if (check())
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.And(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }

        public static Expression<Func<T, bool>> OrIf<T>(this ExpressionStarter<T> exp, Func<bool> check,
            Expression<Func<T, bool>> expr2)
        {
            if (check())
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.Or(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }



        public static Expression<Func<T, bool>> AndIf<T>(this Expression<Func<T, bool>> exp, bool check,
            Expression<Func<T, bool>> expr2)
        {
            if (check)
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.And(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }

        public static Expression<Func<T, bool>> OrIf<T>(this Expression<Func<T, bool>> exp, bool check,
            Expression<Func<T, bool>> expr2)
        {
            if (check)
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.Or(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }

        public static Expression<Func<T, bool>> AndIf<T>(this ExpressionStarter<T> exp, bool check,
            Expression<Func<T, bool>> expr2)
        {
            if (check)
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.And(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }

        public static Expression<Func<T, bool>> OrIf<T>(this ExpressionStarter<T> exp, bool check,
            Expression<Func<T, bool>> expr2)
        {
            if (check)
            {
                if (exp == null)
                {
                    return expr2;
                }
                else if (expr2 == null)
                {
                    return exp;
                }
                else
                {
                    return LinqKit.PredicateBuilder.Or(exp, expr2);
                }
            }
            else
            {
                return exp;
            }
        }
    }
}
