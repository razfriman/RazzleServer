﻿using System.Collections.Generic;
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
                    case MovementType.Unknown:
                        {
                            movement.Velocity = packet.ReadPoint();
                            movement.FallStart = packet.ReadShort();
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