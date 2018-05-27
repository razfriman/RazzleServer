using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Maple
{

    public sealed class Movement
    {
        public MovementType Type { get; set; }
        public Point Position { get; set; }
        public Point Velocity { get; set; }
        public short FallStart { get; set; }
        public short Foothold { get; set; }
        public short Duration { get; set; }
        public byte Stance { get; set; }
        public byte Statistic { get; set; }
    }

    public sealed class Movements : List<Movement>
    {
        public Point Origin { get; private set; }
        public Point Position { get; private set; }
        public short Foothold { get; private set; }
        public byte Stance { get; private set; }

        public Movements(PacketReader iPacket)
        {
            short foothold = 0;
            byte stance = 0;
            var position = iPacket.ReadPoint();

            Origin = position;

            var count = iPacket.ReadByte();

            while (count-- > 0)
            {
                var type = (MovementType)iPacket.ReadByte();

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
                        {
                            movement.Position = iPacket.ReadPoint();
                            movement.Velocity = iPacket.ReadPoint();
                            movement.Foothold = iPacket.ReadShort();
                            movement.Stance = iPacket.ReadByte();
                            movement.Duration = iPacket.ReadShort();
                        }
                        break;
                    case MovementType.Jump:
                    case MovementType.JumpKnockback:
                    case MovementType.FlashJump:
                    case MovementType.ExcessiveKnockback:
                        {
                            movement.Velocity = iPacket.ReadPoint();
                            movement.Stance = iPacket.ReadByte();
                            movement.Duration = iPacket.ReadShort();
                        }
                        break;
                    case MovementType.Immediate:
                    case MovementType.Teleport:
                    case MovementType.Assaulter:
                    case MovementType.Assassinate:
                    case MovementType.Rush:
                    case MovementType.Chair:
                        {
                            movement.Position = iPacket.ReadPoint();
                            movement.Foothold = iPacket.ReadShort();
                            movement.Stance = iPacket.ReadByte();
                            movement.Duration = iPacket.ReadShort();
                        }
                        break;
                    case MovementType.Falling:
                        {
                            movement.Statistic = iPacket.ReadByte();
                        }
                        break;
                    case MovementType.Unknown:
                        {
                            movement.Velocity = iPacket.ReadPoint();
                            movement.FallStart = iPacket.ReadShort();
                            movement.Stance = iPacket.ReadByte();
                            movement.Duration = iPacket.ReadShort();
                        }
                        break;
                    default:
                        {
                            movement.Stance = iPacket.ReadByte();
                            movement.Duration = iPacket.ReadShort();
                        }
                        break;
                }

                position = movement.Position;
                foothold = movement.Foothold;
                stance = movement.Stance;

                Add(movement);
            }

            var keypadStates = iPacket.ReadByte();

            for (byte i = 0; i < keypadStates; i++)
            {
                if (i % 2 == 0)
                {
                    iPacket.ReadByte(); // NOTE: Unknown.
                }
            }

            // Rectangle for bounds checking.
            var lt = iPacket.ReadPoint();
            var rb = iPacket.ReadPoint();

            Position = position;
            Stance = stance;
            Foothold = foothold;
        }

        public byte[] ToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                oPacket.WritePoint(Origin);
                oPacket.WriteByte(Count);

                foreach (var movement in this)
                {
                    oPacket.WriteByte((byte)movement.Type);

                    switch (movement.Type)
                    {
                        case MovementType.Normal:
                        case MovementType.Normal2:
                            {
                                oPacket.WritePoint(movement.Position);
                                oPacket.WritePoint(movement.Velocity);
                                oPacket.WriteShort(movement.Foothold);
                                oPacket.WriteByte(movement.Stance);
                                oPacket.WriteShort(movement.Duration);
                            }
                            break;

                        case MovementType.Jump:
                        case MovementType.JumpKnockback:
                        case MovementType.FlashJump:
                        case MovementType.ExcessiveKnockback:
                        case MovementType.RecoilShot:
                            {
                                oPacket.WriteShort(movement.Velocity.X);
                                oPacket.WriteShort(movement.Velocity.Y);
                                oPacket.WriteByte(movement.Stance);
                                oPacket.WriteShort(movement.Duration);
                            }
                            break;

                        case MovementType.Immediate:
                        case MovementType.Teleport:
                        case MovementType.Assaulter:
                        case MovementType.Assassinate:
                        case MovementType.Rush:
                        case MovementType.Chair:
                            {
                                oPacket.WritePoint(movement.Position);
                                oPacket.WriteShort(movement.Foothold);
                                oPacket.WriteByte(movement.Stance);
                                oPacket.WriteShort(movement.Duration);
                            }
                            break;

                        case MovementType.Falling:
                            {
                                oPacket.WriteByte(movement.Statistic);
                            }
                            break;

                        case MovementType.Unknown:
                            {

                                oPacket.WritePoint(movement.Velocity);
                                oPacket.WriteShort(movement.FallStart);
                                oPacket.WriteByte(movement.Stance);
                                oPacket.WriteShort(movement.Duration);
                            }
                            break;

                        default:
                            {

                                oPacket.WriteByte(movement.Stance);
                                oPacket.WriteShort(movement.Duration);
                            }
                            break;
                    }
                }

                // NOTE: Keypad and boundary values are not read on the client side.
                return oPacket.ToArray();
            }
        }
    }
}
