using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using RazzleServer.Common.Network;
using Microsoft.Extensions.Logging;
using RazzleServer.Common.Packet;
using RazzleServer.Login.Maple;
using RazzleServer.Common.Util;

namespace RazzleServer.Login
{
    public sealed class LoginClient : AClient
    {
        public static Dictionary<ClientOperationCode, List<LoginPacketHandler>> PacketHandlers = new Dictionary<ClientOperationCode, List<LoginPacketHandler>>();

        public byte World { get; private set; }
        public byte Channel { get; private set; }
        public Account Account { get; private set; }
        public string LastUsername { get; private set; }
        public string LastPassword { get; private set; }
        public string[] MacAddresses { get; private set; }
        public LoginServer Server { get; private set; }

        public LoginClient(Socket socket, LoginServer server)
            : base(socket)
        {
            Server = server;
        }

        public static void RegisterPacketHandlers()
        {

            var types = Assembly.GetEntryAssembly()
                                .GetTypes()
                                .Where(x => x.IsSubclassOf(typeof(LoginPacketHandler)));

            var handlerCount = 0;

            foreach (var type in types)
            {
                var attributes = type.GetTypeInfo()
                                     .GetCustomAttributes()
                                     .OfType<PacketHandlerAttribute>()
                                     .ToList();

                foreach (var attribute in attributes)
                {
                    var header = attribute.Header;

                    if (!PacketHandlers.ContainsKey(header))
                    {
                        PacketHandlers[header] = new List<LoginPacketHandler>();
                    }

                    handlerCount++;
                    var handler = (LoginPacketHandler)Activator.CreateInstance(type);
                    PacketHandlers[header].Add(handler);
                    Log.LogDebug($"Registered Packet Handler [{attribute.Header}] to [{type.Name}]");
                }
            }

            Log.LogInformation($"Registered {handlerCount} packet handlers");
        }

        public override void Receive(PacketReader packet)
        {
            ClientOperationCode header = ClientOperationCode.UNKNOWN;
            try
            {
                if (packet.Available >= 2)
                {
                    header = (ClientOperationCode)packet.ReadUShort();

                    if (PacketHandlers.ContainsKey(header))
                    {
                        Log.LogInformation($"Recevied [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");

                        foreach (var handler in PacketHandlers[header])
                        {
                            handler.HandlePacket(packet, this);
                        }
                    }
                    else
                    {
                        Log.LogWarning($"Unhandled Packet [{header.ToString()}] {Functions.ByteArrayToStr(packet.ToArray())}");
                    }

                }
            }
            catch (Exception e)
            {
                Log.LogError(e, $"Packet Processing Error [{header.ToString()}] - {e.Message} - {e.StackTrace}");
            }
        }

        //private void SendLoginResult(LoginResult result)
        //{
        //    using (var oPacket = new PacketWriter(ServerOperationCode.CheckPasswordResult))
        //    {
        //        oPacket
        //            .WriteInt((int)result)
        //            .WriteByte()
        //            .WriteByte();

        //        if (result == LoginResult.Valid)
        //        {
        //            oPacket
        //                .WriteInt(this.Account.ID)
        //                .WriteByte((byte)this.Account.Gender)
        //                .WriteByte() // NOTE: Grade code.
        //                .WriteByte() // NOTE: Subgrade code.
        //                .WriteByte() // NOTE: Country code.
        //                .WriteString(this.Account.Username)
        //                .WriteByte() // NOTE: Unknown.
        //                .WriteByte() // NOTE: Quiet ban reason. 
        //                .WriteLong() // NOTE: Quiet ban lift date.
        //                .WriteDateTime(this.Account.Creation)
        //                .WriteInt() // NOTE: Unknown.
        //                .WriteByte((byte)(WvsLogin.RequestPin ? 0 : 2)) // NOTE: 1 seems to not do anything.
        //                .WriteByte((byte)(WvsLogin.RequestPic ? (string.IsNullOrEmpty(this.Account.Pic) ? 0 : 1) : 2));
        //        }

        //        this.Send(oPacket);
        //    }
        //}

        //private void EULA(PacketReader iPacket)
        //{
        //    bool accepted = iPacket.ReadBool();

        //    if (accepted)
        //    {
        //        this.Account.EULA = true;

        //        Datum datum = new Datum("accounts");

        //        datum["EULA"] = true;

        //        datum.Update("ID = {0}", this.Account.ID);

