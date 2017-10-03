using System;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Map;
using System.Collections.Generic;
using RazzleServer.Movement;
using System.Drawing;
using RazzleServer.Packet;

namespace RazzleServer.Handlers
{
    [PacketHandler(CMSGHeader.MOVE_PLAYER)]
    public class PlayerMovementHandler : APacketHandler
    {
        public override void HandlePacket(PacketReader pr, MapleClient c)
        {

            pr.Skip(1); //dont know, == 1 in spawn map and becomes 3 after changing map
            pr.Skip(4); //CRC ?
            int tickCount = pr.ReadInt();

            List<MapleMovementFragment> movementList = ParseMovement.Parse(pr);
            updatePosition(movementList, c.Account.Character, 0);
            MapleMap Map = c.Account.Character.Map;
            if (movementList != null && pr.Available > 10 && Map.CharacterCount > 1)
            {
                PacketWriter packet = CharacterMovePacket(c.Account.Character.ID, movementList);
                Map.BroadcastPacket(packet, c.Account.Character);
            }
        }

        public static PacketWriter CharacterMovePacket(int characterId, List<MapleMovementFragment> movementList)
        {
            var pw = new PacketWriter(SMSGHeader.MOVE_PLAYER);
            pw.WriteInt(characterId);
            pw.WriteInt(0);
            pw.WriteByte((byte)movementList.Count);
            movementList.ForEach(x => x.Serialize(pw));
            return pw;
        }

        public static void updatePosition(List<MapleMovementFragment> Movement, MapleCharacter chr, int yoffset)
        {
            if (Movement == null || chr == null)
            {
                return;
            }
            for (int i = Movement.Count - 1; i >= 0; i--)
            {
                if (Movement[i] is AbsoluteLifeMovement)
                {
                    Point position = Movement[i].Position;
                    position.Y += yoffset;
                    chr.Position = position;
                    chr.Stance = Movement[i].State;

                    chr.Foothold = Movement[i].Foothold;
                    break;
                }
            }
            chr.LastMove = Movement;
        }
    }
}