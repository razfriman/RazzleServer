namespace RazzleServer.Wz.WzProperties
{
    /// <inheritdoc />
    /// <summary>
    /// A property that's value is null
    /// </summary>
    public class WzNullProperty : WzImageProperty
    {
        public override WzImageProperty DeepClone() => new WzNullProperty(Name);

        public override WzPropertyType Type => WzPropertyType.Null;

        public override WzObjectType ObjectType => WzObjectType.Property;

        public override void Dispose() => Name = null;

        /// <summary>
        /// Creates a blank WzNullProperty
        /// </summary>
        public WzNullProperty() { }

        /// <summary>
        /// Creates a WzNullProperty with the specified name
        /// </summary>
        /// <param name="propName">The name of the property</param>
        public WzNullProperty(string propName) => Name = propName;
    }
}
