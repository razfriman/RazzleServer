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
            sbyte type = (sbyte)packet.ReadByte();
            packet.ReadByte(); // NOTE: Elemental type.
            int damage = packet.ReadInt();
            bool damageApplied = false;
            bool deadlyAttack = false;
            byte hit = 0;
            byte stance = 0;
            int disease = 0;
            byte level = 0;
            short mpBurn = 0;
            int mobObjectID = 0;
            int mobID = 0;
            int noDamageSkillID = 0;

            if (type != MapDamage)
            {
                mobID = packet.ReadInt();
                mobObjectID = packet.ReadInt();

                if (!client.Character.Map.Mobs.Contains(mobObjectID))
                {
                    return;
                }
                var mob = client.Character.Map.Mobs[mobObjectID];

                if (mobID != mob.MapleID)
                {
                    return;
                }

                if (type != BumpDamage)
                {
                    // TODO: Get mob attack and apply to disease/level/mpBurn/deadlyAttack.
                }
            }

            hit = packet.ReadByte();
            byte reduction = packet.ReadByte();
            packet.ReadByte(); // NOTE: Unknown.

            if (reduction != 0)
            {
                // TODO: Return damage (Power Guard).
            }

            if (type == MapDamage)
            {
                level = packet.ReadByte();
                disease = packet.ReadInt();
            }
            else
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
                oPacket.WriteInt(client.Character.ID);
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
                            oPacket.WriteInt(mobID);
                            oPacket.WriteByte(hit);
                            oPacket.WriteByte(reduction);

                            if (reduction > 0)
                            {
                                // TODO: PGMR stuff.
                            }

                            oPacket.WriteByte(stance);
                            oPacket.WriteInt(damage);

                            if (noDamageSkillID > 0)
                            {
                                oPacket.WriteInt(noDamageSkillID);
                            }
                        }
                        break;
                }

                client.Character.Map.Broadcast(oPacket, client.Character);
            }
        }
    }
}