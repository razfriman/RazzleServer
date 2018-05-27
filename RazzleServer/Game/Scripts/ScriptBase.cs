using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class ScriptBase
    {
        public Character Character { get; private set; }
        public ScriptType Type { get; private set; }
        public string Name { get; private set; }

        protected ScriptBase(ScriptType type, string name, Character character)
        {
            Type = type;
            Character = character;
            Name = name;
        }

        public virtual void Execute()
        {
            
        }
    }
}
