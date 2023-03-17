using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kugar.Core.BaseStruct;
using Kugar.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kugar.Core.Collections
{
    /// <summary>
    /// 定时轮询的容器
    /// </summary>
    public class PollingCollection:IEnumerable<PollingItemBase>
    {
        private ConcurrentDictionary<Guid, PollingItemBase> _pollingItems = new();

        public bool Insert(PollingItemBase item)
        {
            if (item.TaskId==Guid.Empty)
            {
                item.TaskId=Guid.Empty;
            }

            item.Collection = this;

            return _pollingItems.TryAdd(item.TaskId, item);
        }

        public void RemoveById(Guid id)
        {

            if (_pollingItems.TryRemove(id,out var item))
            {
                item.Collection = null;
            }
        }
        
        public IEnumerator<PollingItemBase> GetEnumerator()
        {
            foreach (var item in _pollingItems)
            {
                yield return item.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class PollingItemBase:IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="initQueryTime">初始化的查询次数</param>
        public PollingItemBase(int initQueryTime = 0)
        {
            QueryTime = initQueryTime;
        }

        /// <summary>
        /// 查询操作
        /// </summary>
        /// <returns></returns>
        public abstract Task<ResultReturn> Query(IServiceProvider provider);

        public abstract Task ExpireQuery(IServiceProvider provider);

        /// <summary>
        /// 当前查询次数
        /// </summary>
        public int QueryTime {private set; get; }

        /// <summary>
        /// 最大查询次数,超过最大查询次数,将自动移除,不在轮询
        /// </summary>
        public abstract int MaxQueryTime { get; }

        /// <summary>
        /// 任务Id
        /// </summary>
        public Guid TaskId { get; internal set; }

        internal PollingCollection Collection { get; set; }

        internal async Task Execute(IServiceProvider provider)
        {
            try
            {
                var result = await Query(provider);

                this.QueryTime++;

                if (result.IsSuccess)
                {
                    Collection.RemoveById(TaskId);

                    this.Dispose();
                }
                else
                {
                    if (this.QueryTime > this.MaxQueryTime)
                    {
                        await this.ExpireQuery(provider);

                        Collection.RemoveById(TaskId);

                        this.Dispose();
                    }
                }
            }
            catch (Exception e)
            { 
            }
            
        }

        public virtual void Dispose()
        {
        }
    }

    public class PollingQueryTask:TimerHostedService
    {
        public PollingQueryTask(IServiceProvider provider) : base(provider)
        {
        }

        protected override async Task Run(IServiceProvider serviceProvider, CancellationToken stoppingToken)
        {
            var coll = (PollingCollection)serviceProvider.GetService(typeof(PollingCollection));

            if (coll==null)
            {
                return;
            }

            await Task.WhenAll(new PollingExecutor(serviceProvider,coll));
        }

        protected override int Internal => 5000;

        private class PollingExecutor:IEnumerable<Task>
        {
            public PollingExecutor(IServiceProvider provider, PollingCollection coll)
            {
                Provider = provider;
                Collection = coll;
            }

            public IServiceProvider Provider {  get; }

            public PollingCollection Collection { get; }

            public IEnumerator<Task> GetEnumerator()
            {
                foreach (var item in Collection)
                {
                    yield return item.Execute(Provider);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    public static class PollingQueryExt
    {
        /// <summary>
        /// 注册轮询容器,建议在所有task注册之前调用该功能
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterPollingCollection(this IServiceCollection services)
        {
            services.AddSingleton<PollingCollection>();
            services.AddHostedService<PollingQueryTask>();

            return services;
        }
    }
    
}
