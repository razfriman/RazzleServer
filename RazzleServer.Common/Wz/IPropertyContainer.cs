using System.Collections.Generic;

namespace RazzleServer.Common.Wz
{
	public interface IPropertyContainer
	{
		void AddProperty(WzImageProperty prop);
		void AddProperties(IEnumerable<WzImageProperty> props);
		void RemoveProperty(WzImageProperty prop);
		void ClearProperties();
        List<WzImageProperty> WzProperties { get; }
        WzImageProperty this[string name] { get; set; }
	}
}