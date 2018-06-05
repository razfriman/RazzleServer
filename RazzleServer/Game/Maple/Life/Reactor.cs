using System.Collections.Generic;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Game.Maple.Data;
using RazzleServer.Game.Maple.Data.References;
using RazzleServer.Game.Maple.Maps;
using RazzleServer.Game.Maple.Scripting;
using RazzleServer.Game.Maple.Util;

namespace RazzleServer.Game.Maple.Life
{
    public sealed class Reactor : MapObject, ISpawnable
    {
        public int MapleId { get; }
        public sbyte State { get; set; }
        public SpawnPoint SpawnPoint { get; }
        public ReactorReference CachedReference => DataProvider.Reactors.Data[MapleId];

        public Reactor(SpawnPoint spawnPoint)
        {
            MapleId = spawnPoint.MapleId;
            SpawnPoint = spawnPoint;
            Position = spawnPoint.Position;
        }

        public ReactorStateReference CurrentState =>
        CachedReference.States.GetValueOrDefault(State);

        public void Hit(Character character, short actionDelay)
        {
            var currentState = CurrentState;

            switch (currentState.Type)
            {
                case ReactorEventType.Dummy:
                    System.Console.WriteLine("Dummy reactor");
                    Map.Send(TriggerReactorPacket(actionDelay));
                    ScriptProvider.Reactors.Execute(this, character);
                    break;
                case ReactorEventType.HitFromLeft:
                case ReactorEventType.HitFromRight:
                case ReactorEventType.PlainAdvanceState:
                    
                    State = currentState.NextState;
                    var nextState = CurrentState;

                    if (nextState == null)
                    {
                        // Reactor is destroyed
                        if (currentState.Type != ReactorEventType.HitByItem)
                        {
                            if (actionDelay > 0)
                            {
                                Map.Reactors.Remove(ObjectId);
                            }
                            else
                            {
                                Map.Send(TriggerReactorPacket(actionDelay));
                            }
                        }
                        else
                        {
                            // Item triggered on final step
                            Map.Send(TriggerReactorPacket(actionDelay));
                        }
                        System.Console.WriteLine("Script on last state");
                        ScriptProvider.Reactors.Execute(this, character);
                    }
                    else
                    {
                        Map.Send(TriggerReactorPacket(actionDelay));
                        if (currentState.State == currentState.NextState)
                        {
                            // Looping reactor
                            System.Console.WriteLine("Looping reactor");
                            ScriptProvider.Reactors.Execute(this, character);
                        }
                    }
                    break;
            }
        }

        private PacketWriter TriggerReactorPacket(short actionDelay)
        {
            var oPacket = new PacketWriter(ServerOperationCode.ReactorChangeState);
            oPacket.WriteInt(ObjectId);
            oPacket.WriteByte(State);
            oPacket.WritePoint(Position);
            oPacket.WriteShort(actionDelay);
            oPacket.WriteByte(0); // NOTE: Event index.
            oPacket.WriteByte(4); // NOTE: Delay.
            return oPacket;
        }

        public PacketWriter GetCreatePacket() => GetSpawnPacket();

        public PacketWriter GetSpawnPacket()
        {
            var oPacket = new PacketWriter(ServerOperationCode.ReactorEnterField);
            oPacket.WriteInt(ObjectId);
            oPacket.WriteInt(MapleId);
            oPacket.WriteByte(State);
            oPacket.WritePoint(Position);
            oPacket.WriteByte(0);
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
