using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Commands
{
    public sealed class MaxCommand : ACommandScript
    {
        public override string Name => "max";

        public override string Parameters => string.Empty;

        public override bool IsRestricted => true;

        public override void Execute(Character caller, string[] args)
        {
            caller.Level = 200;
            caller.Experience = 0;
            caller.MaxHealth = 30000;
            caller.Health = 30000;
            caller.MaxMana = 30000;
            caller.Mana = 30000;
            caller.Strength = 10000;
            caller.Dexterity = 10000;
            caller.Intelligence = 10000;
            caller.Luck = 10000;
            caller.Meso = int.MaxValue;
        }
    }
}
