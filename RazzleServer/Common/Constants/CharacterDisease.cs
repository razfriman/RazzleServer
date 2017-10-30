namespace RazzleServer.Common.Constants
{
    public enum CharacterDisease : long
    {
        None,
        Slow = 0x1,
        Seduce = 0x80,
        Fishable = 0x100,
        Confuse = 0x80000,
        Stun = 0x2000000000000,
        Poison = 0x4000000000000,
        Sealed = 0x8000000000000,
        Darkness = 0x10000000000000,
        Weaken = 0x4000000000000000
    }
}
