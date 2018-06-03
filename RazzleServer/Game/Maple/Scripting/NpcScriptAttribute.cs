using System;

namespace RazzleServer.Game.Maple.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class NpcScriptAttribute : Attribute
    {
        public NpcScriptAttribute(string script)
        {
            Script = script;
        }

        public NpcScriptAttribute(int npcId)
        {
            Script = npcId.ToString();
        }

        public string Script { get; }
    }
  
}