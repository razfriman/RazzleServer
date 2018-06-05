using System.Collections.Generic;
using RazzleServer.Common.Util;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple.Util
{
    public sealed class Movements : List<Movement>
    {
        public Point Origin { get; }
        public Point Position { get; }
        public short Foothold { get; }
        public byte Stance { get; }

        public Movements(PacketReader packet)
        {
            short foothold = 0;
            byte stance = 0;
            var position = packet.ReadPoint();

            Origin = position;

            var count = packet.ReadByte();

            //26
            //00 01 
            //63 FE 57 06
            //0A

            //00 
            //73 FE 5A 06 
            //80 00 20 00 
            //56 00 
            //02 
            //78 00

            //00
            //74 FE 5B 06
            //84 00 F8 FF 
            //56 00 
            //02 
            //0C 00 


            //00
            //86 FE 59 06 
            //7C 00 F9 FF 
            //57 00
            //02 
            //8A 00

            //00
            //89 FE 59 06
            //64 00 FA FF 
            //57 00 
            //08
            //1E 00 

            //00 
            //8E FE 59 06 
            //34 00 FD FF 
            //57 00 
            //0A 
            //3C 00 

            //0E 
            //34 00 FD FF
            //00 00
            //0A
            //00 00 


            //0F
            //8E FE 58 06 34 00 FD FF 57 00 57 00 0A 00 00 0F 8E FE 58 06 34 00 FD FF 00 00 57 00 06 00 00 0F 8E FE 4E 06 00 00 F0 FF 00 00 57 00 06 5A 00 0F 8D FE 51 06 00 00 68 00 00 00 57 00 06 3C 00 11 44 44 44 44 04 11 11 11 01 63 FE 4E 06 8E FE 5B 06
            while (count-- > 0)
            {
                var type = (MovementType)packet.ReadByte();

                var movement = new Movement
                {
                    Type = type,
                    Foothold = foothold,
                    Position = position,
                    Stance = stance
                };
                switch (type)
                {
                    case MovementType.Normal:
                    case MovementType.Normal2:
                    case MovementType.NormalFloat:
                        {
                            movement.Position = packet.ReadPoint();
                            movement.Velocity = packet.ReadPoint();
                            movement.Foothold = packet.ReadShort();
                            movement.Stance = packet.ReadByte();
                            movement.Duration = packet.ReadShort();
                        }
                        break;
                    case MovementType.Jump:
                    case MovementType.JumpKnockback:
                    case MovementType.FlashJump:
                    case MovementType.ExcessiveKnockback:
                    case MovementType.RelativeFloat:
                        {
                            movement.Velocity = packet.ReadPoint();
                            movement.Stance = packet.ReadByte();
                            movement.Duration = packet.ReadShort();
                        }
                        break;
                    case MovementType.Immediate:
                    case MovementType.Teleport:
                    case MovementType.Assaulter:
                    case MovementType.Assassinate:
                    case MovementType.Rush:
                    case MovementType.Chair:
                    case MovementType.UnknownTeleport:
                        {
                            movement.Position = packet.ReadPoint();
                            movement.Foothold = packet.ReadShort();
                            movement.Stance = packet.ReadByte();
                            movement.Duration = packet.ReadShort();
                        }
                        break;
                    case MovementType.Falling:
                        {
                            movement.Statistic = packet.ReadByte();
                        }
                        break;
                    case MovementType.JumpDown:
                        {
                            movement.Position = packet.ReadPoint();
                            movement.Velocity = packet.ReadPoint();
                            movement.FallStart = packet.ReadShort();
                            movement.Foothold = packet.ReadShort();
                            movement.Stance = packet.ReadByte();
                            movement.Duration = packet.ReadShort();
                        }
                        break;
                    default:
                        {
                            movement.Stance = packet.ReadByte();
                            movement.Duration = packet.ReadShort();
                        }
                        break;
                }

                position = movement.Position;
                foothold = movement.Foothold;
                stance = movement.Stance;

                Add(movement);
            }

            var keypadStates = packet.ReadByte();

            for (byte i = 0; i < keypadStates; i++)
            {
                if (i % 2 == 0)
                {
                    packet.ReadByte(); // NOTE: Unknown.
                }
            }

            // Rectangle for bounds checking.
            var lt = packet.ReadPoint();
            var rb = packet.ReadPoint();

            Position = position;
            Stance = stance;
            Foothold = foothold;
        }

        public byte[] ToByteArray()
        {
            using (var pw = new PacketWriter())
            {
                pw.WritePoint(Origin);
                pw.WriteByte(Count);

                foreach (var movement in this)
                {
                    pw.WriteByte((byte)movement.Type);

                    switch (movement.Type)
                    {
                        case MovementType.Normal:
                        case MovementType.Normal2:
                        case MovementType.NormalFloat:
                            {
                                pw.WritePoint(movement.Position);
                                pw.WritePoint(movement.Velocity);
                                pw.WriteShort(movement.Foothold);
                                pw.WriteByte(movement.Stance);
                                pw.WriteShort(movement.Duration);
                            }
                            break;

                        case MovementType.Jump:
                        case MovementType.JumpKnockback:
                        case MovementType.FlashJump:
                        case MovementType.ExcessiveKnockback:
                        case MovementType.RecoilShot:
                        case MovementType.RelativeFloat:
                            {
                                pw.WriteShort(movement.Velocity.X);
                                pw.WriteShort(movement.Velocity.Y);
                                pw.WriteByte(movement.Stance);
                                pw.WriteShort(movement.Duration);
                            }
                            break;

                        case MovementType.Immediate:
                        case MovementType.Teleport:
                        case MovementType.Assaulter:
                        case MovementType.Assassinate:
                        case MovementType.Rush:
                        case MovementType.Chair:
                        case MovementType.UnknownTeleport:
                            {
                                pw.WritePoint(movement.Position);
                                pw.WriteShort(movement.Foothold);
                                pw.WriteByte(movement.Stance);
                                pw.WriteShort(movement.Duration);
                            }
                            break;

                        case MovementType.Falling:
                            {
                                pw.WriteByte(movement.Statistic);
                            }
                            break;

                        case MovementType.JumpDown:
                            {
                                pw.WritePoint(movement.Position);
                                pw.WritePoint(movement.Velocity);
                                pw.WriteShort(movement.FallStart);
                                pw.WriteShort(movement.Foothold);
                                pw.WriteByte(movement.Stance);
                                pw.WriteShort(movement.Duration);
                            }
                            break;

                        default:
                            {
                                pw.WriteByte(movement.Stance);
                                pw.WriteShort(movement.Duration);
                            }
                            break;
                    }
                }

                // NOTE: Keypad and boundary values are not read on the client side.
                return pw.ToArray();
            }
        }
    }
}
