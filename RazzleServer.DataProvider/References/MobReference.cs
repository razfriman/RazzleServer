using System.Collections.Generic;
using System.Linq;
using RazzleServer.Common.Constants;
using RazzleServer.Wz;
using Serilog;

namespace RazzleServer.DataProvider.References
{
    public class MobReference
    {
        private readonly ILogger _log = Log.ForContext<MobReference>();
        
        public int MapleId { get; set; }

        public List<LootReference> Loots { get; set; } = new List<LootReference>();
        public List<MobSkillReference> Skills { get; set; } = new List<MobSkillReference>();
        public List<MobStatus> Buffs { get; set; } = new List<MobStatus>();
        public List<int> DeathSummons { get; set; } = new List<int>();
        public readonly Dictionary<byte, MobAttackDataReference> Attacks = new Dictionary<byte, MobAttackDataReference>();
        public short Level { get; set; }
        public uint Health { get; set; }
        public uint Mana { get; set; }
        public uint MaxHealth { get; set; }
        public uint MaxMana { get; set; }
        public int HealthRecovery { get; set; }
        public int ManaRecovery { get; set; }
        public uint Experience { get; set; }
        public int Link { get; set; }
        public short SummonType { get; set; }
        public double Traction { get; set; }
        public byte HpBarForeColor { get; set; }
        public byte HpBarBackColor { get; set; }
        public int WeaponAttack { get; set; }
        public int WeaponDefense { get; set; }
        public int MagicAttack { get; set; }
        public int MagicDefense { get; set; }
        public short Accuracy { get; set; }
        public short Avoidability { get; set; }
        public short Speed { get; set; }
        public bool IsUndead { get; set; }

        public bool IsBodyAttack { get; set; }

        public string ElementAttribute { get; set; }

        public bool IsNoRegen { get; set; }

        public bool IsInvincible { get; set; }

        public bool IsSelfDestruction { get; set; }

        public bool IsFirstAttack { get; set; }
        public bool IsNoFlip { get; set; }

        public bool IsPublicReward { get; set; }

        public bool IsFlies { get; set; }

        public bool IsPushed { get; set; }

        public bool IsBoss { get; set; }

        public MobReference()
        {
        }

        public MobReference(WzImage img, WzImage linkImg = null)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }

            MapleId = id;
            var info = img["info"];
            info.WzProperties.ForEach(node =>
            {
                switch (node.Name)
                {
                    case "link":
                        break;
                    case "level":
                        Level = (byte)node.GetInt();
                        break;
                    case "undead":
                        IsUndead = node.GetInt() > 0;
                        break;
                    case "bodyAttack":
                        IsBodyAttack = node.GetInt() > 0;
                        break;
                    case "summonType":
                        SummonType = node.GetShort();
                        break;
                    case "exp":
                        Experience = (uint)node.GetInt();
                        break;
                    case "maxHP":
                        MaxHealth = (uint)node.GetInt();
                        Health = MaxHealth;
                        break;
                    case "maxMP":
                        MaxMana = (uint)node.GetInt();
                        Mana = MaxMana;
                        break;
                    case "elemAttr":
                        ElementAttribute = node.GetString();
                        break;
                    case "PADamage":
                        WeaponAttack = node.GetInt();
                        break;
                    case "PDDamage":
                        WeaponDefense = node.GetInt();
                        break;
                    case "MADamage":
                        MagicAttack = node.GetInt();
                        break;
                    case "MDDamage":
                        MagicDefense = node.GetInt();
                        break;
                    case "eva":
                        Avoidability = node.GetShort();
                        break;
                    case "pushed":
                        IsPushed = node.GetInt() > 0;
                        break;
                    case "noregen":
                        IsNoRegen = node.GetInt() > 0;
                        break;
                    case "invincible":
                        IsInvincible = node.GetInt() > 0;
                        break;
                    case "selfDestruction":
                        IsSelfDestruction = node.GetInt() > 0;
                        break;
                    case "firstAttack":
                        IsFirstAttack = node.GetInt() > 0;
                        break;
                    case "noFlip":
                        IsNoFlip = node.GetInt() > 0;
                        break;
                    case "acc":
                        Accuracy = node.GetShort();
                        break;
                    case "publicReward":
                        IsPublicReward = node.GetInt() > 0;
                        break;
                    case "fs":
                        Traction = node.GetFloat();
                        break;
                    case "flySpeed":
                    case "speed":
                        IsFlies = node.Name == "flySpeed";
                        Speed = node.GetShort();
                        break;
                    case "revive":
                        node.WzProperties?.ForEach(x => DeathSummons.Add(x.GetInt()));
                        break;
                    case "skill":
                        node.WzProperties.ForEach(x => Skills.Add(new MobSkillReference(x)));
                        break;
                    case "hpRecovery":
                        HealthRecovery = node.GetInt();
                        break;
                    case "mpRecovery":
                        ManaRecovery = node.GetInt();
                        break;
                    case "hpTagColor":
                        HpBarForeColor = (byte)node.GetInt();
                        break;
                    case "hpTagBgcolor":
                        HpBarBackColor = (byte)node.GetInt();
                        break;
                    case "boss":
                        IsBoss = node.GetInt() > 0;
                        break;
                    default:
                        _log.Warning($"Unknown mob info node Mob={MapleId} Name={node.Name} Value={node.WzValue}");
                        break;
                }
            });

            Link = info["link"]?.GetInt() ?? 0;
            var nonInfoNodes = Link > 0 && linkImg != null ? linkImg.WzProperties : img.WzProperties;
            var attackNodes = nonInfoNodes.Where(x => x.Name.StartsWith("attack")).ToList();

            foreach (var attackNode in attackNodes)
            {
                var attackData = new MobAttackDataReference(attackNode);
                Attacks.Add(attackData.Id, attackData);
            }
        }
    }
}
