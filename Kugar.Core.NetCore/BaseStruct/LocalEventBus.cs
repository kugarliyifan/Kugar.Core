using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Kugar.Core.Services;

namespace Kugar.Core.BaseStruct.LocalEventBus
{
    public interface IEventArgs
    {

    }

    public interface IEventBusHandler<in T> where T : IEventArgs
    {
        Task Handle(IServiceProvider provider,T e);
    }

    public interface IEventBusRaiser<in T> where T : IEventArgs
    {
        Task Raise(T e);
    }

    public class LocalBusReaderTask<TEventItem>:BackgroundServiceEx,IDisposable where TEventItem : IEventArgs
    {
        public LocalBusReaderTask(Channel<TEventItem> channel, IEventBusHandler<TEventItem> handler, IServiceProvider provider) : base(provider)
        {
            Channel=channel;
            Handler=handler;
        }

        public Channel<TEventItem> Channel { get; }

        public IEventBusHandler<TEventItem> Handler { get; }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var p=Provider.CreateScope();

            while (await Channel.Reader.WaitToReadAsync(stoppingToken))
            {
                if (Channel.Reader.TryRead(out var item))
                {
                    try
                    {
                        await Handler.Handle(p.ServiceProvider, item);
                    }
                    catch (Exception e)
                    {
                        
                    }
                    
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Channel.Writer.Complete();
        }
    }

    public class LocalBusWriter<TEventItem> : IEventBusRaiser<TEventItem> where TEventItem :  IEventArgs
    {
        private Channel<TEventItem> _channel;

        public LocalBusWriter(Channel<TEventItem> channel)
        {
            _channel = channel;
        }

        public async Task Raise(TEventItem e)
        {
            await _channel.Writer.WriteAsync(e);
        }
    } 

    public static class LocalEventBusGlobalExt
    {
        /// <summary>
        /// 注册一个本地的事件总线,触发时,注入IEventBusRaiser<TEventItem> 类,可直接调用raise函数
        /// </summary>
        /// <typeparam name="TEventItem"></typeparam>
        /// <param name="services"></param>
        /// <param name="handler">事件处理器</param>
        /// <returns></returns>
        public static IServiceCollection RegisterLocalBusHandler<TEventItem>(this IServiceCollection services,IEventBusHandler<TEventItem> handler)
            where TEventItem:IEventArgs
        {
            var channel=Channel.CreateUnbounded<TEventItem>();
            
            services.AddSingleton<Channel<TEventItem>>(channel);

            services.AddHostedService<LocalBusReaderTask<TEventItem>>(p =>
                new LocalBusReaderTask<TEventItem>(channel, handler, p));

            services.AddSingleton<IEventBusRaiser<TEventItem>>(new LocalBusWriter<TEventItem>(channel));

            return services;
        }
         
    }
}
