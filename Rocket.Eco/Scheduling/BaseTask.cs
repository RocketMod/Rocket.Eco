using System;
using Rocket.API;
using Rocket.API.Scheduler;

namespace Rocket.Eco.Scheduling
{
    //TODO: Move this into Rocket.Core for universal functionality
    public class BaseTask : ITask
    {
        private readonly ITaskScheduler scheduler;

        public BaseTask(ITaskScheduler scheduler, ILifecycleObject owner, Action action, ExecutionTargetContext executionTargetContext)
        {
            this.scheduler = scheduler;
            Owner = owner;
            Action = action;
            ExecutionTarget = executionTargetContext;
        }

        public ILifecycleObject Owner { get; }
        public Action Action { get; }
        public ExecutionTargetContext ExecutionTarget { get; }

        public bool IsCancelled { get; protected set; }
        public Exception Exception { get; protected set; }
        public bool IsFinished { get; protected set; }

        public virtual void Cancel()
        {
            scheduler.CancelTask(this);
        }
    }
}
