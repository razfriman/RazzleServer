using System;
using System.Collections.Generic;
using RazzleServer.Center.Maple;
using RazzleServer.Common.Constants;
using RazzleServer.Common.Packet;
using RazzleServer.Game.Maple.Characters;
using RazzleServer.Login.Maple;

namespace RazzleServer.Login
{
    public static class LoginPackets
    {
        public static PacketWriter SendLoginResult(LoginResult result, Account acc)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CheckPasswordResult))
            {
                pw.WriteShort((short)result);
                pw.WriteInt(0);

                if (result == LoginResult.Banned)
                {
                    pw.WriteByte(1); // ban reason
                    pw.WriteDateTime(DateTime.Now.AddYears(2)); // ban expiration time. Over 2 years = permanent
                }
                else if (result == LoginResult.Valid)
                {
                    pw.WriteInt(acc.Id);
                    pw.WriteByte((int)acc.Gender); // set gender // pin select
                    pw.WriteBool(acc.IsMaster);
                    pw.WriteByte(0); // 0x80 == usergm == gmlevel 5
                    // pw.WriteByte(0); // Country code
                    pw.WriteString(acc.Username);
                    pw.WriteByte(0);
                    pw.WriteByte(0); // quiet ban reason
                    pw.WriteLong(0); // quiet ban time
                    pw.WriteDateTime(acc.Creation);
                    pw.WriteInt(0);
                }

                return pw;
            }
        }

        internal static PacketWriter DeleteCharacterResult(int characterId, CharacterDeletionResult result)
        {
            using (var pw = new PacketWriter(ServerOperationCode.DeleteCharacterResult))
            {
                pw.WriteInt(characterId);
                pw.WriteByte((byte)result);
                return pw;
            }
        }

        internal static PacketWriter PinResult(PinResult result)
        {
            using (var pw = new PacketWriter(ServerOperationCode.PinCodeOperation))
            {
                pw.WriteByte((byte)result);
                return pw;
            }
        }

        internal static PacketWriter SelectCharacterResult(int characterId, byte[] host, ushort port)
        {
            using (var pw = new PacketWriter(ServerOperationCode.SelectCharacterResult))
            {
                pw.WriteByte(0);
                pw.WriteByte(0);
                pw.WriteBytes(host);
                pw.WriteUShort(port);
                pw.WriteInt(characterId);
                pw.WriteInt(0);
                pw.WriteByte(0);
                return pw;
            }
        }

        public static PacketWriter SelectWorld(bool channelExists, List<Character> characters, Account account)
        {
            using (var pw = new PacketWriter(ServerOperationCode.SelectWorldResult))
            {
                if (!channelExists)
                {
                    pw.WriteByte(8); // Channel offline
                    return pw;
                }

                pw.WriteByte(0);
                pw.WriteByte((byte)characters.Count);
                characters.ForEach(x => pw.WriteBytes(x.ToByteArray()));
                pw.WriteInt(account.MaxCharacters);
                return pw;
            }
        }

        public static PacketWriter SendServerListEnd()
        {
            using (var pw = new PacketWriter(ServerOperationCode.WorldInformation))
            {
                pw.WriteByte(0xFF);
                pw.WriteByte(0);
                return pw;
            }
        }

        public static PacketWriter SendServerList(Worlds worlds)
        {
            using (var pw = new PacketWriter(ServerOperationCode.WorldInformation))
            {
                foreach (var world in worlds)
                {
                    pw.WriteByte(world.Id);
                    pw.WriteString(world.Name);
                    pw.WriteByte((byte)world.Flag);
                    pw.WriteString(world.EventMessage);
                    pw.WriteShort(world.EventExperienceRate);
                    pw.WriteShort(world.EventDropRate);
                    pw.WriteBool(!world.EnableCharacterCreation);
                    pw.WriteByte(world.Count);

                    for (short i = 0; i < world.Count; i++)
                    {
                        pw.WriteString($"{world.Name}-{i}");
                        pw.WriteInt(world.Population);
                        pw.WriteByte(world.Id);
                        pw.WriteShort(i);
                    }
                }

                pw.WriteShort(0); // login balloons count (format: writePoint, writeString)
                return pw;
            }
        }

        internal static PacketWriter CreateNewCharacterResult(bool error, Character character)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CreateNewCharacterResult))
            {
                pw.WriteBool(error);
                pw.WriteBytes(character.ToByteArray());
                return pw;
            }
        }

        public static PacketWriter ReloginResult()
        {
            using (var pw = new PacketWriter(ServerOperationCode.ReloginResponse))
            {
                pw.WriteByte(1);
                return pw;
            }
        }

        public static PacketWriter CharacterNameResult(string name, bool characterExists)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CheckCharacterNameResult))
            {
                pw.WriteString(name);
                pw.WriteBool(characterExists);
                return pw;
            }
        }

        public static PacketWriter SendWorldStatus(World world)
        {
            using (var pw = new PacketWriter(ServerOperationCode.CheckUserLimitResult))
            {
                pw.WriteShort((short)world.Flag);
                return pw;
            }
        }
    }
}