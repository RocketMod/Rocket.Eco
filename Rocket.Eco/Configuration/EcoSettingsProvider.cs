using Rocket.API;
using Rocket.API.Configuration;
using Rocket.Core.Configuration;
using Rocket.Eco.API.Configuration;

namespace Rocket.Eco.Configuration
{
    public sealed class EcoSettingsProvider : IEcoSettingsProvider
    {
        private readonly IConfiguration configuration;
        private readonly IHost host;

        public EcoSettingsProvider(IHost host, IConfiguration configuration)
        {
            this.host = host;
            this.configuration = configuration;
        }

        public void Load()
        {
            ConfigurationContext context = new ConfigurationContext(host, "Configuration");
            configuration.LoadAsync(context, Settings).GetAwaiter().GetResult();
            Settings = configuration.Get(Settings);
        }

        public void Reload()
        {
            configuration.ReloadAsync().GetAwaiter().GetResult();
        }

        public void Save()
        {
            configuration.Set(Settings);
            configuration.SaveAsync().GetAwaiter().GetResult();
        }

        public EcoSettings Settings { get; private set; } = new EcoSettings();
    }
}