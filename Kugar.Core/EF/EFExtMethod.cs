using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using Kugar.Core.Exceptions;

namespace Kugar.Core.ExtMethod
{
    public static class EFExtMethod
    {
        public static DataTable SqlQueryEx(this DbContext context, string sql, params DbParameter[] args)
        {
            return SqlQueryEx(context, sql, null, args);
        }

        /// <summary>
        /// 执行一个sql语句,并返回DataTable
        /// </summary>
        /// <param name="context"></param>
        /// <param name="sql"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static DataTable SqlQueryEx(this DbContext context, string sql, DbTransaction transaction , params DbParameter[] args)
        {
            var conn = context.Database.Connection;

            var state = conn.State;

            if (state != ConnectionState.Open)
            {
                conn.Open();
            }

            var cmd = conn.CreateCommand();

            cmd.CommandText = sql;
            cmd.Parameters.AddRange(args);
            var dt = new DataTable();

            using (cmd)
            {
                cmd.Connection = conn;

                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }
            

            if (state!=ConnectionState.Open)
            {
                conn.Close();
            }

            return dt;
        }

        public static async Task<DataTable> SqlQueryExAsync(this DbContext context, string sql, params DbParameter[] args)
        {
            return await SqlQueryExAsync(context, sql, null, args);
        }

        public static async Task<DataTable> SqlQueryExAsync(this DbContext context, string sql, DbTransaction transaction, params DbParameter[] args)
        {
            var conn = context.Database.Connection;

            var state = conn.State;

            if (state != ConnectionState.Open)
            {
                conn.Open();
            }

            var cmd = conn.CreateCommand();

            cmd.CommandText = sql;
            cmd.Parameters.AddRange(args);
            var dt = new DataTable();

            using (cmd)
            {
                cmd.Connection = conn;

                using (var reader =await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
            }


            if (state != ConnectionState.Open)
            {
                conn.Close();
            }

            return dt;
        }

        /// <summary>
        /// 用于将EF的查询表达式执行并返回DataTable类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IQueryable<T> query,DbTransaction transaction=null)
        {
            var dbQuery = (DbQuery<T>)query;

            if (dbQuery==null)
            {
                throw new ArgumentTypeNotMatchException(nameof(query), "DbQuery<T>");
            }
            
            var objQuery = (ObjectQuery<T>)dbQuery.GetPropertyValue("InternalQuery").GetPropertyValue("ObjectQuery");

            var cmd=objQuery.Context.Connection.CreateCommand();//.CreateParameter()
            
            foreach (var parameter in objQuery.Parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = parameter.Name;
                p.Value = parameter.Value;
                p.DbType = parameter.ParameterType.GetDbParameterType();
                
                cmd.Parameters.Add(p);
            }

            cmd.CommandText = objQuery.CommandText;
     
            var dt = new DataTable();

            using (cmd)
            {
                cmd.Connection = objQuery.Context.Connection;
                cmd.Transaction = transaction;
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }

        /// <summary>
        /// 用于将EF的查询表达式执行并返回DataTable类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public static async Task<DataTable> ToDataTableAsync<T>(this IQueryable<T> query, DbTransaction transaction = null)
        {
            var dbQuery = (DbQuery<T>)query;

            if (dbQuery == null)
            {
                throw new ArgumentTypeNotMatchException(nameof(query), "DbQuery<T>");
            }

            var objQuery = (ObjectQuery<T>)dbQuery.GetPropertyValue("InternalQuery").GetPropertyValue("ObjectQuery");

            var cmd = objQuery.Context.Connection.CreateCommand();//.CreateParameter()

            foreach (var parameter in objQuery.Parameters)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = parameter.Name;
                p.Value = parameter.Value;
                p.DbType = parameter.ParameterType.GetDbParameterType();

                cmd.Parameters.Add(p);
            }

            cmd.CommandText = objQuery.CommandText;

            var dt = new DataTable();

            using (cmd)
            {
                cmd.Connection = objQuery.Context.Connection;
                cmd.Transaction = transaction;
                using (var reader =await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
            }

            return dt;
        }
    }
}
