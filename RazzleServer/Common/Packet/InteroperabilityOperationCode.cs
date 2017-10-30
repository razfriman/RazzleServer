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
        CharacterCreationRequest,
        CharacterCreationResponse,
        MigrationRegisterRequest,
        MigrationRegisterResponse,
        CharacterNameCheckRequest,
        CharacterNameCheckResponse,
        MigrationRequest,
        MigrationResponse,
        ChannelPortRequest,
        ChannelPortResponse
    }
}
