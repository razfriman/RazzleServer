using System;
using RazzleServer.Common.Constants;
using RazzleServer.Login.Maple;
using RazzleServer.Net.Packet;
using RazzleServer.Server;

namespace RazzleServer.Login
{
    public static class LoginPackets
    {
        public static PacketWriter LoginResult(LoginResult result, LoginAccount acc)
        {
            using var pw = new PacketWriter(ServerOperationCode.CheckPasswordResult);
            pw.WriteByte(result);
            pw.WriteByte(0);
            pw.WriteInt(0);

            switch (result)
            {
                case Common.Constants.LoginResult.Banned:
                    pw.WriteByte(acc.BanReason);
                    pw.WriteDateTime(DateConstants.PermanentBanDate);
                    break;
                case Common.Constants.LoginResult.Valid:
                    pw.WriteInt(acc.Id);
                    pw.WriteByte((int)acc.Gender);
                    pw.WriteBool(acc.IsMaster);
                    pw.WriteByte(1);
                    pw.WriteString(acc.Username);
                    break;
                case Common.Constants.LoginResult.InvalidPassword:
                case Common.Constants.LoginResult.InvalidUsername:
                case Common.Constants.LoginResult.LoggedIn:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }

            pw.WriteLong(0);
            pw.WriteLong(0);
            pw.WriteLong(0);

            return pw;
        }

        public static PacketWriter ListWorlds(AWorlds worlds)
        {
            using var pw = new PacketWriter(ServerOperationCode.WorldInformation);
            foreach (var world in worlds.Values)
            {
                pw.WriteByte(world.Id);
                pw.WriteString(world.Name);
                pw.WriteByte(world.Count);

                for (short i = 0; i < world.Count; i++)
                {
                    pw.WriteString($"{world.Name}-{i}");
                    pw.WriteInt(world.Population);
                    pw.WriteByte(world.Id);
                    pw.WriteShort(i);
                }
            }

            return pw;
        }

        public static PacketWriter EndListWorlds()
        {
            using var pw = new PacketWriter(ServerOperationCode.WorldInformation);
            pw.WriteByte(0xFF);
            pw.WriteByte(0);
            return pw;
        }
    }
}
