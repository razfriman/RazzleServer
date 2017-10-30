namespace RazzleServer.Center.Handlers
{
    public class CharacterEntriesResponseHandler
    {
        //private void CharacterEntriesResponse(PacketReader inPacket)
        //{
        //    int accountID = inPacket.ReadInt();
        //    List<byte[]> entires = new List<byte[]>();

        //    while (inPacket.Remaining > 0)
        //    {
        //        entires.Add(inPacket.ReadBytes(inPacket.ReadByte()));
        //    }

        //    using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterEntriesResponse))
        //    {
        //        outPacket.WriteInt(accountID);

        //        foreach (var entry in entires)
        //        {
        //            outPacket.WriteByte((byte)entry.Length);
        //            outPacket.WriteBytes(entry);
        //        }

        //        WvsCenter.Login.Send(outPacket);
        //    }
        //}
    }
}
