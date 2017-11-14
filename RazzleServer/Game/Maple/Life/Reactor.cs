using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
using RazzleServer.Common.WzLib;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Reactor : MapObject, ISpawnable
    {
        public int MapleID { get; private set; }
        public string Label { get; private set; }
        public byte State { get; set; }
        public SpawnPoint SpawnPoint { get; private set; }
        public List<ReactorState> States { get; set; } = new List<ReactorState>();

        public Reactor CachedReference => DataProvider.Reactors[MapleID];

        public Reactor(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }
            MapleID = id;
            Label = img["action"]?.GetString();

            foreach (var state in img.WzProperties)
            {
                if (int.TryParse(state.Name, out var stateNumber))
                {
                    States.Add(new ReactorState(state));
                }
            }
        }

        public Reactor(int mapleID)
        {
            MapleID = mapleID;
            Label = CachedReference.Label;
            State = CachedReference.State;
            States = CachedReference.States;
        }

        public Reactor(SpawnPoint spawnPoint)
            : this(spawnPoint.MapleID)
        {
            SpawnPoint = spawnPoint;
            Position = spawnPoint.Position;
        }

        public void Hit(Character character, short actionDelay)
        {
            ReactorState state = States[State];

            // TODO - Reactor scripts
            switch (state.Type)
            {
                case ReactorEventType.PlainAdvanceState:
                    {
                        State = state.NextState;

                        if (State == States.Count - 1)
                        {
                            Map.Reactors.Remove(this);
                        }
                        else
                        {
                            using (var oPacket = new PacketWriter(ServerOperationCode.ReactorChangeState))
                            {
                                oPacket.WriteInt(ObjectID);
                                oPacket.WriteByte(State);
                                oPacket.WritePoint(Position);
                                oPacket.WriteShort(actionDelay);
                                oPacket.WriteByte(0); // NOTE: Event index.
                                oPacket.WriteByte(4); // NOTE: Delay.

                                Map.Broadcast(oPacket);
                            }
                        }
                    }
                    break;
            }
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.ReactorEnterField);

            oPacket.WriteInt(ObjectID);
            oPacket.WriteInt(MapleID);
            oPacket.WriteByte(State);
            oPacket.WritePoint(Position);
            oPacket.WriteShort(0); // NOTE: Flags (not sure).
            oPacket.WriteBool(false); // NOTE: Unknown
            oPacket.WriteString(Label);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.ReactorLeaveField);

            oPacket.WriteInt(ObjectID);
            oPacket.WriteByte(State);
            oPacket.WritePoint(Position);

            return oPacket;
        }
    }
}
