using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Scheduling;
using Rocket.Core.Logging;

namespace Rocket.Eco.Scheduling
{
    /// <inheritdoc cref="ITaskScheduler" />
    public sealed class EcoTaskScheduler : ITaskScheduler
    {
        //TODO: Make this configurable
        private const long MainTargetTickrate = 60;

        private readonly IDependencyContainer container;

        private readonly object lockObj = new object();
        private readonly ILogger logger;
        private readonly object taskIdLock = new object();

        private readonly List<ITask> tasks = new List<ITask>();

        private int taskId;

        /// <inheritdoc />
        public EcoTaskScheduler(IDependencyContainer container, ILogger logger)
        {
            this.container = container;
            this.logger = logger;

            Thread mainThread = new Thread(MainThreadWork);
            Thread asyncThread = new Thread(AsyncThreadWork);

            mainThread.Start();
            asyncThread.Start();
        }

        /// <inheritdoc />
        public ITask ScheduleAt(ILifecycleObject @object, Action action, string taskName, DateTime date, bool runAsync = false)
        {
            ThreadSafeTask task;
            ExecutionTargetContext context = runAsync ? ExecutionTargetContext.Async : ExecutionTargetContext.NextFrame;

            lock (taskIdLock)
            {
                task = new ThreadSafeTask(taskId++, taskName, this, @object, action, context, TimeSpan.Zero, date, null);
            }

            lock (lockObj)
            {
                tasks.Add(task);
            }

            return task;
        }

        /// <inheritdoc />
        public ITask SchedulePeriodically(ILifecycleObject @object, Action action, string taskName, TimeSpan period, TimeSpan? delay = null, bool runAsync = false)
        {
            ThreadSafeTask task;
            ExecutionTargetContext context = runAsync ? ExecutionTargetContext.Async : ExecutionTargetContext.NextFrame;
            DateTime startTime = DateTime.UtcNow;

            if (delay != null) startTime = startTime.Add(delay.Value);

            lock (taskIdLock)
            {
                task = new ThreadSafeTask(taskId++, taskName, this, @object, action, context, period, startTime, null);
            }

            lock (lockObj)
            {
                tasks.Add(task);
            }

            return task;
        }

        //Creating a clone to ensure the collection will always be the same as the moment it was accessed.
        /// <inheritdoc />
        public IEnumerable<ITask> Tasks
        {
            get
            {
                lock (lockObj)
                {
                    return new List<ITask>(tasks.Where(c => c.Owner.IsAlive));
                }
            }
        }

        /// <inheritdoc />
        public ITask ScheduleUpdate(ILifecycleObject @object, Action action, string taskName, ExecutionTargetContext target)
        {
            ThreadSafeTask task;

            lock (taskIdLock)
            {
                task = new ThreadSafeTask(taskId++, taskName, this, @object, action, target);
            }

            lock (lockObj)
            {
                tasks.Add(task);
            }

            return task;
        }

        /// <inheritdoc />
        public bool CancelTask(ITask task)
        {
            if (!(task is ThreadSafeTask threadSafeTask)) return false;

            threadSafeTask.IsCancelled = true;

            return true;
        }

        private void MainThreadWork()
        {
            //This will allow the `mainThread` to keep a semi-reliable tick-rate.
            Stopwatch watch = new Stopwatch();

            while (true)
            {
                watch.Start();

                //Using `Tasks` to prevent an outside source from modifying the list during an iteration.
                IEnumerable<ThreadSafeTask> newTasks = Tasks.Where(x =>
                                                                x.ExecutionTarget == ExecutionTargetContext.NextFrame || x.ExecutionTarget == ExecutionTargetContext.NextPhysicsUpdate || x.ExecutionTarget == ExecutionTargetContext.EveryFrame || x.ExecutionTarget == ExecutionTargetContext.NextPhysicsUpdate)
                                                            .Cast<ThreadSafeTask>();

                foreach (ThreadSafeTask task in newTasks)
                {
                    if (!task.IsCancelled && !task.IsFinished && !(task.StartTime != null && task.StartTime > DateTime.UtcNow))
                    {
                        if (task.EndTime != null && task.EndTime <= DateTime.UtcNow || task.LastRunTime != null && task.Period != null && task.LastRunTime.Value.Add(task.Period.Value) > DateTime.UtcNow)
                            goto REMOVE;

                        task.Action.Invoke();
                        task.LastRunTime = DateTime.UtcNow;

                        if (task.ExecutionTarget != ExecutionTargetContext.NextFrame && task.ExecutionTarget != ExecutionTargetContext.NextPhysicsUpdate && task.Period != null)
                            continue;
                    }

                    REMOVE:
                    lock (lockObj)
                    {
                        tasks.Remove(task);
                    }
                }

                watch.Stop();

                long time = watch.ElapsedMilliseconds - 1000 / MainTargetTickrate;

                if (time < 0)
                    Thread.Sleep((int) -time);
                else if (time != 0)
                    if (time > 1000)
                    {
                        logger.LogWarning($"The main/physics thread has fallen behind by {time} milliseconds!");
                        logger.LogWarning("Please try to reduce the amount of IO based or heavy tasks called on this thread.");
                    }

                watch.Reset();
            }
        }

        private void AsyncThreadWork()
        {
            while (true)
            {
                IEnumerable<ThreadSafeTask> newTasks = Tasks.Where(x =>
                                                                x.ExecutionTarget == ExecutionTargetContext.NextAsyncFrame || x.ExecutionTarget == ExecutionTargetContext.EveryAsyncFrame)
                                                            .Cast<ThreadSafeTask>();

                foreach (ThreadSafeTask task in newTasks)
                {
                    if (!task.IsCancelled && !task.IsFinished && !(task.StartTime != null && task.StartTime > DateTime.UtcNow))
                    {
                        if (task.EndTime != null && task.EndTime <= DateTime.UtcNow || task.LastRunTime != null && task.Period != null && task.LastRunTime.Value.Add(task.Period.Value) > DateTime.UtcNow)
                            goto REMOVE;

                        task.Action.Invoke();
                        task.LastRunTime = DateTime.UtcNow;

                        if (task.ExecutionTarget != ExecutionTargetContext.NextAsyncFrame && task.Period != null)
                            continue;
                    }

                    REMOVE:
                    lock (lockObj)
                    {
                        tasks.Remove(task);
                    }
                }

                Thread.Sleep(20);
            }
        }
    }
}