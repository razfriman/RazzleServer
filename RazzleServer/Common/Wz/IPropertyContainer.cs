using System.Collections.Generic;

namespace MapleLib.WzLib
{
	public interface IPropertyContainer
	{
		void AddProperty(WzImageProperty prop);
		void AddProperties(List<WzImageProperty> props);
		void RemoveProperty(WzImageProperty prop);
		void ClearProperties();
        List<WzImageProperty> WzProperties { get; }
        WzImageProperty this[string name] { get; set; }
	}
}