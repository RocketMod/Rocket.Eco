using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Rocket.API;
using Rocket.API.DependencyInjection;
using Rocket.API.Scheduler;
using Rocket.Eco.API;

namespace Rocket.Eco.Scheduling
{
    public sealed class EcoTaskScheduler : ContainerAccessor, ITaskScheduler
    {
        private readonly IEnumerable<ITask> tasks = new List<ITask>();
        internal EcoTaskScheduler(IDependencyContainer container) : base(container) { }
        public ReadOnlyCollection<ITask> Tasks => tasks.Where(c => c.Owner.IsAlive).ToList().AsReadOnly();

        public ITask Schedule(ILifecycleObject @object, Action action, ExecutionTargetContext target) => throw new NotImplementedException();

        public ITask ScheduleNextFrame(ILifecycleObject @object, Action action) => throw new NotImplementedException();

        public ITask ScheduleEveryFrame(ILifecycleObject @object, Action action) => throw new NotImplementedException();

        public ITask ScheduleNextPhysicUpdate(ILifecycleObject @object, Action action) => throw new NotImplementedException();

        public ITask ScheduleEveryPhysicUpdate(ILifecycleObject @object, Action action) => throw new NotImplementedException();

        public ITask ScheduleNextAsyncFrame(ILifecycleObject @object, Action action) => throw new NotImplementedException();

        public ITask ScheduleEveryAsyncFrame(ILifecycleObject @object, Action action) => throw new NotImplementedException();

        public bool CancelTask(ITask task) => throw new NotImplementedException();
    }
}