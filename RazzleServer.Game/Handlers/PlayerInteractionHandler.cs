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
                            client.GameCharacter.Trade ??= new Trade(client.GameCharacter);
                        }
                            break;

                        case InteractionType.PlayerShop:
                        {
                            var description = packet.ReadString();
                            client.GameCharacter.PlayerShop ??= new PlayerShop(client.GameCharacter, description);
                        }
                            break;
                    }
                }
                    break;

                case InteractionCode.Visit:
                {
                    if (client.GameCharacter.PlayerShop == null)
                    {
                        var objectId = packet.ReadInt();

                        if (client.GameCharacter.Map.PlayerShops.Contains(objectId))
                        {
                            client.GameCharacter.Map.PlayerShops[objectId].AddVisitor(client.GameCharacter);
                        }
                    }
                }
                    break;

                default:
                {
                    if (client.GameCharacter.Trade != null)
                    {
                        client.GameCharacter.Trade.Handle(client.GameCharacter, code, packet);
                    }
                    else
                    {
                        client.GameCharacter.PlayerShop?.Handle(client.GameCharacter, code, packet);
                    }
                }
                    break;
            }
        }
    }
}
