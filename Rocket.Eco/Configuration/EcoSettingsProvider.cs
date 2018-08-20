using Rocket.API;
using Rocket.API.Configuration;
using Rocket.Core.Configuration;
using Rocket.Eco.API.Configuration;

namespace Rocket.Eco.Configuration
{
    public sealed class EcoSettingsProvider : IEcoSettingsProvider
    {
        private readonly IHost host;
        private readonly IConfiguration configuration;

        public EcoSettingsProvider(IHost host, IConfiguration configuration)
        {
            this.host = host;
            this.configuration = configuration;
        }

        public void Load()
        {
            ConfigurationContext context = new ConfigurationContext(host, "Configuration");
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
