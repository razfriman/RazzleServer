using RazzleServer.Common.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.TakeDamage)]
    public class TakeDamageHandler : GamePacketHandler
    {
        private const sbyte BumpDamage = -1;
        private const sbyte MapDamage = -2;

        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            packet.ReadInt(); // NOTE: Ticks.
            var type = (sbyte)packet.ReadByte();
            packet.ReadByte(); // NOTE: Elemental type.
            var damage = packet.ReadInt();
            var damageApplied = false;
            var deadlyAttack = false;
            byte hit = 0;
            byte stance = 0;
            var disease = 0;
            short mpBurn = 0;
            int mobObjectId;
            var mobId = 0;
            var noDamageSkillId = 0;

            if (type != MapDamage)
            {
                mobId = packet.ReadInt();
                mobObjectId = packet.ReadInt();

                if (!client.Character.Map.Mobs.Contains(mobObjectId))
                {
                    return;
                }
                var mob = client.Character.Map.Mobs[mobObjectId];

                if (mobId != mob.MapleId)
                {
                    return;
                }

                if (type != BumpDamage)
                {
                    // TODO: Get mob attack and apply to disease/level/mpBurn/deadlyAttack.
                }
            }

            hit = packet.ReadByte();
            var reduction = packet.ReadByte();
            packet.ReadByte(); // NOTE: Unknown.

            if (reduction != 0)
            {
                // TODO: Return damage (Power Guard).
            }

            if (type != MapDamage)
            {
                stance = packet.ReadByte();

                if (stance > 0)
                {
                    // TODO: Power Stance.
                }
            }

            if (damage == -1)
            {
                // TODO: Validate no damage skills.
            }

            if (disease > 0 && damage != 0)
            {
                // NOTE: Fake/Guardian don't prevent disease.
                // TODO: Add disease buff.
            }

            if (damage > 0)
            {
                // TODO: Check for Meso Guard.
                // TODO: Check for Magic Guard.
                // TODO: Check for Achilles.

                if (!damageApplied)
                {
                    if (deadlyAttack)
                    {
                        // TODO: Deadly attack function.
                    }
                    else
                    {
                        client.Character.Health -= (short)damage;
                    }

                    if (mpBurn > 0)
                    {
                        client.Character.Mana -= mpBurn;
                    }
                }

                // TODO: Apply damage to buffs.
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.Hit))
            {
                oPacket.WriteInt(client.Character.Id);
                oPacket.WriteByte(type);

                switch (type)
                {
                    case MapDamage:
                        {
                            oPacket.WriteInt(damage);
                            oPacket.WriteInt(damage);
                        }
                        break;

                    default:
                        {
                            oPacket.WriteInt(damage); // TODO: ... or PGMR damage.
                            oPacket.WriteInt(mobId);
                            oPacket.WriteByte(hit);
                            oPacket.WriteByte(reduction);

                            if (reduction > 0)
                            {
                                // TODO: PGMR stuff.
                            }

                            oPacket.WriteByte(stance);
                            oPacket.WriteInt(damage);

                            if (noDamageSkillId > 0)
                            {
                                oPacket.WriteInt(noDamageSkillId);
                            }
                        }
                        break;
                }

                client.Character.Map.Send(oPacket, client.Character);
            }
        }
    }
}