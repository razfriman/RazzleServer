using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Maps;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Reactor : MapObject, ISpawnable
    {
        public int MapleId { get; }
        public string Label { get; }
        public byte State { get; set; }
        public SpawnPoint SpawnPoint { get; }
        public List<ReactorState> States { get; set; } = new List<ReactorState>();
        public ReactorReference CachedReference => DataProvider.Reactors.Data[MapleId];
       
        public Reactor(int mapleId)
        {
            MapleId = mapleId;
            Label = CachedReference.Label;
            State = CachedReference.State;
            States = CachedReference.States;
        }

        public Reactor(SpawnPoint spawnPoint)
            : this(spawnPoint.MapleId)
        {
            SpawnPoint = spawnPoint;
            Position = spawnPoint.Position;
        }

        public void Hit(Character character, short actionDelay)
        {
            var state = States[State];

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
                                oPacket.WriteInt(ObjectId);
                                oPacket.WriteByte(State);
                                oPacket.WritePoint(Position);
                                oPacket.WriteShort(actionDelay);
                                oPacket.WriteByte(0); // NOTE: Event index.
                                oPacket.WriteByte(4); // NOTE: Delay.

                                Map.Send(oPacket);
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

            oPacket.WriteInt(ObjectId);
            oPacket.WriteInt(MapleId);
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

            oPacket.WriteInt(ObjectId);
            oPacket.WriteByte(State);
            oPacket.WritePoint(Position);

            return oPacket;
        }
    }
}
