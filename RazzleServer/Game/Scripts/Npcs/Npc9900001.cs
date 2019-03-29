using System.Text;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Scripting;

namespace RazzleServer.Game.Scripts.Npcs
{
    [NpcScript("9900001")]
    public class Npc9900001 : ANpcScript
    {
        public override void Execute()
        {
            SendOk("Welcome to RazzleServer");
            var mapIds = new[] {100000000, 101000000};
            var result = SendChoice("Where do you want to go?" + Blue(CreateSelectionList(NpcListType.Map, mapIds)));
            Character.ChangeMap(mapIds[result]);
        }
    }
}
