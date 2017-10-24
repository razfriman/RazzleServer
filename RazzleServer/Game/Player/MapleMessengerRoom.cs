using RazzleServer.Common.Packet;
using RazzleServer.Util;
using System.Collections.Generic;
using System.Linq;
using MapleLib.PacketLib;

namespace RazzleServer.Player
{
    public class MapleMessengerRoom
    {
        private static readonly AutoIncrement IDCounter = new AutoIncrement(1);

        public static readonly Dictionary<int, MapleMessengerRoom> Rooms = new Dictionary<int, MapleMessengerRoom>();

        public int ID { get; set; }
        public int Capacity { get; set; }


        private readonly Dictionary<int, MapleMessengerCharacter> Participants = new Dictionary<int, MapleMessengerCharacter>();

        public MapleMessengerRoom(int capacity)
        {
            ID = IDCounter.Get;
            Capacity = capacity;
            Rooms.Add(ID, this);
        }

        public static MapleMessengerRoom GetChatRoom(int id) => Rooms.TryGetValue(id, out MapleMessengerRoom ret) ? ret : null;

        public bool AddPlayer(MapleCharacter chr)
        {
            if (Participants.ContainsKey(chr.ID)) return false;
            int position = GetFreePosition();
            if (position == -1) return false; //No space
            MapleMessengerCharacter mcc = new MapleMessengerCharacter(position, chr);
            chr.ChatRoom = this;
            chr.Client.Send(Packets.EnterRoom((byte)position));
            var playerAddPacket = Packets.AddPlayer(mcc);
            BroadCastPacket(playerAddPacket, chr.ID);
            foreach (MapleMessengerCharacter participant in Participants.Values)
            {
                chr.Client.Send(Packets.AddPlayer(participant));
            }
            Participants.Add(chr.ID, mcc);
            return true;
        }

        public void RemovePlayer(int chrId)
        {
            MapleMessengerCharacter mcc;
            if (!Participants.TryGetValue(chrId, out mcc)) return;
            Participants.Remove(chrId);
            mcc.Character = null;
            if (!Participants.Any())
            {
                Rooms.Remove(ID);
                return;
            }
            var removePacket = Packets.PlayerLeft((byte)mcc.Position);
            BroadCastPacket(removePacket);
        }

        public MapleCharacter GetChatCharacterByName(string name) => Participants.Select(x => x.Value.Character).FirstOrDefault(x => x.Name == name);


        public void DoChat(int characterIdFrom, string message)
        {
            MapleMessengerCharacter mcc;
            if (!Participants.TryGetValue(characterIdFrom, out mcc)) return;
            var chatPacket = Packets.Chat(mcc.Character.Name, message);
            BroadCastPacket(chatPacket, characterIdFrom);
        }

        private int GetFreePosition()
        {
            bool[] currentPositions = new bool[Capacity];
            foreach (MapleMessengerCharacter mcc in Participants.Values)
            {
                currentPositions[mcc.Position] = true;
            }
            for (int i = 0; i < Capacity; i++)
            {
                if (!currentPositions[i])
                    return i;
            }
            return -1;
        }

        private void BroadCastPacket(PacketWriter packet, int characterSourceId = 0, bool repeatToSource = false)
        {
            foreach (MapleCharacter chr in Participants.Select(x => x.Value.Character))
            {
                if (repeatToSource || chr.ID != characterSourceId)
                {
                    chr.Client.Send(packet);
                }
            }
        }
        
        public static class Packets
        {
            public static PacketWriter EnterRoom(byte position)
            {
                var pw = new PacketWriter(ServerOperationCode.MESSENGER);
                pw.WriteByte(1);
                pw.WriteByte(position);
                return pw;
            }

            public static PacketWriter PlayerLeft(byte position)
            {
                var pw = new PacketWriter(ServerOperationCode.MESSENGER);
                pw.WriteByte(2);
                pw.WriteByte(position);
                return pw;
            }

            public static PacketWriter Chat(string characterName, string message)
            {
                var pw = new PacketWriter(ServerOperationCode.MESSENGER);
                pw.WriteByte(6);
                pw.WriteMapleString(characterName);
                pw.WriteMapleString(message);
                return pw;
            }

            public static PacketWriter AddPlayer(MapleMessengerCharacter mcc)
            {
                MapleCharacter player = mcc.Character;
                var pw = new PacketWriter(ServerOperationCode.MESSENGER);
                pw.WriteByte(0);
                pw.WriteByte((byte)mcc.Position);
                MapleCharacter.AddCharLook(pw, player, false);

                pw.WriteMapleString(player.Name);
                pw.WriteByte(player.Client.Channel);
                pw.WriteByte(1);
                pw.WriteShort(player.Job);

                return pw;
            }
        }
    }
}