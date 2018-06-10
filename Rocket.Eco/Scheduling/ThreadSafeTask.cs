using System;
using Rocket.API;
using Rocket.API.Scheduler;

namespace Rocket.Eco.Scheduling
{
    /// <inheritdoc />
    /// <summary>
    ///     A representation of an <see cref="ITask" /> where every member is thread-safe to get and set.
    /// </summary>
    public sealed class ThreadSafeTask : ITask
    {
        private readonly object exceptionLock = new object();
        private readonly object isCancelledLock = new object();
        private readonly object isFinishedLock = new object();
        private readonly ITaskScheduler scheduler;

        private bool isCancelled;

        private bool isFinished;

        /// <inheritdoc />
        public ThreadSafeTask(ITaskScheduler scheduler, ILifecycleObject owner, Action action, ExecutionTargetContext executionTargetContext)
        {
            this.scheduler = scheduler;
            Owner = owner;
            Action = action;
            ExecutionTarget = executionTargetContext;
        }

        /// <inheritdoc />
        public int TaskId { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public TimeSpan? Period { get; }

        /// <inheritdoc />
        public DateTime? StartTime { get; }

        /// <inheritdoc />
        public DateTime? EndTime { get; }

        /// <inheritdoc />
        public DateTime? LastRunTime { get; }

        /// <inheritdoc />
        public ILifecycleObject Owner { get; }

        /// <inheritdoc />
        public Action Action { get; }

        /// <inheritdoc />
        public ExecutionTargetContext ExecutionTarget { get; }

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        public ITaskScheduler Scheduler { get; }
    }
}