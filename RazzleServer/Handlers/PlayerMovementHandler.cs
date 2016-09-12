using System;
using RazzleServer.Packet;
using RazzleServer.Player;
using RazzleServer.Map;
using System.Collections.Generic;
using RazzleServer.Movement;

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
            pr.Skip(1);
            pr.Skip(4); //new v137
            Point originalPosition = pr.ReadPoint();
            pr.Skip(4); //wobble?

            List<MapleMovementFragment> movementList = ParseMovement.Parse(pr);
            updatePosition(movementList, c.Account.Character, 0);
            MapleMap Map = c.Account.Character.Map;
            if (movementList != null && pr.Available > 10 && Map.CharacterCount > 1)
            {
                PacketWriter packet = CharacterMovePacket(c.Account.Character.ID, movementList, originalPosition);
                Map.BroadcastPacket(packet, c.Account.Character);
            }
        }

        public static PacketWriter CharacterMovePacket(int characterId, List<MapleMovementFragment> movementList, Point startPosition)
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteHeader(SMSGHeader.MOVE_PLAYER);

            pw.WriteInt(characterId);
            pw.WriteInt(0);
            pw.WritePoint(startPosition);
            pw.WriteInt(0);

            byte size = (byte)movementList.Count;
            pw.WriteByte(size);
            for (byte i = 0; i < size; i++)
            {
                MapleMovementFragment mmf = movementList[i];
                mmf.Serialize(pw);
            }

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