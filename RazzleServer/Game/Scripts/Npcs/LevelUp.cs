using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("levelUP")]
    public class LevelUp : ANpcScript
    {
        public override void Execute()
        {
            SendOk("Welcome to RazzleServer");
            var mapIds = new[] {100000000, 101000000};
            var result = SendChoice("Where do you want to go?" + Blue(CreateSelectionList(NpcListType.Map, mapIds)));
            if (result >= 0)
            {
                GameCharacter.ChangeMap(mapIds[result]);
            }
        }
    }
}
