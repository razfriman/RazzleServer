using System;
using System.Collections.Generic;

namespace RazzleServer.Data.WZ
{
    public class WzMap
    {
        public int MapId { get; set; }

        public int FieldType { get; set; }

        public FieldLimit Limit { get; set; }

        public string FieldScript { get; set; }

        public string FirstUserEnter { get; set; }

        public string UserEnter { get; set; }

        public string Name { get; set; }

        public int Fly { get; set; }

        public int Swim { get; set; }

        public int ForcedReturn { get; set; }

        public int ReturnMap { get; set; }

        public int TimeLimit { get; set; }

        public bool Town { get; set; }

        public double MobRate { get; set; }

        public short TopBorder { get; set; }
        public short LeftBorder { get; set; }
        public short RightBorder { get; set; }
        public short BottomBorder { get; set; }

        public List<LadderRope> LaderRopes = new List<LadderRope>();

        public List<MobSpawn> MobSpawnPoints = new List<MobSpawn>();

        public List<Npc> Npcs = new List<Npc>();

        public Dictionary<string, Portal> Portals = new Dictionary<string, Portal>();

        public List<FootHold> FootHolds = new List<FootHold>();

        public List<Reactor> Reactors = new List<Reactor>();

        public class LadderRope
        {
            public Point StartPoint { get; set; }

            public Point EndPoint { get; set; }
        }

        public class MobSpawn
        {
            public int MobId { get; set; }

            public WzMob wzMob { get; set; }

            public int MobTime { get; set; }

            public Point Position { get; set; }
           
            public short Rx0 { get; set; }

            public short Rx1 { get; set; }

            public short Cy { get; set; }

            public short Fh { get; set; }

            public bool F { get; set; }

            public bool Hide { get; set; }
        }

        public class Npc
        {
            public int Id { get; set; }

            public short x { get; set; }

            public short y { get; set; }

            public short Rx0 { get; set; }

            public short Rx1 { get; set; }

            public short Cy { get; set; }

            public short Fh { get; set; }

            public bool F { get; set; }

            public bool Hide { get; set; }
        }

        public class Portal
        {
            public byte Id { get; set; }

            public PortalType Type { get; set; }

            public Point Position { get; set; }            

            public int ToMap { get; set; }

            public string Name { get; set; }

            public string ToName { get; set; }

            public string Script { get; set; }
        }

        public class FootHold
        {
            public short Id { get; set; }
            public short Prev { get; set; }
            public short Next { get; set; }
            public Point Point1 { get; set; }
            public Point Point2 { get; set; }

            public bool IsWall
            {
                get
                {
                    return Point1.X == Point2.X;
                }
            }
        }

        public class Reactor
        {
            public int Id { get; set; }
            public int ReactorTime { get; set; }
            public Point Position { get; set; }
            public byte State { get; set; }
        }

        public enum PortalType : byte 
        {
            Startpoint = 0x0, //sp
            Invisible = 0x1, //pi
            Visible = 0x2, //pv
            Collision = 0x3, //pc
            Changable = 0x4, //pg
            ChangableInvisible = 0x5, //pgi
            TownportalPoint = 0x6, //tp
            Script = 0x7, //ps
            ScriptInvisible = 0x8, //psi
            CollisionScript = 0x9, //pcs
            Hidden = 0xA, //ph
            ScriptHidden = 0xB, //psh
            CollisionVerticalJump = 0xC, //pcj
            CollisionCustomImpact = 0xD, //pci
            CollisionUnknownPcig = 0xE //pcig
        }

        [Flags]
        public enum FieldLimit
        {
            None = 0,
            Jump = 1,
            MovementSkill = 2,
            SummonBag = 4,
            MysticDoor = 8,
            ChangeChannel = 0x10,
            PortalScroll = 0x20,
            TeleportItem = 0x40,
            MiniGame = 0x80,
            SpecificPortalScroll = 0x100,
            Mount = 0x200,
            Potion = 0x400,
            PartyLeaderChange = 0x800,
            NoMobCapacity = 0x1000,
            WeddingInvitation = 0x2000,
            CashShopWeatherItem = 0x4000,
            Pet = 0x8000,
            AntiMacro = 0x10000,
            FallDown = 0x20000,
            SummonNpc = 0x40000,
            NoExpDecrease = 0x80000,
            NoDamageOnFalling = 0x100000,
            OpenParcel = 0x200000,
            DropItem = 0x400000
        }
    }
}
