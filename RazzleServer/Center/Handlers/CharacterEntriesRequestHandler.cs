using System;
namespace RazzleServer.Center.Handlers
{
    public class CharacterEntriesRequestHandler
    {
        public CharacterEntriesRequestHandler()
        {
        }

        //private void CharacterEntriesRequest(PacketReader inPacket)
        //{
        //    byte worldID = inPacket.ReadByte();
        //    int accountID = inPacket.ReadInt();

        //    using (PacketReader outPacket = new Packet(InteroperabilityOperationCode.CharacterEntriesRequest))
        //    {
        //        outPacket.WriteInt(accountID);

        //        WvsCenter.Worlds[worldID].RandomChannel.Send(outPacket);
        //    }
        //}
    }
}
