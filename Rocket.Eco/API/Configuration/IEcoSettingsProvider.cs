namespace Rocket.Eco.API.Configuration
{
    public interface IEcoSettingsProvider
    {
        EcoSettings Settings { get; }
        
        void Load();
        
        void Reload();
        
        void Save();
    }
}
