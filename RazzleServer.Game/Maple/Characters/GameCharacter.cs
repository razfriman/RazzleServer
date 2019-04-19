using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.DataProvider;
using RazzleServer.DataProvider.References;
using RazzleServer.Game.Maple.Items;
using RazzleServer.Game.Maple.Life;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Game.Server;
using RazzleServer.Net.Packet;
using Serilog;

namespace RazzleServer.Game.Maple.Characters
{
    public class GameCharacter : Character, IMoveable, ISpawnable
    {
        private readonly ILogger _log = Log.ForContext<GameCharacter>();

        public GameClient Client { get; }
        public ANpcScript NpcScript { get; set; }
        public override AMapleClient BaseClient => Client;

        public Portal ClosestPortal
        {
            get
            {
                Portal closestPortal = null;
                var shortestDistance = double.PositiveInfinity;

                foreach (var loopPortal in Map.Portals.Values)
                {
                    var distance = loopPortal.Position.DistanceFrom(Position);

                    if (distance < shortestDistance)
                    {
                        closestPortal = loopPortal;
                        shortestDistance = distance;
                    }
                }

                return closestPortal;
            }
        }

        public Portal ClosestSpawnPoint
        {
            get
            {
                Portal closestPortal = null;
                var shortestDistance = double.PositiveInfinity;

                foreach (var loopPortal in Map.Portals.Values)
                {
                    if (loopPortal.IsSpawnPoint)
                    {
                        var distance = loopPortal.Position.DistanceFrom(Position);

                        if (distance < shortestDistance)
                        {
                            closestPortal = loopPortal;
                            shortestDistance = distance;
                        }
                    }
                }

                return closestPortal;
            }
        }


        public GameCharacter(int id = 0, GameClient client = null) : base(id) => Client = client;

        public override void Initialize()
        {
            using var pw = new PacketWriter(ServerOperationCode.SetField);
            pw.WriteInt(Client.Server.ChannelId);
            pw.WriteByte(++Portals);
            pw.WriteBool(true);
            pw.WriteInt(Damage.Random.OriginalSeed1);
            pw.WriteInt(Damage.Random.OriginalSeed2);
            pw.WriteInt(Damage.Random.OriginalSeed3);
            pw.WriteInt(Damage.Random.OriginalSeed3);
            pw.WriteShort((ushort)CharacterDataFlags.All); // flags
            pw.WriteBytes(DataToByteArray());
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            Client.Send(pw);

            IsInitialized = true;
            Map.Characters.Add(this);
            Client.Server.World.Send(GamePackets.Notify(Client.Server.World.TickerMessage, NoticeType.ScrollingText));
            // Update buddy list
            // update quest mob kills
            PrimaryStats.UpdateStatsForParty();
            Client.StartPingCheck();
        }


        public override void Release() => PrimaryStats.Update();

        public override void Notify(string message, NoticeType type = NoticeType.PinkText) =>
            Client.Send(GamePackets.Notify(message, type));

        public override void Revive()
        {
            PrimaryStats.Health = 50;
            ChangeMap(Map.CachedReference.ReturnMapId);
        }

        public override void ChangeMap(int mapId, string portalLabel)
        {
            var portal = CachedData.Maps.Data[mapId].Portals.FirstOrDefault(x => x.Label == portalLabel);

            if (portal == null)
            {
                LogCheatWarning(CheatType.InvalidMapChange);
                return;
            }

            ChangeMap(mapId, portal.Id);
        }

        public override void ChangeMap(int mapId, byte? portalId = null)
        {
            _log.Information($"ChangeMap: Character={Id} Map={mapId} Portal={portalId}");
            Map.Characters.Remove(this);

            using var pw = new PacketWriter(ServerOperationCode.SetField);
            pw.WriteInt(Client.Server.ChannelId);
            pw.WriteByte(++Portals);
            pw.WriteBool(false);
            pw.WriteInt(mapId);
            pw.WriteByte(portalId ?? SpawnPoint);
            pw.WriteShort(PrimaryStats.Health);
            Client.Send(pw);
            Client.Server[mapId].Characters.Add(this);
        }


        public override void Attack(PacketReader packet, AttackType type)
        {
            var attack = new Attack(packet, type);

            var skill = attack.SkillId > 0 ? Skills[attack.SkillId] : null;
            skill?.Cast();

            // TODO: Modify packet based on attack type.
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerMeleeAttack);
            pw.WriteInt(Id);
            pw.WriteByte((byte)(attack.Targets * 0x10 + attack.Hits));
            pw.WriteByte(skill?.CurrentLevel ?? 0);

            if (attack.SkillId != 0)
            {
                pw.WriteInt(attack.SkillId);
            }

            pw.WriteByte(attack.Display);
            pw.WriteByte(attack.Animation);
            pw.WriteByte(attack.WeaponSpeed);
            pw.WriteByte(0); // NOTE: Skill mastery.
            pw.WriteInt(0); // NOTE: StarId = Item ID at attack.StarPosition

            foreach (var target in attack.Damages)
            {
                pw.WriteInt(target.Key);
                pw.WriteByte(6);
                if (attack.IsMesoExplosion)
                {
                    pw.WriteByte(target.Value.Count);
                }

                foreach (var hit in target.Value)
                {
                    pw.WriteUInt(hit);
                }
            }

