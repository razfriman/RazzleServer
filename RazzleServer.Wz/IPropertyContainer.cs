using System.Collections.Generic;

namespace RazzleServer.Wz
{
    public interface IPropertyContainer
    {
        void AddProperty(WzImageProperty prop);
        void AddProperties(IEnumerable<WzImageProperty> props);
        void RemoveProperty(WzImageProperty prop);
        void ClearProperties();
        Dictionary<string, WzImageProperty> WzProperties { get; }
        WzImageProperty this[string name] { get; set; }
    }
}
