using System;

namespace RazzleServer.Game.Maple.Scripting
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class ReactorScriptAttribute : Attribute
    {
        public ReactorScriptAttribute(string script) => Script = script;

        public string Script { get; }
    }
  
}