            Map.Send(pw, this);

            foreach (var target in attack.Damages)
            {
                if (!Map.Mobs.Contains(target.Key))
                {
                    continue;
                }

                var mob = Map.Mobs[target.Key];
                mob.IsProvoked = true;
                mob.SwitchController(this);
                foreach (var hit in target.Value)
                {
                    if (mob.Damage(this, hit))
                    {
                        mob.Die();
                    }
                }
            }
        }

        public override void Talk(string text, bool show = true)
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerChat);
            pw.WriteInt(Id);
            pw.WriteBool(IsMaster);
            pw.WriteString(text);
            pw.WriteBool(show);
            Map.Send(pw);
        }

        public override void PerformFacialExpression(int expressionId)
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerEmote);
            pw.WriteInt(Id);
            pw.WriteInt(expressionId);
            Map.Send(pw, this);
        }

        public override void ShowLocalUserEffect(UserEffect effect)
        {
            using var pw = new PacketWriter(ServerOperationCode.Effect);
            pw.WriteByte(effect);
            Client.Send(pw);
        }

        public override void ShowRemoteUserEffect(UserEffect effect, bool skipSelf = false)
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerEffect);
            pw.WriteInt(Id);
            pw.WriteByte((int)effect);
            Map.Send(pw, skipSelf ? this : null);
        }

        public void Converse(Npc npc, QuestReference quest = null)
        {
            LastQuest = quest;
            npc.Converse(this);
        }


        public override void Save()
        {
            if (IsInitialized)
            {
                SpawnPoint = ClosestSpawnPoint?.Id ?? 0;
            }

            base.Save();
        }

        public override void Load()
        {
            base.Load();
            Map = Client?.Server[Map.MapleId];
        }

        public override void Hide(bool isHidden)
        {
            if (isHidden)
            {
                Map.Characters.Hide(this);
            }
            else
            {
                Map.Characters.Show(this);
            }

            using var pw = new PacketWriter(ServerOperationCode.AdminResult);
            pw.WriteByte(AdminResultType.Hide);
            pw.WriteBool(isHidden);
            Send(pw);
        }

        public override void SendItemEffect()
        {
            using var pw = new PacketWriter(ServerOperationCode.ItemEffect);
            pw.WriteInt(Id);
            pw.WriteInt(ItemEffect);
            Map.Send(pw, this);
        }

        public byte[] DataToByteArray()
        {
            var pw = new PacketWriter();
            pw.WriteBytes(StatisticsToByteArray());
            pw.WriteByte(PrimaryStats.BuddyListSlots);
            pw.WriteInt(PrimaryStats.Meso);
            pw.WriteBytes(Items.ToByteArray());
            pw.WriteBytes(Skills.ToByteArray());
            pw.WriteBytes(Quests.ToByteArray());
            pw.WriteShort(0); // Mini games (5 ints)
            pw.WriteBytes(Rings.ToByteArray());
            pw.WriteBytes(TeleportRocks.ToByteArray());
            return pw.ToArray();
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerEnterField);
            pw.WriteInt(Id);
            pw.WriteString(Name);
            pw.WriteBytes(Buffs.ToMapBuffValues());
            pw.WriteShort((short)PrimaryStats.Job);
            pw.WriteBytes(AppearanceToByteArray());
            pw.WriteInt(Items.Available(5110000));
            pw.WriteInt(ItemEffect);
            pw.WriteInt(Item.GetType(Chair) == ItemType.Setup ? Chair : 0);
            pw.WritePoint(Position);
            pw.WriteByte(Stance);
            pw.WriteShort(Foothold);

            var pet = Pets.GetEquippedPet();
            pw.WriteBool(pet != null);
            if (pet != null)
            {
                pw.WriteInt(pet.Item.Id);
                pw.WriteString(pet.Name);
                pw.WriteLong(pet.Item.CashId);
                pw.WritePoint(pet.Position);
                pw.WriteByte(pet.Stance);
                pw.WriteShort(pet.Foothold);
            }

            if (PlayerShop != null && PlayerShop.Owner == this)
            {
                pw.WriteByte(InteractionType.PlayerShop);
                pw.WriteInt(PlayerShop.ObjectId);
                pw.WriteString(PlayerShop.Description);
                pw.WriteBool(PlayerShop.IsPrivate);
                pw.WriteByte(0);
                pw.WriteByte(1);
                pw.WriteByte(PlayerShop.IsFull ? 1 : 2); // NOTE: Visitor availability.
                pw.WriteByte(0);
            }
            else
            {
                pw.WriteByte(0);
            }

            pw.WriteByte(0); // NOTE: Couple ring.
            pw.WriteByte(0); // NOTE: Friendship ring.
            pw.WriteByte(0); // NOTE: Marriage ring.
            pw.WriteByte(0);

            return pw;
        }

        public PacketWriter GetDestroyPacket()
        {
            using var pw = new PacketWriter(ServerOperationCode.RemotePlayerLeaveField);
            pw.WriteInt(Id);
            return pw;
        }
    }
}