        //        this.SendLoginResult(LoginResult.Valid);
        //    }
        //    else
        //    {
        //        this.Stop(); // NOTE: I'm pretty sure in the real client it disconnects you if you refuse to accept the EULA.
        //    }
        //}

        //private void SetGender(PacketReader iPacket)
        //{
        //    if (this.Account.Gender != Gender.Unset)
        //    {
        //        return;
        //    }

        //    bool valid = iPacket.ReadBool();

        //    if (valid)
        //    {
        //        Gender gender = (Gender)iPacket.ReadByte();

        //        this.Account.Gender = gender;

        //        Datum datum = new Datum("accounts");

        //        datum["Gender"] = (byte)this.Account.Gender;

        //        datum.Update("ID = {0}", this.Account.ID);

        //        this.SendLoginResult(LoginResult.Valid);
        //    }
        //}

        //private void CheckPin(PacketReader iPacket)
        //{
        //    byte a = iPacket.ReadByte();
        //    byte b = iPacket.ReadByte();

        //    PinResult result;

        //    if (b == 0)
        //    {
        //        string pin = iPacket.ReadString();

        //        if (SHACryptograph.Encrypt(SHAMode.SHA256, pin) != this.Account.Pin)
        //        {
        //            result = PinResult.Invalid;
        //        }
        //        else
        //        {
        //            if (a == 1)
        //            {
        //                result = PinResult.Valid;
        //            }
        //            else if (a == 2)
        //            {
        //                result = PinResult.Register;
        //            }
        //            else
        //            {
        //                result = PinResult.Error;
        //            }
        //        }
        //    }
        //    else if (b == 1)
        //    {
        //        if (string.IsNullOrEmpty(this.Account.Pin))
        //        {
        //            result = PinResult.Register;
        //        }
        //        else
        //        {
        //            result = PinResult.Request;
        //        }
        //    }
        //    else
        //    {
        //        result = PinResult.Error;
        //    }

        //    using (var oPacket = new PacketWriter(ServerOperationCode.CheckPinCodeResult))
        //    {
        //        oPacket.WriteByte((byte)result);

        //        this.Send(oPacket);
        //    }
        //}

        //private void UpdatePin(PacketReader iPacket)
        //{
        //    bool procceed = iPacket.ReadBool();
        //    string pin = iPacket.ReadString();

        //    if (procceed)
        //    {
        //        this.Account.Pin = SHACryptograph.Encrypt(SHAMode.SHA256, pin);

        //        Datum datum = new Datum("accounts");

        //        datum["Pin"] = this.Account.Pin;

        //        datum.Update("ID = {0}", this.Account.ID);

        //        using (var oPacket = new PacketWriter(ServerOperationCode.UpdatePinCodeResult))
        //        {
        //            oPacket.WriteByte(); // NOTE: All the other result types end up in a "trouble logging into the game" message.

        //            this.Send(oPacket);
        //        }
        //    }
        //}

        //private void ListWorlds()
        //{
        //    foreach (World world in WvsLogin.Worlds)
        //    {
        //        using (var oPacket = new PacketWriter(ServerOperationCode.WorldInformation))
        //        {
        //            oPacket
        //                .WriteByte(world.ID)
        //                .WriteString(world.Name)
        //                .WriteByte((byte)world.Flag)
        //                .WriteString(world.EventMessage)
        //                .WriteShort(100) // NOTE: Event EXP rate
        //                .WriteShort(100) // NOTE: Event Drop rate
        //                .WriteBool(false) // NOTE: Character creation disable.
        //                .WriteByte((byte)world.Count);

        //            foreach (Channel channel in world)
        //            {
        //                oPacket
        //                    .WriteString($"{world.Name}-{channel.ID}")
        //                    .WriteInt(channel.Population)
        //                    .WriteByte(1)
        //                    .WriteByte(channel.ID)
        //                    .WriteBool(false); // NOTE: Adult channel.
        //            }

        //            //TODO: Add login balloons. These are chat bubbles shown on the world select screen
        //            oPacket.WriteShort(); //balloon count
        //                                  //foreach (var balloon in balloons)
        //                                  //{
        //                                  //    oPacket
        //                                  //        .WriteShort(balloon.X)
        //                                  //        .WriteShort(balloon.Y)
        //                                  //        .WriteString(balloon.Text);
        //                                  //}

        //            this.Send(oPacket);
        //        }

        //        using (var oPacket = new PacketWriter(ServerOperationCode.WorldInformation))
        //        {
        //            oPacket.WriteByte(byte.MaxValue);

        //            this.Send(oPacket);
        //        }

