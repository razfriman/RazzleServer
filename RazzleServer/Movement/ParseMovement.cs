using Microsoft.Extensions.Logging;
using RazzleServer.Packet;
using System.Collections.Generic;
using System.Drawing;
using RazzleServer.Util;

namespace RazzleServer.Movement
{
    public static class ParseMovement
    {
        private static ILogger Log = LogManager.Log;

        public static List<MapleMovementFragment> Parse(PacketReader pr)
        {
            List<MapleMovementFragment> movementList = new List<MapleMovementFragment>();
            byte movements = pr.ReadByte();

            for (int i = 0; i < movements; i++)
            {
                byte type = pr.ReadByte();
                switch (type)//completely changed by alonevampire
                {
                    case 0x00:
                    case 0x08:
                    case 0x0F:
                    case 0x12:
                    case 0x17:
                    case 0x3A:
                    case 0x3B:
                    case 0x3C:
                        {
                            Point position = pr.ReadPoint();
                            Point wobble = pr.ReadPoint();
                            short fh = pr.ReadShort();
                            short fhFallStart = 0;
                            Point offset = new Point();
                            if (type == 0xF)
                                fhFallStart = pr.ReadShort();
                            if (type != 0x3A)
                                offset = pr.ReadPoint();
                            byte state = pr.ReadByte();
                            short duration = pr.ReadShort();

                            AbsoluteLifeMovement alm = new AbsoluteLifeMovement(type, position, state, duration, wobble, offset, fh, fhFallStart);
                            movementList.Add(alm);
                            break;
                        }
                    case 0x01:
                    case 0x02:
                    case 0x10:
                    case 0x13:
                    case 0x14:
                    case 0x16:
                    case 0x36:
                    case 0x37:
                    case 0x38:
                    case 0x39:
                        {
                            Point position = pr.ReadPoint();
                            short fhFallStart = 0;
                            if (type == 19 || type == 20)
                                fhFallStart = pr.ReadShort();

                            byte state = pr.ReadByte();
                            short duration = pr.ReadShort();

                            RelativeLifeMovement rlm = new RelativeLifeMovement(type, position, state, duration, fhFallStart);
                            movementList.Add(rlm);
                            break;
                        }

                    case 0x03:
                    case 0x04:
                    case 0x05:
                    case 0x06:
                    case 0x07:
                    case 0x09:
                    case 0x0A:
                    case 0x0B:
                    case 0x0D:
                    case 0x18:
                    case 0x19:
                    case 0x31:
                    case 0x32:
                    case 0x33:
                    case 0x35:
                        {
                            Point position = pr.ReadPoint();
                            short fh = pr.ReadShort();
                            byte state = pr.ReadByte();
                            short duration = pr.ReadShort();

                            TeleportMovement tm = new TeleportMovement(type, position, state, duration, fh);
                            movementList.Add(tm);
                            break;
                        }
                    case 0x1B:
                    case 0x1C:
                    case 0x1D:
                    case 0x1E:
                    case 0x1F:
                    case 0x20:
                    case 0x21:
                    case 0x22:
                    case 0x23:
                    case 0x24:
                    case 0x25:
                    case 0x26:
                    case 0x27:
                    case 0x28:
                    case 0x29:
                    case 0x2A:
                    case 0x2B:
                    case 0x2C:
                    case 0x2D:
                    case 0x2E:
                    case 0x2F:
                    case 0x30:
                    case 0x34:
                        {
                            byte state = pr.ReadByte();
                            short duration = pr.ReadShort();

                            GroundMovement gm = new GroundMovement(type, new Point(), state, duration);
                            movementList.Add(gm);
                            break;
                        }
                    case 0x0E:
                        {
                            Point wobble = pr.ReadPoint();
                            short fhFallStart = pr.ReadShort();
                            byte state = pr.ReadByte();
                            short duration = pr.ReadShort();
                            WobbleMovement m = new WobbleMovement(type, wobble, fhFallStart, state, duration);
                            movementList.Add(m);
                        }
                        break;
                    case 0x0C:
                        {
                            byte wui = pr.ReadByte();
                            ChangeEquipMovement cem = new ChangeEquipMovement(type, wui);
                            movementList.Add(cem);
                            break;
                        }
                    default:
                        Log.LogWarning($"Unknown movement type [{type}] [{pr}]");
                        return null;
                }
            }

            if (movements != movementList.Count) //probably hack
            {
                string packet = pr.ToString();
                Log.LogWarning($"Movement count mismatch in packet [{packet.Substring(0, 5)}] [{packet}]");
                return null;
            }

            return movementList;
        }
    }
}
