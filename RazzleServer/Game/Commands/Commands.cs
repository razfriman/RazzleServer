using System.Collections.ObjectModel;

namespace RazzleServer.Commands
{
    public sealed class Commands : KeyedCollection<string, Command>
    {
        protected override string GetKeyForItem(Command item) => item.Name;
    }
}
