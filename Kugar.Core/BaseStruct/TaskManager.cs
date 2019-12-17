using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using Kugar.Core.Collections;
using Kugar.Core.ExtMethod;

namespace Kugar.Core.BaseStruct
{

    public class TaskManager : ListEx<ITaskItem>
    {
        private static TaskManager defaultManager ;
        
        private static readonly object lockerObj=new object();

        private Timer timer = null;

        public TaskManager()
        {
            timer = new Timer(checkTaskList, null, 500, 0);
        }

        public static TaskManager Default
        {
            get
            {
                if (defaultManager==null)
                {
                    lock (lockerObj)
                    {
                        if (defaultManager==null)
                        {
                            defaultManager = new TaskManager();
                        }
                    }
                }

                return defaultManager;
            }
        }

        public ITaskItem GetTaskByID(Guid id)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i]!=null && this[i].TaskID==id)
                {
                    return this[i];
                }
            }
            
            return null;
        }

        public void RemoveTaskByID(Guid id)
        {
            this.Remove((s) => s.TaskID == id);
        }

        public bool IsNameUnique { set; get; }

        public event EventHandler<TaskProcessComplatedEventArgs> TaskProcessComplated;

        public event EventHandler<TaskItemEventArgs> TaskProcessStarted;

        /// <summary>
        ///     任务准备开始时，引发该事件
        /// </summary>
        public event EventHandler<TaskProcessStartingEventArgs> TaskProcessStarting;

        protected override void InsertItem(int index, ITaskItem item)
        {
            if (item==null)
            {
                return;
            }

            for (int i = 0; i < base.Count; i++)
            {
                var temp = base[i];

                if (temp.TaskID == item.TaskID)
                {
                    throw new ArgumentException("已存在ID为:" + item.TaskID, @"item");
                }

                if (IsNameUnique && temp.Name == item.Name)
                {
                    throw new ArgumentException("已存在名为:" + item.Name, @"item");
                }
            }


            base.InsertItem(index, item);


            if (item is TaskItemBase)
            {
                (item as TaskItemBase).Manager = this;
            }
            

        }

        protected override void SetItem(int index, ITaskItem item)
        {
            for (int i = 0; i < base.Count; i++)
            {
                var temp = base[i];

                if (temp==item)
                {
                    continue;
                }

                if (temp.TaskID == item.TaskID)
                {
                    throw new ArgumentException("已存在ID为:" + item.TaskID, @"item");
                }

                if (IsNameUnique && temp.Name == item.Name)
                {
                    throw new ArgumentException("已存在名为:" + item.Name, @"item");
                }
            }

            base.SetItem(index, item);

            if (item is TaskItemBase)
            {
                (item as TaskItemBase).Manager = this;
            }
        }

        internal  void OnTaskComplated(TaskProcessComplatedEventArgs e)
        {
            Events.EventHelper.RaiseAsync(TaskProcessComplated, this, e);
        }

        internal void OnTaskStarted(TaskItemEventArgs e)
        {
            Events.EventHelper.RaiseAsync(TaskProcessStarted, this, e);
        }

        internal void OnTaskStarting(TaskProcessStartingEventArgs e)
        {
            Events.EventHelper.Raise(TaskProcessStarted, this, e);
        }

        private void checkTaskList(object state)
        {
            timer.Change(Timeout.Infinite, Timeout.Infinite);

            var now = DateTime.Now;
            lock (this)
            {
                if (this.Count>0)
                {
                    foreach (var item in this)
                    {
                        if (item.NextExecuteTime.HasValue && item.TaskType == TaskExecuteType.Auto && now >= item.NextExecuteTime)
                        {
                            try
                            {
                                item.Execute();
                            }
                            catch (Exception)
                            {
                                continue;
                            }
                            
                        }
                    }
                }
            }

            timer.Change(500,0 );
        }
    }

    /// <summary>
    ///     只允许手动执行的任务,,忽略所有参数
    /// </summary>
    public abstract class TaskItem_Manual : TaskItem_TimingInterval
    {

        protected TaskItem_Manual()
            : this(string.Empty,null)
        {
            
        }

        protected TaskItem_Manual(DateTime? lastExecuteTime)
            :this(string.Empty,lastExecuteTime){}

        private static DateTime maxtime = DateTime.MaxValue;

        private TaskItem_Manual(string name, DateTime? lastExecuteTime)
            : base(name,lastExecuteTime,DateTime.MinValue)
        {

        }

        public override TaskExecuteType TaskType
        {
            get
            {
                return TaskExecuteType.Manual;
            }
            set
            {
                //base.TaskType = value;
            }
        }
        
        public override DateTime? NextExecuteTime
        {
            get
            {
                return null;
            }
            protected set
            {
                //base.NextExecuteTime = value;
            }
        }

        protected override DateTime? CalcNextExecuteTime()
        {
            return maxtime;
        }
    }

    /// <summary>
    ///     单次的一次性执行,RunTimeSpan参数为指定的运行时间
    /// </summary>
    public abstract class TaskItem_OnceTimeSchedule : TaskItem_TimingInterval
    {
        protected TaskItem_OnceTimeSchedule()
            : this(string.Empty, null,DateTime.MinValue)
        {
            
        }

        protected TaskItem_OnceTimeSchedule(DateTime? lastExecuteTime, DateTime executeTimeSpan)
            : this(string.Empty, lastExecuteTime, executeTimeSpan)
        {
        }

        protected TaskItem_OnceTimeSchedule(string name, DateTime? lastExecuteTime, DateTime executeTimeSpan)
            : base(name, lastExecuteTime, executeTimeSpan)
        {
        }

        protected override DateTime? CalcNextExecuteTime()
        {
            if (LastExecuteTime!=null && LastExecuteTime.GetValueOrDefault()!=DateTime.MinValue)    //如果已经执行过的
            {
                return DateTime.MaxValue;
            }
            else
            {
                return ExecuteTimeSpan;
            }
        }
    }

    /// <summary>
    ///     定时任务，即每个指定时间执行一次任务
    /// </summary>
    public abstract class TaskItem_TimingInterval : TaskItemBase
    {
        private TimeSpan realTimeSpan;

        protected TaskItem_TimingInterval()
            : this(string.Empty, null,DateTime.MinValue)
        {
            
        }

        protected TaskItem_TimingInterval(DateTime? lastExecuteTime, DateTime executeTimeSpan)
            : this(string.Empty, lastExecuteTime, executeTimeSpan)
        {
        }

        protected TaskItem_TimingInterval(string name, DateTime? lastExecuteTime, DateTime executeTimeSpan)
            : base(name, lastExecuteTime, executeTimeSpan)
        {
        }

        public override DateTime ExecuteTimeSpan
        {
            get
            {
                return base.ExecuteTimeSpan;
            }
            set
            {
                base.ExecuteTimeSpan = value;
                realTimeSpan = value - DateTime.MinValue;

                NextExecuteTime = CalcNextExecuteTime();
            }
        }

        protected override DateTime? CalcNextExecuteTime()
        {
            if (TaskType == TaskExecuteType.Manual)
            {
                return null;
            }

            var tempTime = LastExecuteTime.GetValueOrDefault(DateTime.Now);

            tempTime = tempTime.AddMilliseconds(realTimeSpan.TotalMilliseconds);

            return tempTime;
        }
    }

    /// <summary>
    ///         循环任务，在循环的每一个指定时刻执行，
    ///         如： 每小时的45分，    则 RunTimeSpan=0001-01-01 00：45：00,,CycleType=CycleScheduleType.PerHour
    ///                每天的6点10分，   则 RunTimeSpan=0001-01-01 06：10：00,,CycleType=CycleScheduleType.PerDay
    ///                每月的2号9点0分，则 RunTimeSpan=0001-01-02 09：00：00,,CycleType=CycleScheduleType.PerMonth
    /// </summary>
    public abstract class TaskItem_CycleSchedule : TaskItem_TimingInterval
    {
        protected TaskItem_CycleSchedule(DateTime? lastExecuteTime):
            this(lastExecuteTime,DateTime.MinValue){}

        protected TaskItem_CycleSchedule(DateTime? lastExecuteTime, DateTime executeTimeSpan)
            : this(string.Empty,CycleScheduleType.PerHour, lastExecuteTime, executeTimeSpan)
        {
        }

        protected TaskItem_CycleSchedule(string name, CycleScheduleType cycleType, DateTime? lastExecuteTime, DateTime executeTimeSpan)
            : base(name, lastExecuteTime, executeTimeSpan)
        {
            CycleType = cycleType;
        }

        private CycleScheduleType _cycleType;
        public CycleScheduleType CycleType 
        { 
            set 
            { 
                _cycleType = value;
                NextExecuteTime=CalcNextExecuteTime();
            }
            get { return _cycleType; }
        }

        protected override DateTime? CalcNextExecuteTime()
        {
            if (TaskType==TaskExecuteType.Manual)
            {
                return null;
            }

            var span = this.ExecuteTimeSpan;

            switch (CycleType)
            {
                case CycleScheduleType.PerDay:
                    {
                        DateTime tempTime=DateTime.Now;

                        //if (LastExecuteTime == null || LastExecuteTime<DateTime.Now)
                        //{
                            tempTime = DateTime.Now;

                            if (tempTime.Hour <= span.Hour && tempTime.Minute<=span.Minute)
                            {
                                tempTime = DateTime.Now;
                            }
                            else
                            {
                                tempTime = DateTime.Now.AddDays(1);
                            }
                        //}
                        //else
                        //{
                        //    tempTime = LastExecuteTime.GetValueOrDefault().AddDays(1);
                        //}

                        return new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, span.Hour, span.Minute,
                                                    span.Second); ;
                    }
                case CycleScheduleType.PerWeek:
                    {
                        DateTime tempTime = DateTime.Now;

                        if ((int)tempTime.DayOfWeek < span.Day-1)
                        {
                            tempTime = tempTime.ThisWeek((DayOfWeek)(span.Day-1));
                        }
                        else if ((int)tempTime.DayOfWeek < span.Day-1)
                        {
                            if(tempTime.TimeOfDay>span.TimeOfDay)
                            {
                                tempTime = tempTime.NextWeek((DayOfWeek)(span.Day-1));
                            }
                        }
                        else
                        {
                            tempTime = tempTime.NextWeek((DayOfWeek)(span.Day-1));
                        }

                        return new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, span.Hour, span.Minute, span.Second);
                    }
                    break;
                case CycleScheduleType.PerMonth:
                    {
                        DateTime tempTime;

                        if (LastExecuteTime == null)
                        {
                            tempTime = DateTime.Now;

                            if (tempTime.Day <= span.Day && tempTime.Hour <= span.Hour && tempTime.Minute <= span.Minute)
                            {
                                tempTime = DateTime.Now;
                            }
                            else
                            {
                                tempTime = DateTime.Now.AddMonths(1);
                            }
                        }
                        else
                        {
                            tempTime = LastExecuteTime.GetValueOrDefault().AddMonths(1);
                        }

                        return new DateTime(tempTime.Year, tempTime.Month, span.Day, span.Hour, span.Minute, span.Second);
                    }
                case CycleScheduleType.PerHour:
                    {
                        DateTime tempTime;

                        if (LastExecuteTime == null)
                        {
                            tempTime = DateTime.Now;

                            if (tempTime.Minute < span.Minute)
                            {
                                tempTime = DateTime.Now;
                            }
                            else
                            {
                                tempTime = DateTime.Now.AddHours(1);
                            }
                        }
                        else
                        {
                            tempTime = LastExecuteTime.GetValueOrDefault().AddHours(1);
                        }

                        return new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, tempTime.Hour, span.Minute, span.Second);
                    }
                default:
                    return DateTime.MaxValue;
            }


        }
    }

    public enum CycleScheduleType
    {
        PerMonth,
        PerDay,
        PerHour,
        PerWeek
    }

    /// <summary>
    ///     任务的执行方式
    /// </summary>
    public enum TaskExecuteType
    {
        /// <summary>
        ///     该任务为允许自动执行，根据制定的任务调用
        /// </summary>
        Auto,

        /// <summary>
        ///     该任务为手动执行，不定时执行
        /// </summary>
        Manual
    }

    public abstract class TaskItemBase :MarshalByRefObject, ITaskItem
    {
        private int ErrorCount = 0;

        private Dictionary<ShortGuid,ErrorResetArgs> cacheErrorReset=new Dictionary<ShortGuid,ErrorResetArgs>(3);

        protected TaskItemBase()
        {
            OtherInfo=new Dictionary<string, string>();
            TaskType = TaskExecuteType.Auto;
            this.TaskID = Guid.NewGuid();
            this.ErrorResetTime = 180000;
        }

        protected TaskItemBase(DateTime? lastExecuteTime, DateTime executeTimeSpan):this(string.Empty,lastExecuteTime,executeTimeSpan){}

        protected TaskItemBase(string name,DateTime? lastExecuteTime,  DateTime executeTimeSpan) : this()
        {
            LastExecuteTime = lastExecuteTime;
            Name = name;
            ExecuteTimeSpan = executeTimeSpan;
        }

        private DateTime? _lastExecuteTime;

        private static string _taskTypeName = typeof (TaskItemBase).Name;
        public virtual string TaskTypeName
        {
            get { return _taskTypeName; }
        }

        /// <summary>
        ///     上一次执行的时间
        /// </summary>
        public virtual DateTime? LastExecuteTime
        {
            get { return _lastExecuteTime; }
            set 
            {
                _lastExecuteTime = value;
                NextExecuteTime=CalcNextExecuteTime();
            }
        }

        /// <summary>
        ///     下一次执行的时间
        /// </summary>
        public virtual DateTime? NextExecuteTime { get; protected set; }

        private DateTime _runTimeSpan;
        public virtual DateTime ExecuteTimeSpan
        {
            get { return _runTimeSpan; }
            set
            {
                _runTimeSpan = value;
                NextExecuteTime = CalcNextExecuteTime();
            }
        }

        public virtual bool IsTaskExecuting { get; protected set; }

        public virtual Dictionary<string, string> OtherInfo { get; private set; }

        private TaskExecuteType _taskType = TaskExecuteType.Manual;
        public virtual TaskExecuteType TaskType
        {
            get { return _taskType; }
            set 
            {
                _taskType = value;
                NextExecuteTime = CalcNextExecuteTime();
            }
        }

        public string Name { get;set;}

        public int ErrorResetTime { get;set;}
            
        public void Execute()
        {
            Execute(true, null);
        }

        public void Execute(object state)
        {
            Execute(true,state,null,null);
        }

        public void Execute(bool isCalcNextRunTime,object state)
        {
            Execute(isCalcNextRunTime, state, null,null);
        }

        public void Execute(bool isCalcNextRunTime, object state, EventHandler<TaskProcessComplatedEventArgs> onComplateCallback)
        {
            Execute(isCalcNextRunTime, state, null,onComplateCallback);
        }

        protected void Execute(bool isCalcNextRunTime,object state,ShortGuid? executeID,EventHandler<TaskProcessComplatedEventArgs> onComplateCallback)
        {
            if (IsTaskExecuting)
            {
                throw new TaskItem_TaskExecuteing();
                //return;
            }

            lock (this)
            {
                if (isCalcNextRunTime)
                {
                    LastExecuteTime = DateTime.Now;
                    NextExecuteTime = CalcNextExecuteTime();
                }
            }

            if (!this.OnTaskProcessStarting())
            {
                return;
            }

            lock (this)
            {
                IsTaskExecuting = true;
            }

            if (Manager != null)
            {
                Manager.OnTaskStarted(new TaskItemEventArgs(this));
            }

            this.OnTaskStarted();

            this.OnTaskExecuteStateChanged();

            var realExecuteID = executeID.HasValue ? executeID.GetValueOrDefault() : ShortGuid.NewGuid();

            ThreadPool.QueueUserWorkItem((s) =>
                                            {
                                               
                                                Exception error = null;
                                                try
                                                {
                                                    RunTask(state);
                                                }
                                                catch (Exception ex)
                                                {
                                                    error = ex;

                                                    ErrorResetArgs errorArgs = null;

                                                    if (!cacheErrorReset.ContainsKey(executeID.GetValueOrDefault()))
                                                    {
                                                        errorArgs = new ErrorResetArgs()
                                                        {
                                                            ErrorRaiseTime = DateTime.Now,
                                                            ExecuteID = realExecuteID,
                                                            ExecuteState = state,
                                                            ErrorCount = 0
                                                        };

                                                        cacheErrorReset.Add(realExecuteID, errorArgs);
                                                    }
                                                    else
                                                    {
                                                        errorArgs = cacheErrorReset[realExecuteID];
                                                    }

                                                    if (errorArgs.ErrorCount <= 5)
                                                    {
                                                        errorArgs.ErrorCount++;
                                                        CallbackTimer.Default.Call(ErrorResetTime * 1000, errorResetExec, realExecuteID);
                                                    }
                                                }
                                                finally
                                                {
                                                    IsTaskExecuting = false;

                                                    if (error == null)
                                                    {
                                                        cacheErrorReset.Remove(realExecuteID);
                                                    }

                                                    try
                                                    {
                                                        if (onComplateCallback!=null)
                                                        {
                                                            onComplateCallback(this,new TaskProcessComplatedEventArgs(this,error));
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        
                                                    }
                                                    

                                                    OnTaskProcessComplated(error, realExecuteID);
                                                }
                                            });


        }

        public void RefreshNextExecuteTime()
        {
            lock (this)
            {
                NextExecuteTime = CalcNextExecuteTime();
            }
        }

        public TaskManager Manager { get; internal set; }

        public Guid TaskID { get; private set; }

        public event EventHandler<TaskProcessComplatedEventArgs> TaskProcessComplated;
        public event EventHandler<TaskItemEventArgs> TaskProcessStarted;
        public event EventHandler<TaskProcessStartingEventArgs> TaskProcessStarting;
    
        public event EventHandler<TaskItemEventArgs> TaskExecuteStateChanged;

        /// <summary>
        ///     计算下一次执行的时间
        /// </summary>
        /// <returns></returns>
        protected abstract DateTime? CalcNextExecuteTime();

        protected void OnTaskStarted()
        {
            Events.EventHelper.RaiseAsync(TaskProcessStarted,this,new TaskItemEventArgs(this));
        }

        /// <summary>
        ///     在每执行完一次任务之后，必须调用该函数
        /// </summary>
        /// <param name="error"></param>
        protected virtual void OnTaskProcessComplated(Exception error,ShortGuid executeID)
        {
            lock (this)
            {
                IsTaskExecuting = false;
            }

            var e1=new TaskProcessComplatedEventArgs(this, error);

            if (Manager != null)
            {
                Manager.OnTaskComplated(e1);
            }

            Events.EventHelper.RaiseAsync(TaskProcessComplated, this, e1);

            this.OnTaskExecuteStateChanged();
        }

        private void errorResetExec(object state)
        {
            if (!(state is Guid))
            {
                return;
            }

            var executeID = (Guid) state;

            if (cacheErrorReset.ContainsKey(executeID))
            {
                var item = cacheErrorReset[executeID];

                this.Execute(false,item.ExecuteState,executeID,null);
            }
        }

        /// <summary>
        ///     在每执行完一次任务之后，必须调用该函数
        /// </summary>
        protected virtual bool OnTaskProcessStarting()
        {
            lock (this)
            {
                IsTaskExecuting = false;
            }

            if (this.TaskProcessStarting != null)
            {
                try
                {
                    var e1 = new TaskProcessStartingEventArgs(this);

                    if (Manager != null)
                    {
                        Manager.OnTaskStarting(e1);

                        if (e1.Cancel)
                        {
                            return false;
                        }
                    }

                    this.TaskProcessStarting(this, e1);

                    if (e1.Cancel)
                    {
                        return false;
                    }

                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual void OnTaskExecuteStateChanged()
        {
            if (TaskExecuteStateChanged!=null)
            {
                TaskExecuteStateChanged(this, new TaskItemEventArgs(this));
            }
        }

        protected abstract void RunTask(object state);

        public abstract object Clone();

        [Serializable]
        private class ErrorResetArgs
        {
            public DateTime ErrorRaiseTime;
            public object ExecuteState;
            public Guid ExecuteID;
            public int ErrorCount;
        }
    }

    public interface ITaskItem : ICloneable
    {
        string TaskTypeName { get; }

        /// <summary>
        ///     该任务上一次执行的时间
        /// </summary>
        DateTime? LastExecuteTime { get; }

        /// <summary>
        ///     该任务下一次执行的时间
        /// </summary>
        DateTime? NextExecuteTime { get; }
        
        /// <summary>
        ///     执行的时间间隔，该参数的具体定义，请参考派生类的说明
        /// </summary>
        DateTime ExecuteTimeSpan { get; set; }

        /// <summary>
        ///     任务的执行状态，为true时，表示该任务正在执行，为false时，表示该任务尚未执行
        /// </summary>
        bool IsTaskExecuting { get; }

        /// <summary>
        ///     该任务的其他信息
        /// </summary>
        Dictionary<string, string> OtherInfo { get; }

        /// <summary>
        ///     任务的执行方式,默认为Auto
        /// </summary>
        TaskExecuteType TaskType { get; set; }

        /// <summary>
        ///     任务的名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     当任务执行后出现错误时,该属性指定多少秒时间后重新执行
        /// </summary>
        int ErrorResetTime { set; get; }

        Guid TaskID { get; }

        /// <summary>
        ///     执行该任务，并刷新下次执行时间
        /// </summary>
        void Execute();

        void Execute(object state);

        /// <summary>
        ///     执行该任务，该方法为异步执行，判断是否完成任务，请订阅TaskProcessComplated事件
        /// </summary>
        /// <param name="isCalcNextRunTime">是否刷新下一次执行的时间，如需手动执行任务，并且希望该任务依然按照计划定时执行的话，该值为false</param>
        /// <param name="state"></param>
        void Execute(bool isCalcNextRunTime, object state);

        /// <summary>
        ///     执行该任务，该方法为异步执行，判断是否完成任务，请订阅TaskProcessComplated事件
        /// </summary>
        /// <param name="isCalcNextRunTime">是否刷新下一次执行的时间，如需手动执行任务，并且希望该任务依然按照计划定时执行的话，该值为false</param>
        /// <param name="state"></param>
        /// <param name="onComplateCallback"></param>
        void Execute(bool isCalcNextRunTime, object state, EventHandler<TaskProcessComplatedEventArgs> onComplateCallback);

        /// <summary>
        ///     手动刷新下一次运行的时间
        /// </summary>
        void RefreshNextExecuteTime();

        TaskManager Manager { get; }

        /// <summary>
        ///     任务完成时，引发该事件
        /// </summary>
        event EventHandler<TaskProcessComplatedEventArgs> TaskProcessComplated;

        /// <summary>
        ///     任务已经开始时，引发该事件
        /// </summary>
        event EventHandler<TaskItemEventArgs> TaskProcessStarted;

        /// <summary>
        ///     任务准备开始时，引发该事件
        /// </summary>
        event EventHandler<TaskProcessStartingEventArgs> TaskProcessStarting;

        /// <summary>
        ///     任务执行状态改变时，引发该事件
        /// </summary>
        event EventHandler<TaskItemEventArgs> TaskExecuteStateChanged;

    }

    [Serializable]
    public class TaskItemEventArgs:EventArgs
    {
        public TaskItemEventArgs(ITaskItem task)
        {
            Task = task;
        }

        public ITaskItem Task { get; private set; }
    }

    [Serializable]
    public class TaskProcessComplatedEventArgs : TaskItemEventArgs
    {
        public TaskProcessComplatedEventArgs(ITaskItem task, Exception error) : base(task)
        {
            Error = error;
        }

        public Guid ExecuteID { private set; get; }
        public Exception Error { get; private set; }
        public DateTime ErrorRaiseTime { get; set; }
        public int ErrorCount { get; set; }
        public bool HasError { get { return Error != null; } }
    }

    [Serializable]
    public class TaskProcessStartingEventArgs : TaskItemEventArgs
    {
        public TaskProcessStartingEventArgs(ITaskItem task) : base(task)
        {
            Cancel = false;
        }

        public bool Cancel { set; get; }
    }

    [Serializable]
    public class TaskItem_TaskExecuteing:Exception
    {
        
    }
}
