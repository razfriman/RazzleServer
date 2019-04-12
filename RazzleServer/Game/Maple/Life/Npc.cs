using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Util;
using RazzleServer.Net.Packet;
using RazzleServer.Wz;

namespace RazzleServer.Game.Maple.Life
{
    public class Npc : LifeObject, ISpawnable, IControllable
    {
        [JsonIgnore] public Character Controller { get; set; }

        [JsonIgnore] public Dictionary<int, NpcShopItem> ShopItems = new Dictionary<int, NpcShopItem>();

        [JsonIgnore] public NpcReference CachedReference => DataProvider.Npcs.Data[MapleId];

        public Npc()
        {
        }

        public Npc(WzImageProperty img)
            : base(img, LifeObjectType.Npc)
        {
        }

        public void Move(PacketReader iPacket)
        {
            var action1 = iPacket.ReadByte();
            var action2 = iPacket.ReadByte();

            var movements = iPacket.Available > 0
                ? new Movements(iPacket)
                : null;

            using var pw = new PacketWriter(ServerOperationCode.NpcMove);
            pw.WriteInt(ObjectId);
            pw.WriteByte(action1);
            pw.WriteByte(action2);

            if (movements != null)
            {
                pw.WriteBytes(movements.ToByteArray());
            }

            Map.Send(pw);
        }

        public void ShowShop(Character customer)
        {
            using var pw = new PacketWriter(ServerOperationCode.NpcShopShow);
            pw.WriteInt(MapleId);
            pw.WriteShort((short)ShopItems.Count);
            ShopItems.Values.ToList().ForEach(x => pw.WriteBytes(x.ToByteArray()));
            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);
            customer.Client.Send(pw);
        }

        public void Converse(Character talker)
        {
            if (CachedReference.ShopItems.Any())
            {
                if (!ShopItems.Any())
                {
                    ShopItems = DataProvider.Npcs.Data.GetValueOrDefault(MapleId).ShopItems
                        .Select(x => new NpcShopItem(x))
                        .ToDictionary(x => x.MapleId, x => x);
                }

                talker.CurrentNpcShop = this;
                ShowShop(talker);
            }
            else if (CachedReference.StorageCost > 0)
            {
                talker.Storage.Show(this);
            }
            else
            {
                ScriptProvider.Npcs.Execute(this, talker);
            }
        }

        public void Handle(Character talker, PacketReader packet)
        {
            var script = talker.NpcScript;


            if (script == null)
            {
                return;
            }

            var lastMessageType = (NpcMessageType)packet.ReadByte();
            var action = packet.ReadByte();

            switch (lastMessageType)
            {
                case NpcMessageType.Standard:
                    if (action == 0)
                    {
                        script.State--;

                        if (script.State <= 0)
                        {
                            script = null;
                            return;
                        }

                        script.Send(script.States[script.State], false);
                    }
                    else if (action == 1)
                    {
                        if (script.State + 1 < script.States.Count)
                        {
                            script.State++;
                            script.Send(script.States[script.State], false);
                        }
                        else
                        {
                            script.State++;
                            script.SetResult(1);
                        }
                    }
                    else
                    {
                        script = null;
                    }

                    break;
                case NpcMessageType.YesNo:
                case NpcMessageType.AcceptDecline:
                case NpcMessageType.AcceptDeclineNoExit:
                    if (action == 0)
                    {
                        script.SetResult(0);
                        script.State++;
                    }
                    else if (action == 1)
                    {
                        script.SetResult(1);
                        script.State++;
                    }

                    break;
                case NpcMessageType.RequestText:
                    if (action != 0)
                    {
                        script.SetResult(packet.ReadString());
                        script.State++;
                    }

                    break;
                case NpcMessageType.RequestNumber:
                    if (action == 1)
                    {
                        script.SetResult(packet.ReadInt());
                        script.State++;
                    }

                    break;
                case NpcMessageType.Choice:
                case NpcMessageType.Quiz:
                case NpcMessageType.RequestStyle:
                    if (action != 0)
                    {
                        script.SetResult(packet.ReadInt());
                        script.State++;
                    }

                    break;
            }
        }

        public void AssignController()
        {
            if (Controller != null)
            {
                // Already has a controller
                return;
            }

            var leastControlled = int.MaxValue;
            Character newController = null;

            lock (Map.Characters)
            {
                foreach (var character in Map.Characters.Values.Where(x => x.Client.Connected))
                {
                    if (character.ControlledNpcs.Count >= leastControlled)
                    {
                        continue;
                    }

                    leastControlled = character.ControlledNpcs.Count;
                    newController = character;
                }
            }

            newController?.ControlledNpcs.Add(this);
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket() => GetInternalPacket(false);

        public PacketWriter GetControlRequestPacket() => GetInternalPacket(true);

        private PacketWriter GetInternalPacket(bool requestControl)
        {
            var pw = new PacketWriter(requestControl
                ? ServerOperationCode.NpcControlRequest
                : ServerOperationCode.NpcEnterField);

            if (requestControl)
            {
                pw.WriteBool(true);
            }

            pw.WriteInt(ObjectId);
            pw.WriteInt(MapleId);
            pw.WritePoint(Position);
            pw.WriteBool(!FacesLeft);
            pw.WriteShort(Foothold);
            pw.WriteShort(MinimumClickX);
            pw.WriteShort(MaximumClickX);

            return pw;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var pw = new PacketWriter(ServerOperationCode.NpcControlRequest);

            pw.WriteBool(false);
            pw.WriteInt(ObjectId);

            return pw;
        }

        public PacketWriter GetDialogPacket(NpcStateInfo stateInfo)
        {
            var pw = new PacketWriter(ServerOperationCode.NpcScriptChat);

            pw.WriteByte(4); // NOTE: Unknown.
            pw.WriteInt(MapleId);
            pw.WriteByte(stateInfo.Type);
            pw.WriteString(stateInfo.Text);

            switch (stateInfo.Type)
            {
                case NpcMessageType.Standard:
                    pw.WriteBool(stateInfo.IsPrevious);
                    pw.WriteBool(stateInfo.IsNext);
                    break;
                case NpcMessageType.RequestStyle:
                    stateInfo.Styles.ForEach(pw.WriteInt);
                    break;
                case NpcMessageType.RequestNumber:
                    pw.WriteInt(stateInfo.NumberDefault);
                    pw.WriteInt(stateInfo.NumberMinimum);
                    pw.WriteInt(stateInfo.NumberMaximum);
                    pw.WriteInt(0);
                    break;
                case NpcMessageType.RequestText:
                    pw.WriteInt(0);
                    pw.WriteInt(0);
                    break;
                case NpcMessageType.Quiz:
                    pw.WriteByte(0);
                    pw.WriteInt((int)stateInfo.NpcQuizType);
                    pw.WriteInt(stateInfo.QuizObjectId);
                    pw.WriteInt(stateInfo.QuizCorrect);
                    pw.WriteInt(stateInfo.QuizQuestions);
                    pw.WriteInt(stateInfo.QuizTime);
                    break;
                case NpcMessageType.AcceptDecline:
                case NpcMessageType.Choice:
                case NpcMessageType.YesNo:
                    break;
            }

            return pw;
        }

        public PacketWriter GetDestroyPacket()
        {
            var pw = new PacketWriter(ServerOperationCode.NpcLeaveField);

            pw.WriteInt(ObjectId);

            return pw;
        }
    }
}
