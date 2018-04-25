using System;
using Eco.Gameplay.Systems.Chat;
using Eco.Shared.Localization;
using Rocket.API.Chat;
using Rocket.API.I18N;
using Rocket.API.Player;

namespace Rocket.Eco
{
    public sealed class EcoChatManager : IChatManager
    {
        public void SendMessage(IPlayer player, string message, params object[] bindings)
        {
            if (!(player is IOnlinePlayer onlinePlayer))
                throw new ArgumentException("Must be an IOnlinePlayer!", nameof(player));

            onlinePlayer.SendMessage(message);
        }

        public void SendLocalizedMessage(ITranslationLocator translations, IPlayer player, string translationKey, params object[] bindings)
        {
            string message = translations.GetLocalizedMessage(translationKey, bindings);

            SendMessage(player, message);
        }

        public void Broadcast(string message, params object[] bindings)
        {
            ChatManager.ServerMessageToAll(new LocString(message), false);
        }

        public void BroadcastLocalized(ITranslationLocator translations, string translationKey, params object[] bindings)
        {
            string message = translations.GetLocalizedMessage(translationKey, bindings);

            Broadcast(message);
        }
        
    }
}
