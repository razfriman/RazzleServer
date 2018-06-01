using RazzleServer.Common.Util;

namespace RazzleServer.Game.Maple.Commands
{
    public sealed class Commands : MapleKeyedCollection<string, Command>
    {
        public override string GetKey(Command item) => item.Name;
    }
}
