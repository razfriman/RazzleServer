using System;
using System.Linq;
using System.Text;
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
            var builder = new StringBuilder();
            for (var i = 0; i < mapIds.Length; i++)
            {
                builder.Append("\r\n#L" + i + "##m" + mapIds[i] + "##l");
            }

            var result = SendChoice("Where do you want to go?#b" + builder.ToString());
            Character.ChangeMap(mapIds[result]);
        }
    }
}
