using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Logging;
using Rocket.API.Scheduler;
using Rocket.Eco.API;

namespace Rocket.Eco.Scheduling
{
    public sealed class EcoTaskScheduler : ContainerAccessor, ITaskScheduler
    {
        //TODO: Make this configurable
        private const long mainTargetTickrate = 60;
        private readonly Thread asyncThread;

        private readonly object lockObj = new object();

        private readonly Thread mainThread;
        private readonly List<ITask> tasks = new List<ITask>();

        internal EcoTaskScheduler(IDependencyContainer container) : base(container)
        {
            mainThread = new Thread(MainThreadWork);
            asyncThread = new Thread(AsyncThreadWork);

            mainThread.Start();
            asyncThread.Start();
        }

        //Creating a clone to ensure the collection will always be the same as the moment it was accessed.
        public ReadOnlyCollection<ITask> Tasks
        {
            get
            {
                lock (lockObj)
                {
                    return new List<ITask>(tasks.Where(c => c.Owner.IsAlive)).AsReadOnly();
                }
            }
        }

        public ITask Schedule(ILifecycleObject @object, Action action, ExecutionTargetContext target)
        {
            ThreadSafeTask task = new ThreadSafeTask(this, @object, action, target);

            lock (lockObj)
            {
                tasks.Add(task);
            }

            return task;
        }

        public ITask ScheduleNextFrame(ILifecycleObject @object, Action action) => Schedule(@object, action, ExecutionTargetContext.NextFrame);

        public ITask ScheduleEveryFrame(ILifecycleObject @object, Action action) => Schedule(@object, action, ExecutionTargetContext.EveryFrame);

        public ITask ScheduleNextPhysicUpdate(ILifecycleObject @object, Action action) => Schedule(@object, action, ExecutionTargetContext.NextPhysicsUpdate);

        public ITask ScheduleEveryPhysicUpdate(ILifecycleObject @object, Action action) => Schedule(@object, action, ExecutionTargetContext.EveryPhysicsUpdate);

        public ITask ScheduleNextAsyncFrame(ILifecycleObject @object, Action action) => Schedule(@object, action, ExecutionTargetContext.NextAsyncFrame);

        public ITask ScheduleEveryAsyncFrame(ILifecycleObject @object, Action action) => Schedule(@object, action, ExecutionTargetContext.EveryAsyncFrame);

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
                IEnumerable<ITask> newTasks = Tasks.Where(x =>
                    x.ExecutionTarget == ExecutionTargetContext.NextFrame || x.ExecutionTarget == ExecutionTargetContext.NextPhysicsUpdate || x.ExecutionTarget == ExecutionTargetContext.EveryFrame || x.ExecutionTarget == ExecutionTargetContext.NextPhysicsUpdate);

                foreach (ITask task in newTasks)
                {
                    if (!task.IsCancelled && !task.IsFinished)
                    {
                        task.Action.Invoke();

                        if (task.ExecutionTarget != ExecutionTargetContext.NextFrame && task.ExecutionTarget != ExecutionTargetContext.NextPhysicsUpdate)
                            continue;
                    }

                    lock (lockObj)
                    {
                        tasks.Remove(task);
                    }
                }

                watch.Stop();

                long time = watch.ElapsedMilliseconds - 1000 / mainTargetTickrate;

                if (time < 0)
                {
                    Thread.Sleep((int) -time);
                }
                else if (time != 0)
                {
                    ILogger logger = Container.Resolve<ILogger>();
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
                IEnumerable<ITask> newTasks = Tasks.Where(x =>
                    x.ExecutionTarget == ExecutionTargetContext.NextAsyncFrame || x.ExecutionTarget == ExecutionTargetContext.EveryAsyncFrame);

                foreach (ITask task in newTasks)
                {
                    if (!task.IsCancelled && !task.IsFinished)
                    {
                        task.Action.Invoke();

                        if (task.ExecutionTarget != ExecutionTargetContext.NextAsyncFrame)
                            continue;
                    }

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