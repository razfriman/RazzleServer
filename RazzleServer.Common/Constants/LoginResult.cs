namespace RazzleServer.Common.Constants
{
    public enum LoginResult : byte
    {
        Valid = 0,
        Banned = 3,
        InvalidPassword = 4,
        InvalidUsername = 5,
        LoggedIn = 7
    }
}
