using System.Collections.ObjectModel;

namespace RazzleServer.Game.Maple.Commands
{
    public sealed class Commands : KeyedCollection<string, Command>
    {
        protected override string GetKeyForItem(Command item)
        {
            return item.Name;
        }
    }
}
