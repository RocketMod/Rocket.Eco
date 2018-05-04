using Eco.Gameplay.Systems.Chat;
using Eco.Shared.Localization;
using Rocket.API.Chat;
using Rocket.API.Player;

namespace Rocket.Eco
{
    public sealed class EcoChatManager : IChatManager
    {
        public void SendMessage(IOnlinePlayer player, string message, params object[] bindings) => player.SendMessage(message);

        public void Broadcast(string message, params object[] bindings) => ChatManager.ServerMessageToAll(new LocString(message), false);
    }
}