using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Scripts
{
    public abstract class ScriptBase
    {
        public Character Character { get; }
        public ScriptType Type { get; }
        public string Name { get; }

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
