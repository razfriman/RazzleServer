using System;
using System.IO;
using RazzleServer.Common.Wz.Util;

namespace RazzleServer.Common.Wz.WzProperties
{
	/// <summary>
	/// A property that's value is null
	/// </summary>
	public class WzNullProperty : WzImageProperty
	{
		#region Inherited Members
        public override void SetValue(object value)
        {
            throw new NotImplementedException();
        }

        public override WzImageProperty DeepClone()
        {
            var clone = new WzNullProperty(Name);
            return clone;
        }

		/// <summary>
		/// The WzPropertyType of the property
		/// </summary>
		public override WzPropertyType PropertyType => WzPropertyType.Null;

		/// <summary>
		/// The WzObjectType of the property
		/// </summary>
		public override WzObjectType ObjectType => WzObjectType.Property;

		public override void WriteValue(WzBinaryWriter writer)
		{
			writer.Write((byte)0);
		}
		
		/// <summary>
		/// Disposes the object
		/// </summary>
		public override void Dispose()
		{
            Name = null;
		}
		#endregion

		#region Custom Members
		/// <summary>
		/// Creates a blank WzNullProperty
		/// </summary>
		public WzNullProperty() { }
		/// <summary>
		/// Creates a WzNullProperty with the specified name
		/// </summary>
		/// <param name="propName">The name of the property</param>
		public WzNullProperty(string propName)
		{
            Name = propName;
		}
		#endregion

	}
}