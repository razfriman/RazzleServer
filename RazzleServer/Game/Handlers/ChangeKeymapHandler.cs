using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.ChangeKeymap)]
    public class ChangeKeymapHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var mode = packet.ReadInt();
            var count = packet.ReadInt();

            if (mode == 0)
            {
                if (count == 0)
                {
                    return;
                }

                for (var i = 0; i < count; i++)
                {
                    var key = (KeymapKey)packet.ReadInt();
                    var type = (KeymapType)packet.ReadByte();
                    var action = (KeymapAction)packet.ReadInt();

                    if (client.Character.Keymap.Contains(key))
                    {
                        if (type == KeymapType.None)
                        {
                            client.Character.Keymap.Remove(key);
                        }
                        else
                        {
                            client.Character.Keymap[key].Type = type;
                            client.Character.Keymap[key].Action = action;
                        }
                    }
                    else
                    {
                        client.Character.Keymap.Add(new Shortcut(key, action, type));
                    }
                }
            }
            else if (mode == 1) // NOTE: Pet automatic mana potion.
            {
                client.Character.Release();
            }
            else if (mode == 2) // NOTE: Pet automatic mana potion.
            {
                client.Character.Release();
            }
        }
    }
}