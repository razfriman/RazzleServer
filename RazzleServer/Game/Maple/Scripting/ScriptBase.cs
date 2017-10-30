using RazzleServer.Common.Constants;
using RazzleServer.Game.Maple.Characters;

namespace RazzleServer.Game.Maple.Scripting
{
    public abstract class ScriptBase : LuaScriptable
    {
        protected Character mCharacter;

        protected ScriptBase(ScriptType type, string name, Character character, bool useThread)
            : base(string.Format(Application.ExecutablePath + @"..\..\scripts\{0}\{1}.lua", type.ToString().ToLower(), name), useThread)
        {
            mCharacter = character;
        }
    }
}
