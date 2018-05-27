using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Shops;
using RazzleServer.Game.Scripts;

namespace RazzleServer.Game.Maple.Life
{
    public class Npc : LifeObject, ISpawnable, IControllable
    {
        [JsonIgnore]
        public Character Controller { get; set; }

        public Shop Shop { get; set; }
        public int StorageCost { get; set; }

        [JsonIgnore]
        public Dictionary<Character, NpcScript> Scripts { get; private set; }

        private readonly ILogger _log = LogManager.Log;

        public Npc() { }

        public Npc(WzImageProperty img)
            : base(img, LifeObjectType.Npc)
        {
            Scripts = new Dictionary<Character, NpcScript>();
        }

        public void Move(PacketReader iPacket)
        {
            var action1 = iPacket.ReadByte();
            var action2 = iPacket.ReadByte();

            Movements movements = null;

            if (iPacket.Available > 0)
            {
                movements = new Movements(iPacket);
            }

            using (var oPacket = new PacketWriter(ServerOperationCode.NpcMove))
            {

                oPacket.WriteInt(ObjectId);
                oPacket.WriteByte(action1);
                oPacket.WriteByte(action2);

                if (movements != null)
                {
                    oPacket.WriteBytes(movements.ToByteArray());
                }

                Map.Send(oPacket);
            }
        }

        public void Converse(Character talker)
        {
            if (Shop != null)
            {
                Shop.Show(talker);
            }
            else if (StorageCost > 0)
            {
                talker.Storage.Show(this);
            }
            else
            {
                var script = new NpcScript(this, talker);

                Scripts[talker] = script;

                try
                {
                    script.Execute();
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Error executing NPC script");
                }
            }
        }

        public void Handle(Character talker, PacketReader iPacket)
        {
            if (talker.LastNpc == null)
            {
                return;
            }

            var lastMessageType = (NpcMessageType)iPacket.ReadByte();
            var action = iPacket.ReadByte();

            var selection = -1;

            byte endTalkByte;

            switch (lastMessageType)
            {
                case NpcMessageType.RequestText:
                case NpcMessageType.RequestNumber:
                case NpcMessageType.RequestStyle:
                case NpcMessageType.Choice:
                    endTalkByte = 0;
                    break;

                default:
                    endTalkByte = byte.MaxValue;
                    break;
            }

            if (action != endTalkByte)
            {
                if (iPacket.Available >= 4)
                {
                    selection = iPacket.ReadInt();
                }
                else if (iPacket.Available > 0)
                {
                    selection = iPacket.ReadByte();
                }

                if (lastMessageType == NpcMessageType.RequestStyle)
                {
                    //selection = this.StyleSelectionHelpers[talker][selection];
                }

                Scripts[talker].SetResult(selection != -1 ? selection : action);
            }
            else
            {
                talker.LastNpc = null;
            }
        }

        public void AssignController()
        {
            if (Controller == null)
            {
                var leastControlled = int.MaxValue;
                Character newController = null;

                lock (Map.Characters)
                {
                    foreach (var character in Map.Characters.Values)
                    {
                        if (character.ControlledNpcs.Count < leastControlled)
                        {
                            leastControlled = character.ControlledNpcs.Count;
                            newController = character;
                        }
                    }
                }

                newController?.ControlledNpcs.Add(this);
            }
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket() => GetInternalPacket(false);

        public PacketWriter GetControlRequestPacket() => GetInternalPacket(true);

        private PacketWriter GetInternalPacket(bool requestControl)
        {
            var oPacket = new PacketWriter(requestControl ? ServerOperationCode.NpcChangeController : ServerOperationCode.NpcEnterField);

            if (requestControl)
            {
                oPacket.WriteBool(true);
            }

            oPacket.WriteInt(ObjectId);
            oPacket.WriteInt(MapleId);
            oPacket.WritePoint(Position);
            oPacket.WriteBool(!FacesLeft);
            oPacket.WriteShort(Foothold);
            oPacket.WriteShort(MinimumClickX);
            oPacket.WriteShort(MaximumClickX);

            return oPacket;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.NpcChangeController);

            oPacket.WriteBool(false);
            oPacket.WriteInt(ObjectId);

            return oPacket;
        }

        public PacketWriter GetDialogPacket(string text, NpcMessageType messageType, params byte[] footer)
        {
            var oPacket = new PacketWriter(ServerOperationCode.ScriptMessage);


            oPacket.WriteByte(4); // NOTE: Unknown.
            oPacket.WriteInt(MapleId);
            oPacket.WriteByte((byte)messageType);
            oPacket.WriteByte(0); // NOTE: Speaker.
            oPacket.WriteString(text);
            oPacket.WriteBytes(footer);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.NpcLeaveField);

            oPacket.WriteInt(ObjectId);

            return oPacket;
        }
    }
}
