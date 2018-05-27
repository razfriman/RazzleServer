using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Data;

namespace RazzleServer.Game.Maple.Characters
{
    public sealed class CharacterTrocks
    {
        public Character Parent { get; private set; }

        public List<int> Regular { get; private set; }
        public List<int> Vip { get; private set; }

        public CharacterTrocks(Character parent)
        {
            Parent = parent;

            Regular = new List<int>();
            Vip = new List<int>();
        }

        public void Load()
        {
            //foreach (Datum datum in new Datums("trocks").Populate("CharacterId = {0}", Parent.Id))
            //{
            //    byte index = (byte)datum["Index"];
            //    int map = (int)datum["Map"];

            //    if (index >= 5)
            //    {
            //        VIP.Add(map);
            //    }
            //    else
            //    {
            //        Regular.Add(map);
            //    }
            //}
        }

        public void Save()
        {
            //Database.Delete("trocks", "CharacterId = {0}", this.Parent.Id);

            //byte index = 0;

            //foreach (int map in Regular)
            //{
            //    Datum datum = new Datum("trocks");

            //    datum["CharacterId"] = Parent.Id;
            //    datum["Index"] = index++;
            //    datum["Map"] = map;

            //    datum.Insert();
            //}

            //index = 5;

            //foreach (int map in VIP)
            //{
            //    Datum datum = new Datum("trocks");

            //    datum["CharacterId"] = Parent.Id;
            //    datum["Index"] = index++;
            //    datum["Map"] = map;

            //    datum.Insert();
            //}
        }

        public bool Contains(int mapId)
        {
            foreach (var map in Regular)
            {
                if (map == mapId)
                {
                    return true;
                }
            }

            foreach (var map in Vip)
            {
                if (map == mapId)
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
                        var mapId = iPacket.ReadInt();

                        if (type == TrockType.Regular)
                        {
                            if (!Regular.Contains(mapId))
                            {
                                return;
                            }

                            Regular.Remove(mapId);
                        }
                        else if (type == TrockType.Vip)
                        {
                            if (!Vip.Contains(mapId))
                            {
                                return;
                            }

                            Vip.Remove(mapId);
                        }
                    }
                    break;

                case TrockAction.Add:
                    {
                        var mapId = Parent.Map.MapleId;

                        // TODO: Check if the map field limits allow trocks (e.g. Maple Island is forbidden).

                        if (true)
                        {
                            if (type == TrockType.Regular)
                            {
                                Regular.Add(mapId);
                            }
                            else if (type == TrockType.Vip)
                            {
                                Vip.Add(mapId);
                            }
                        }
                    }
                    break;
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.MapTransferResult))
            {
                oPacket.WriteByte((byte)(action == TrockAction.Remove ? 2 : 3));
                oPacket.WriteByte((byte)type);
                oPacket.WriteBytes(type == TrockType.Regular ? RegularToByteArray() : VipToByteArray());

                Parent.Client.Send(oPacket);
            }
        }

        public bool Use(int itemId, PacketReader iPacket)
        {
            var used = false;
            var action = iPacket.ReadByte();

            var type = itemId == 5040000 ? TrockType.Regular : TrockType.Vip;

            var destinationMapId = -1;
            var result = TrockResult.Success;

            if (action == 0) // NOTE: Preset map.
            {
                var mapId = iPacket.ReadInt();

                if (!Parent.Trocks.Contains(mapId))
                {
                    result = TrockResult.CannotGo;
                }

                destinationMapId = mapId;
            }
            else if (action == 1) // NOTE: IGN.
            {
                var targetName = iPacket.ReadString();

                Character target = null;// this.Parent.Client.Channel.Characters.GetCharacter(targetName);

                if (target == null)
                {
                    result = TrockResult.DifficultToLocate;
                }
                else
                {
                    destinationMapId = target.Map.MapleId;
                }
            }

            iPacket.ReadInt(); // NOTE: Ticks.

            if (destinationMapId != -1)
            {
                var originMap = Parent.Map;
                var destinationMap = DataProvider.Maps.Data[destinationMapId];

                // TODO: Field limit check.
                // TODO: Origin map field limit check.
                // TODO: Continent check.
                if (originMap.MapleId == destinationMap.MapleId)
                {
                    result = TrockResult.AlreadyThere;
                }
            }

            if (result == TrockResult.Success)
            {
                Parent.ChangeMap(destinationMapId);

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
                var remaining = 1;

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

        public byte[] VipToByteArray()
        {
            using (var oPacket = new PacketWriter())
            {
                var remaining = 1;

                while (remaining <= Vip.Count)
                {
                    oPacket.WriteInt(Vip[remaining - 1]);

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