        //        // TODO: Last connected world. Get this from the database. Set the last connected world once you succesfully load a character.
        //        using (var oPacket = new PacketWriter(ServerOperationCode.LastConnectedWorld))
        //        {
        //            oPacket.WriteInt(); // NOTE: World ID.

        //            this.Send(oPacket);
        //        }

        //        // TODO: Recommended worlds. Get this from configuration.
        //        using (var oPacket = new PacketWriter(ServerOperationCode.RecommendedWorldMessage))
        //        {
        //            oPacket
        //                .WriteByte(1) // NOTE: Count.
        //                .WriteInt() // NOTE: World ID.
        //                .WriteString("Check out Scania! The best world to play - and not because it's the only one available... hehe."); // NOTE: Message.

        //            this.Send(oPacket);
        //        }
        //    }
        //}

        //private void InformWorldStatus(PacketReader iPacket)
        //{
        //    byte worldID = iPacket.ReadByte();

        //    World world;

        //    try
        //    {
        //        world = WvsLogin.Worlds[worldID];
        //    }
        //    catch (KeyNotFoundException)
        //    {
        //        return;
        //    }

        //    using (var oPacket = new PacketWriter(ServerOperationCode.CheckUserLimitResult))
        //    {
        //        oPacket.WriteShort((short)world.Status);

        //        this.Send(oPacket);
        //    }
        //}

        //private void SelectWorld(PacketReader iPacket)
        //{
        //    iPacket.ReadByte(); // NOTE: Connection kind (GameLaunching, WebStart, etc.).
        //    this.World = iPacket.ReadByte();
        //    this.Channel = iPacket.ReadByte();
        //    iPacket.ReadBytes(4); // NOTE: IPv4 Address.

        //    List<byte[]> characters = WvsLogin.CenterConnection.GetCharacters(this.World, this.Account.ID);

        //    using (var oPacket = new PacketWriter(ServerOperationCode.SelectWorldResult))
        //    {
        //        oPacket
        //            .WriteBool(false)
        //            .WriteByte((byte)characters.Count);

        //        foreach (byte[] characterBytes in characters)
        //        {
        //            oPacket.WriteBytes(characterBytes);
        //        }

        //        oPacket
        //            .WriteByte((byte)(WvsLogin.RequestPic ? (string.IsNullOrEmpty(this.Account.Pic) ? 0 : 1) : 2))
        //            .WriteInt(this.Account.MaxCharacters);

        //        this.Send(oPacket);
        //    }
        //}

        //private void ViewAllChar(PacketReader iPacket)
        //{
            //if (this.IsInViewAllChar)
            //{
            //    using (var oPacket = new PacketWriter(ServerOperationCode.ViewAllCharResult))
            //    {
            //        oPacket
            //            .WriteByte((byte)VACResult.UnknownError)
            //            .WriteByte();

            //        this.Send(oPacket);
            //    }

            //    return;
            //}

            //this.IsInViewAllChar = true;

            //List<Character> characters = new List<Character>();

            //foreach (Datum datum in new Datums("characters").PopulateWith("ID", "AccountID = {0}", this.Account.ID))
            //{
            //    Character character = new Character((int)datum["ID"], this);

            //    character.Load();

            //    characters.Add(character);
            //}

            //using (var oPacket = new PacketWriter(ServerOperationCode.ViewAllCharResult))
            //{
            //    if (characters.Count == 0)
            //    {
            //        oPacket
            //            .WriteByte((byte)VACResult.NoCharacters);
            //    }
            //    else
            //    {
            //        oPacket
            //            .WriteByte((byte)VACResult.SendCount)
            //            .WriteInt(MasterServer.Worlds.Length)
            //            .WriteInt(characters.Count);
            //    }

            //    this.Send(oPacket);
            //}

            //foreach (WorldServer world in MasterServer.Worlds)
            //{
            //    using (var oPacket = new PacketWriter(ServerOperationCode.ViewAllCharResult))
            //    {
            //        IEnumerable<Character> worldChars = characters.Where(x => x.WorldID == world.ID);

            //        oPacket
            //            .WriteByte((byte)VACResult.CharInfo)
            //            .WriteByte(world.ID)
            //            .WriteByte((byte)worldChars.Count());

            //        foreach (Character character in worldChars)
            //        {
            //            oPacket.WriteBytes(character.ToByteArray());
            //        }

            //        this.Send(oPacket);
            //    }
            //}
        //}

