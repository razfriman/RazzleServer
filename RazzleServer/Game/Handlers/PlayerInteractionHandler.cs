using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Interaction;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.PlayerInteraction)]
    public class PlayerInteractionHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var code = (InteractionCode)packet.ReadByte();

            switch (code)
            {
                case InteractionCode.Create:
                {
                    var type = (InteractionType)packet.ReadByte();

                    switch (type)
                    {
                        case InteractionType.Omok:
                        {
                        }
                            break;

                        case InteractionType.Trade:
                        {
                            if (client.Character.Trade == null)
                            {
                                client.Character.Trade = new Trade(client.Character);
                            }
                        }
                            break;

                        case InteractionType.PlayerShop:
                        {
                            var description = packet.ReadString();

                            if (client.Character.PlayerShop == null)
                            {
                                client.Character.PlayerShop = new PlayerShop(client.Character, description);
                            }
                        }
                            break;
                    }
                }
                    break;

                case InteractionCode.Visit:
                {
                    if (client.Character.PlayerShop == null)
                    {
                        var objectId = packet.ReadInt();

                        if (client.Character.Map.PlayerShops.Contains(objectId))
                        {
                            client.Character.Map.PlayerShops[objectId].AddVisitor(client.Character);
                        }
                    }
                }
                    break;

                default:
                {
                    if (client.Character.Trade != null)
                    {
                        client.Character.Trade.Handle(client.Character, code, packet);
                    }
                    else
                    {
                        client.Character.PlayerShop?.Handle(client.Character, code, packet);
                    }
                }
                    break;
            }
        }
    }
}
