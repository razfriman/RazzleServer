using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterTrocks
    {
        public Character Parent { get; private set; }

        public List<int> Regular { get; private set; }
        public List<int> VIP { get; private set; }

        public CharacterTrocks(Character parent)
        {
            Parent = parent;

            Regular = new List<int>();
            VIP = new List<int>();
        }

        public void Load()
        {
            foreach (Datum datum in new Datums("trocks").Populate("CharacterID = {0}", Parent.ID))
            {
                byte index = (byte)datum["Index"];
                int map = (int)datum["Map"];

                if (index >= 5)
                {
                    VIP.Add(map);
                }
                else
                {
                    Regular.Add(map);
                }
            }
        }

        public void Save()
        {
            //Database.Delete("trocks", "CharacterID = {0}", this.Parent.ID);

            byte index = 0;

            foreach (int map in Regular)
            {
                Datum datum = new Datum("trocks");

                datum["CharacterID"] = Parent.ID;
                datum["Index"] = index++;
                datum["Map"] = map;

                datum.Insert();
            }

            index = 5;

            foreach (int map in VIP)
            {
                Datum datum = new Datum("trocks");

                datum["CharacterID"] = Parent.ID;
                datum["Index"] = index++;
                datum["Map"] = map;

                datum.Insert();
            }
        }

        public bool Contains(int mapID)
        {
            foreach (int map in Regular)
            {
                if (map == mapID)
                {
                    return true;
                }
            }

            foreach (int map in VIP)
            {
                if (map == mapID)
                {
                    return true;
                }
            }

            return false;
        }

        public void Update(PacketReader iPacket)
        {
            var action = (TrockAction)iPacket.ReadByte();
            var type = (TrockType)iPacket.ReadByte();

            switch (action)
            {
                case TrockAction.Remove:
                    {
                        int mapID = iPacket.ReadInt();

                        if (type == TrockType.Regular)
                        {
                            if (!Regular.Contains(mapID))
                            {
                                return;
                            }

                            Regular.Remove(mapID);
                        }
                        else if (type == TrockType.VIP)
                        {
                            if (!VIP.Contains(mapID))
                            {
                                return;
                            }

                            VIP.Remove(mapID);
                        }
                    }
                    break;

                case TrockAction.Add:
                    {
                        int mapID = Parent.Map.MapleID;

                        // TODO: Check if the map field limits allow trocks (e.g. Maple Island is forbidden).

                        if (true)
                        {
                            if (type == TrockType.Regular)
                            {
                                Regular.Add(mapID);
                            }
                            else if (type == TrockType.VIP)
                            {
                                VIP.Add(mapID);
                            }
                        }
                        else
                        {
                        }
                    }
                    break;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MapTransferResult))
            {
                oPacket.WriteByte((byte)(action == TrockAction.Remove ? 2 : 3));
                oPacket.WriteByte((byte)type);
                oPacket.WriteBytes(type == TrockType.Regular ? RegularToByteArray() : VIPToByteArray());

                Parent.Client.Send(oPacket);
            }
        }

        public bool Use(int itemID, PacketReader iPacket)
        {
            bool used = false;
            byte action = iPacket.ReadByte();

            TrockType type = itemID == 5040000 ? TrockType.Regular : TrockType.VIP;

            int destinationMapID = -1;
            TrockResult result = TrockResult.Success;

            if (action == 0) // NOTE: Preset map.
            {
                int mapID = iPacket.ReadInt();

                if (!Parent.Trocks.Contains(mapID))
                {
                    result = TrockResult.CannotGo;
                }

                destinationMapID = mapID;
            }
            else if (action == 1) // NOTE: IGN.
            {
                string targetName = iPacket.ReadString();

                Character target = null;// this.Parent.Client.Channel.Characters.GetCharacter(targetName);

                if (target == null)
                {
                    result = TrockResult.DifficultToLocate;
                }
                else
                {
                    destinationMapID = target.Map.MapleID;
                }
            }

            iPacket.ReadInt(); // NOTE: Ticks.

            if (destinationMapID != -1)
            {
                var originMap = Parent.Map;
                var destinationMap = DataProvider.Maps[destinationMapID];

                // TODO: Field limit check.
                // TODO: Origin map field limit check.
                // TODO: Continent check.
                if (originMap.MapleID == destinationMap.MapleID)
                {
                    result = TrockResult.AlreadyThere;
                }
            }

            if (result == TrockResult.Success)
            {
                Parent.ChangeMap(destinationMapID);

                used = true;
            }
            else
            {
                using (var oPacket = new PacketWriter(ServerOperationCode.MapTransferResult))
                {
                    oPacket.WriteByte((byte)result);
                    oPacket.WriteByte((byte)type);

                    Parent.Client.Send(oPacket);
                }
            }

            return used;
        }

        public byte[] RegularToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                int remaining = 1;

                while (remaining <= Regular.Count)
                {
                    oPacket.WriteInt(Regular[remaining - 1]);

                    remaining++;
                }

                while (remaining <= 5)
                {
                    oPacket.WriteInt(999999999);

                    remaining++;
                }

                return oPacket.ToArray();
            }
        }

        public byte[] VIPToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                int remaining = 1;

                while (remaining <= VIP.Count)
                {
                    oPacket.WriteInt(VIP[remaining - 1]);

                    remaining++;
                }

                while (remaining <= 10)
                {
                    oPacket.WriteInt(999999999);

                    remaining++;
                }

                return oPacket.ToArray();
            }
        }
    }
}
