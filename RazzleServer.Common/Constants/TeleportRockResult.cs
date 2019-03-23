namespace RazzleServer.Common.Constants
{
    public enum TeleportRockResult : byte
    {
        Success = 1,
        Delete = 2,
        Add = 3,
        CannotGo2 = 5,
        DifficultToLocate = 6,
        DifficultToLocate2 = 7,
        CannotGo = 8,
        AlreadyThere = 9,
        CannotSaveMap = 10,
        CannotLeaveMapleIsland = 11
    }
}
