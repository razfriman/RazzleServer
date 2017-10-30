﻿using RazzleServer.Common.Constants;
using RazzleServer.Common.Data;
using RazzleServer.Common.Packet;
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
        public ReactorState[] States { get; set; }

        public Reactor CachedReference
        {
            get
            {
                return DataProvider.Reactors[this.MapleID];
            }
        }

        public Reactor(Datum datum)
            : base()
        {
            this.MapleID = (int)datum["reactorid"];
            this.Label = string.Empty; // TODO: Is this even relevant?
            this.State = 0;
            this.States = new ReactorState[(sbyte)datum["max_states"]];
        }

        public Reactor(int mapleID)
        {
            this.MapleID = mapleID;
            this.Label = this.CachedReference.Label;
            this.State = this.CachedReference.State;
            this.States = this.CachedReference.States;
        }

        public Reactor(SpawnPoint spawnPoint)
            : this(spawnPoint.MapleID)
        {
            this.SpawnPoint = spawnPoint;
            this.Position = spawnPoint.Position;
        }

        public void Hit(Character character, short actionDelay, int skillID)
        {
            ReactorState state = this.States[this.State];

            switch (state.Type)
            {
                case ReactorEventType.PlainAdvanceState:
                    {
                        this.State = state.NextState;

                        if (this.State == this.States.Length - 1) // TODO: Is this the correct way of doing this?
                        {
                            this.Map.Reactors.Remove(this);
                        }
                        else
                        {
                            using (PacketReader oPacket = new Packet(ServerOperationCode.ReactorChangeState))
                            {
                                oPacket
                                    .WriteInt(this.ObjectID)
                                    .WriteByte(this.State)
                                    .WriteShort(this.Position.X)
                                    .WriteShort(this.Position.Y)
                                    .WriteShort(actionDelay)
                                    .WriteByte() // NOTE: Event index.
                                    .WriteByte(4); // NOTE: Delay.

                                this.Map.Broadcast(oPacket);
                            }
                        }
                    }
                    break;
            }
        }

        public PacketWriter GetCreatePacket()
        {
            return this.GetSpawnPacket();
        }

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.ReactorEnterField);

            oPacket
                .WriteInt(this.ObjectID)
                .WriteInt(this.MapleID)
                .WriteByte(this.State)
                .WriteShort(this.Position.X)
                .WriteShort(this.Position.Y)
                .WriteShort() // NOTE: Flags (not sure).
                .WriteBool(false) // NOTE: Unknown
                .WriteString(this.Label);

            return oPacket;
        }

        public PacketWriter GetDestroyPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.ReactorLeaveField);

            oPacket
                .WriteInt(this.ObjectID)
                .WriteByte(this.State)
                .WriteShort(this.Position.X)
                .WriteShort(this.Position.Y);

            return oPacket;
        }
    }
}
