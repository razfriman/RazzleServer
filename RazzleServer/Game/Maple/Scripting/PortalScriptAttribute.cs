using System;

namespace RazzleServer.Game.Maple.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class PortalScriptAttribute : Attribute
    {
        public PortalScriptAttribute(string script) => Script = script;

        public string Script { get; }
    }
  
}