        //private void SetViewAllChar(PacketReader iPacket)
        //{
        //    this.IsInViewAllChar = iPacket.ReadBool();
        //}

        //private void CheckCharacterName(PacketReader iPacket)
        //{
        //    string name = iPacket.ReadString();
        //    bool unusable = WvsLogin.CenterConnection.IsNameTaken(name);

        //    using (var oPacket = new PacketWriter(ServerOperationCode.CheckDuplicatedIDResult))
        //    {
        //        oPacket
        //            .WriteString(name)
        //            .WriteBool(unusable);

        //        this.Send(oPacket);
        //    }
        //}

        //private void CreateCharacter(PacketReader iPacket)
        //{
        //    byte[] characterData = iPacket.ReadBytes();

        //    using (var outPacket = new PacketWriter(ServerOperationCode.CreateNewCharacterResult))
        //    {
        //        outPacket.WriteByte(); // NOTE: 1 for failure. Could be implemented as anti-packet editing.
        //        outPacket.WriteBytes(WvsLogin.CenterConnection.CreateCharacter(this.World, this.Account.ID, characterData));

        //        this.Send(outPacket);
        //    }
        //}

        //// TODO: Proper character deletion with all the necessary checks (cash items, guilds, etcetera). 
        //private void DeleteCharacter(PacketReader iPacket)
        //{
        //    string pic = iPacket.ReadString();
        //    int characterID = iPacket.ReadInt();

        //    CharacterDeletionResult result;

        //    if (SHACryptograph.Encrypt(SHAMode.SHA256, pic) == this.Account.Pic || !WvsLogin.RequestPic)
        //    {
        //        //NOTE: As long as foreign keys are set to cascade, all child entries related to this CharacterID will also be deleted.
        //        Database.Delete("characters", "ID = {0}", characterID);

        //        result = CharacterDeletionResult.Valid;
        //    }
        //    else
        //    {
        //        result = CharacterDeletionResult.InvalidPic;
        //    }

        //    using (var oPacket = new PacketWriter(ServerOperationCode.DeleteCharacterResult))
        //    {
        //        oPacket
        //            .WriteInt(characterID)
        //            .WriteByte((byte)result);

        //        this.Send(oPacket);
        //    }
        //}

        //private void SelectCharacter(PacketReader iPacket, bool fromViewAll = false, bool requestPic = false, bool registerPic = false)
        //{
        //    string pic = string.Empty;

        //    if (requestPic)
        //    {
        //        pic = iPacket.ReadString();
        //    }
        //    else if (registerPic)
        //    {
        //        iPacket.ReadByte();
        //    }

        //    int characterID = iPacket.ReadInt();

        //    //if (this.IsInViewAllChar)
        //    //{
        //    //    this.WorldID = (byte)iPacket.ReadInt();
        //    //    this.ChannelID = 0; // TODO: Least loaded channel.
        //    //}

        //    this.MacAddresses = iPacket.ReadString().Split(new char[] { ',', ' ' });

        //    if (registerPic)
        //    {
        //        iPacket.ReadString();
        //        pic = iPacket.ReadString();

        //        if (string.IsNullOrEmpty(this.Account.Pic))
        //        {
        //            this.Account.Pic = SHACryptograph.Encrypt(SHAMode.SHA256, pic);

        //            Datum datum = new Datum("accounts");

        //            datum["Pic"] = this.Account.Pic;

        //            datum.Update("ID = {0}", this.Account.ID);
        //        }
        //    }

        //    if (!requestPic || SHACryptograph.Encrypt(SHAMode.SHA256, pic) == this.Account.Pic)
        //    {
        //        if (!WvsLogin.CenterConnection.Migrate(this.RemoteEndPoint.Address.ToString(), this.Account.ID, characterID))
        //        {
        //            this.Stop();

        //            return;
        //        }

        //        using (var oPacket = new PacketWriter(ServerOperationCode.SelectCharacterResult))
        //        {
        //            oPacket
        //                .WriteByte()
        //                .WriteByte()
        //                .WriteBytes(127, 0, 0, 1)
        //                .WriteUShort(WvsLogin.Worlds[this.World][this.Channel].Port)
        //                .WriteInt(characterID)
        //                .WriteInt()
        //                .WriteByte();

        //            this.Send(oPacket);
        //        }
        //    }
        //    else
        //    {
        //        using (var oPacket = new PacketWriter(ServerOperationCode.CheckSPWResult))
        //        {
        //            oPacket.WriteByte();

        //            this.Send(oPacket);
        //        }
        //    }
        //}
    }
}
