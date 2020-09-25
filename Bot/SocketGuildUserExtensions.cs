using System.Linq;
using Discord.WebSocket;

namespace Bot
{
    public static class SocketGuildUserExtensions
    {
        public static bool HasRole(this SocketGuildUser user, string roleName) =>
            user.Roles.Any(role => role.Name == roleName);
    }
}