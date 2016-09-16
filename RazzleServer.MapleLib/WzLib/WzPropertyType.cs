namespace MapleLib.WzLib
{
	public enum WzPropertyType : int
	{
		#region Regular
		Null = 0x1,
		UnsignedShort = 0x2,
		CompressedInt = 0x4,
		ByteFloat = 0x8,
		Double = 0x10,
		String = 0x20,
		#endregion

		#region Extended
		SubProperty = 0x80,
		Canvas = 0x100,
		Vector = 0x200,
		Convex = 0x400,
		Sound = 0x800,
		UOL = 0x1000,
		#endregion

		#region Png
		PNG = 0x2000,
		#endregion
	}
}