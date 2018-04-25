using System;
using Rocket.API;

namespace Rocket.Eco.API
{
    public abstract class RuntimeObject
    {
        protected readonly IRuntime Runtime;

        protected RuntimeObject(IRuntime runtime)
        {
            Runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }
    }
}