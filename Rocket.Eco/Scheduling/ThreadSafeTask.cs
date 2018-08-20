using System;
using Rocket.API;
using Rocket.API.Scheduling;

namespace Rocket.Eco.Scheduling
{
    /// <inheritdoc />
    /// <summary>
    ///     A representation of an <see cref="ITask" /> where every member is thread-safe to get and set.
    /// </summary>
    public sealed class ThreadSafeTask : ITask
    {
        private readonly object isCancelledLock = new object();
        private readonly object isFinishedLock = new object();
        internal readonly object lastRunTimeLock = new object();

        private bool isCancelled;
        private bool isFinished;
        private DateTime? lastRunTime;

        /// <inheritdoc />
        public ThreadSafeTask(int taskId, string name, ITaskScheduler scheduler, ILifecycleObject owner, Action action, ExecutionTargetContext executionTargetContext)
        {
            TaskId = taskId;
            Name = name;
            Scheduler = scheduler;
            Owner = owner;
            Action = action;
            ExecutionTarget = executionTargetContext;
        }

        /// <inheritdoc />
        public ThreadSafeTask(int taskId, string name, ITaskScheduler scheduler, ILifecycleObject owner, Action action, ExecutionTargetContext executionTargetContext, TimeSpan? period, DateTime? startTime, DateTime? endTime) : this(taskId, name, scheduler, owner, action, executionTargetContext)
        {
            Period = period;
            StartTime = startTime;
            EndTime = endTime;
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
        public DateTime? LastRunTime
        {
            get
            {
                lock (lastRunTimeLock)
                {
                    return lastRunTime;
                }
            }
            internal set
            {
                lock (lastRunTimeLock)
                {
                    lastRunTime = value;
                }
            }
        }

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

        /// <inheritdoc />
        public ITaskScheduler Scheduler { get; }
    }
}