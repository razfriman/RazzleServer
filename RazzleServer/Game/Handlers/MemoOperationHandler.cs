using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MemoOperation)]
    public class MemoOperationHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var action = (MemoAction)packet.ReadByte();

            switch (action)
            {
                case MemoAction.Send:
                    {
                        // TODO: This is occured when you send a note from the Cash Shop.
                        // As we don't have Cash Shop implemented yet, this remains unhandled.
                    }
                    break;

                case MemoAction.Delete:
                    {
                        var count = packet.ReadByte();
                        var a = packet.ReadByte();
                        var b = packet.ReadByte();

                        for (byte i = 0; i < count; i++)
                        {
                            var id = packet.ReadInt();

                            if (!client.Character.Memos.Contains(id))
                            {
                                continue;
                            }

                            client.Character.Memos[id].Delete();
                        }

                    }
                    break;
            }
        }
    }
}