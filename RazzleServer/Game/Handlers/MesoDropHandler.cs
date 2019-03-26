﻿using RazzleServer.Game.Maple.Items;
using RazzleServer.Net.Packet;

namespace RazzleServer.Game.Handlers
{
    [PacketHandler(ClientOperationCode.MesoDrop)]
    public class MesoDropHandler : GamePacketHandler
    {
        public override void HandlePacket(PacketReader packet, GameClient client)
        {
            var amount = packet.ReadInt();

            if (amount > client.Character.Meso || amount < 10 || amount > 50000)
            {
                return;
            }

            client.Character.Meso -= amount;

            var mesoDrop = new Meso(amount) {Dropper = client.Character, Owner = null};

            client.Character.Map.Drops.Add(mesoDrop);
        }
    }
}
