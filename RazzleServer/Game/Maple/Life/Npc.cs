using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Shops;
using RazzleServer.Game.Maple.Util;

namespace RazzleServer.Game.Maple.Life
{
    public class Npc : LifeObject, ISpawnable, IControllable
    {
        [JsonIgnore]
        public Character Controller { get; set; }

        [JsonIgnore]
        public Shop Shop => DataProvider.Shops.Data.GetValueOrDefault(MapleId);

        [JsonIgnore]
        public NpcReference CachedReference => DataProvider.Npcs.Data[MapleId];

        private readonly ILogger _log = LogManager.Log;

        public Npc() { }

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
                talker.CurrentNpcShop = Shop;
                Shop.Show(talker);
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
                    else
                    {
                        script = null;
                        return;
                    }
                    break;
                case NpcMessageType.RequestText:
                    if (action != 0)
                    {
                        script.SetResult(packet.ReadString());
                        script.State++;
                    }
                    else
                    {
                        script = null;
                        return;
                    }
                    break;
                case NpcMessageType.RequestNumber:
                    if (action == 1)
                    {
                        script.SetResult(packet.ReadInt());
                        script.State++;
                    }
                    else
                    {
                        script = null;
                        return;
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
                    else
                    {
                        script = null;
                        return;
                    }
                    break;
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
                    foreach (var character in Map.Characters.Values.Where(x => x.Client.Connected))
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
            oPacket.WriteBool(!Hide);

            return oPacket;
        }

        public PacketWriter GetControlCancelPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.NpcChangeController);

            oPacket.WriteBool(false);
            oPacket.WriteInt(ObjectId);

            return oPacket;
        }

        public PacketWriter GetDialogPacket(NpcStateInfo stateInfo)
        {
            var pw = new PacketWriter(ServerOperationCode.ScriptMessage);

            pw.WriteByte(4); // NOTE: Unknown.
            pw.WriteInt(MapleId);
            pw.WriteByte((byte)stateInfo.Type);
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
            var oPacket = new PacketWriter(ServerOperationCode.NpcLeaveField);

            oPacket.WriteInt(ObjectId);

            return oPacket;
        }
    }
}
