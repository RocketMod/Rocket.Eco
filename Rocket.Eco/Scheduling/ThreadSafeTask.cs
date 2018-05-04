using System;
using Rocket.API;
using Rocket.API.Scheduler;

namespace Rocket.Eco.Scheduling
{
    public sealed class ThreadSafeTask : ITask
    {
        private readonly object exceptionLock = new object();
        private readonly object isCancelledLock = new object();
        private readonly object isFinishedLock = new object();
        private readonly ITaskScheduler scheduler;

        private Exception exception;

        private bool isCancelled;

        private bool isFinished;

        public ThreadSafeTask(ITaskScheduler scheduler, ILifecycleObject owner, Action action, ExecutionTargetContext executionTargetContext)
        {
            this.scheduler = scheduler;
            Owner = owner;
            Action = action;
            ExecutionTarget = executionTargetContext;
        }

        public Exception Exception
        {
            get
            {
                lock (exceptionLock)
                {
                    return exception;
                }
            }
            internal set
            {
                lock (exceptionLock)
                {
                    exception = value;
                }
            }
        }

        public ILifecycleObject Owner { get; }
        public Action Action { get; }
        public ExecutionTargetContext ExecutionTarget { get; }

        public bool IsCancelled
        {
            get
            {
                lock (isCancelledLock)
                {
                    return isCancelled;
                }
            }
            internal set
            {
                lock (isCancelledLock)
                {
                    isCancelled = value;
                }
            }
        }

        public bool IsFinished
        {
            get
            {
                lock (isFinishedLock)
                {
                    return isFinished;
                }
            }
            internal set
            {
                lock (isFinishedLock)
                {
                    isFinished = value;
                }
            }
        }

        public void Cancel()
        {
            scheduler.CancelTask(this);
        }
    }
}