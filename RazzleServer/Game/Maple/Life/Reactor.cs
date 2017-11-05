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
        public ReactorState[] States { get; set; }

        public Reactor CachedReference => DataProvider.Reactors[MapleID];

        public Reactor(WzImage img)
        {
            var name = img.Name.Remove(7);
            if (!int.TryParse(name, out var id))
            {
                return;
            }
            MapleID = id;

            /*
               <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
              <imgdir name="1020001.img">
                  <imgdir name="info">
                      <string name="info" value="91020000"/>
                      <string name="link" value="1020000"/>
                  </imgdir>
                  <string name="action" value="s4hitmanMap1"/>
              </imgdir>
               */   



                /*
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <imgdir name="2221003.img">

                    <imgdir name="info">
                        <string name="info" value="흥부네 지붕:흥부의 박씨를 떨어뜨려 박을 소환한다."/>
                    </imgdir>


                    <imgdir name="0">
                        <canvas name="0" width="1" height="1">
                            <vector name="origin" x="0" y="0"/>
                        </canvas>
                        <imgdir name="event">
                            <imgdir name="0">
                                <int name="type" value="100"/>
                                <int name="state" value="1"/>
                                <int name="0" value="4031244"/>
                                <int name="1" value="1"/>
                                <int name="2" value="1"/>
                                <vector name="lt" x="-150" y="-48"/>
                                <vector name="rb" x="155" y="41"/>
                            </imgdir>
                        </imgdir>
                    </imgdir>


                    <imgdir name="1">
                        <canvas name="0" width="1" height="1">
                            <vector name="origin" x="0" y="0"/>
                        </canvas>
                    </imgdir>


                    <string name="action" value="fvquest0"/>
                </imgdir>
                                */
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

        public void Hit(Character character, short actionDelay, int skillID)
        {
            ReactorState state = States[State];

            switch (state.Type)
            {
                case ReactorEventType.PlainAdvanceState:
                    {
                        State = state.NextState;

                        if (State == States.Length - 1) // TODO: Is this the correct way of doing this?
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
