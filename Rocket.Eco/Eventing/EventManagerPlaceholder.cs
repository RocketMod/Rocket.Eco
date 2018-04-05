using System;

using Eco.Gameplay.Players;

using Rocket.API.Eventing;

namespace Rocket.Eco.Eventing
{
    public static class EventManagerPlaceholder
    {
        public static void CallOnJoin(object obj)
        {
            User user = (User)obj;
        }

        public static void CallOnLeave(object obj)
        {
            User user = (User)obj;
        }
    }
}
