using System;
namespace RazzleServer.Center.Handlers
{
    public class CharacterNameCheckResponse
    {
        public CharacterNameCheckResponse()
        {
        }

        //private void CharacterNameCheckResponse(PacketReader inPacket)
        //{
        //    string name = inPacket.ReadString();
        //    bool unusable = inPacket.ReadBool();

        //    using (var outPacket = new PacketWriter(InteroperabilityOperationCode.CharacterNameCheckResponse))
        //    {
        //        outPacket
        //            .WriteString(name)
        //            .WriteBool(unusable);

        //        WvsCenter.Login.Send(outPacket);
        //    }
        //}
    }
}
