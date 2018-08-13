using Rocket.API;
using Rocket.API.Configuration;
using Rocket.Core.Configuration;
using Rocket.Eco.API.Configuration;

namespace Rocket.Eco.Configuration
{
    public sealed class EcoSettingsProvider : IEcoSettingsProvider
    {
        private readonly IRuntime runtime;
        private readonly IConfiguration configuration;

        public EcoSettingsProvider(IRuntime runtime, IConfiguration configuration)
        {
            this.runtime = runtime;
            this.configuration = configuration;
        }

        public void Load()
        {
            ConfigurationContext context = new ConfigurationContext(runtime, "Configuration");
            configuration.Load(context, Settings);
            Settings = configuration.Get(Settings);
        }

        public void Reload()
        {
            configuration.Reload();
        }

        public void Save()
        {
            configuration.Set(Settings);
            configuration.Save();
        }

        public EcoSettings Settings { get; private set; } = new EcoSettings();
    }
}
