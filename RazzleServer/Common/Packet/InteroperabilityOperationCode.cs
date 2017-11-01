namespace RazzleServer.Common.Packet
{
    public enum InteroperabilityOperationCode : ushort
    {
        Unknown,
        RegistrationRequest,
        RegistrationResponse,
        UpdateChannel,
        UpdateChannelID,
        UpdateChannelPopulation,
        CharacterEntriesRequest,
        CharacterEntriesResponse,
        MigrationRegisterRequest,
        MigrationRegisterResponse,
        MigrationRequest,
        MigrationResponse,
        ChannelPortRequest,
        ChannelPortResponse
    }
}
