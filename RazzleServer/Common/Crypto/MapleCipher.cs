using System;

namespace RazzleServer.Common.MapleCryptoLib
{
	/// <summary>
	/// Cipher class used for encrypting and decrypting maple packet data
	/// </summary>
	public class MapleCipher
	{
        /// <summary>
		/// AES transformer
		/// </summary>
		private FastAES Transformer { get; set; }

		/// <summary>
		/// General locker to prevent multithreading
		/// </summary>
		private volatile Object Locker = new Object();

		/// <summary>
		/// Vector to use in the MapleCrypto
		/// </summary>
		private InitializationVector MapleIV { get; set; }

		/// <summary>
		/// Gameversion of the current <see cref="MapleCipher"/> instance
		/// </summary>
		public ushort GameVersion { get; private set; }

		/// <summary>
		/// Bool stating if the current instance received its handshake
		/// </summary>
		public bool Handshaken { get; set; }

		/// <summary>
		/// Creates a new instance of <see cref="MapleCipher"/>
		/// </summary>
		/// <param name="currentGameVersion">The current MapleStory version</param>
		/// <param name="AESKey">AESKey for the current MapleStory version</param>
		public MapleCipher(ushort currentGameVersion, ulong AESKey)
		{
			Handshaken = false;
			GameVersion = currentGameVersion;
			Transformer = new FastAES(ExpandKey(AESKey));
		}
		
        /// <summary>
		/// Encrypts packet data
		/// </summary>
		public ushort? Encrypt(ref byte[] data, bool toClient)
		{
			if (!Handshaken || MapleIV == null) return null;
			ushort? ret;


			byte[] newData = new byte[data.Length + 4];
			if (toClient)
			{
				WriteHeaderToClient(newData);
			}
			else
			{
				WriteHeaderToServer(newData);
			}

			EncryptShanda(data);

			lock (Locker)
			{
				Transform(data);
				ret = MapleIV.MustSend ? MapleIV.LOWORD : null as ushort?;
			}

			Buffer.BlockCopy(data, 0, newData, 4, data.Length);
			data = newData;

			return ret;
		}

		/// <summary>
		/// Decrypts a maple packet contained in <paramref name="data"/>
		/// </summary>
		/// <param name="data">Data to decrypt</param>
		public void Decrypt(ref byte[] data)
		{
			if (!Handshaken || MapleIV == null) return;
			int length = GetPacketLength(data);

			byte[] newData = new byte[length];
			Buffer.BlockCopy(data, 4, newData, 0, length);

			lock (Locker)
			{
				Transform(newData);
			}
			DecryptShanda(newData);
			data = newData;
		}

		/// <summary>
		/// Gets the length of <paramref name="data"/>
		/// </summary>
		/// <param name="data">Data to check</param>
		/// <returns>Length of <paramref name="data"/></returns>
		public int GetPacketLength(byte[] data) => (data[0] + (data[1] << 8)) ^ (data[2] + (data[3] << 8));

		/// <summary>
		/// Manually sets the vector for the current instance
		/// </summary>
		public void SetIV(uint IV)
		{
			MapleIV = new InitializationVector(IV);
			Handshaken = true;
		}

		/// <summary>
		/// Handles an handshake for the current instance
		/// </summary>
		public void Handshake(ref byte[] data)
		{
			ushort length = BitConverter.ToUInt16(data, 0);
			byte[] ret = new byte[length];
			Buffer.BlockCopy(data, 2, ret, 0, ret.Length);
			data = ret;
		}
		
        /// <summary>
		/// Expands the key we store as long
		/// </summary>
		/// <returns>The expanded key</returns>
		private byte[] ExpandKey(ulong AESKey)
		{
			byte[] Expand = BitConverter.GetBytes(AESKey);
			byte[] Key = new byte[Expand.Length * 4];
			for (int i = 0; i < Expand.Length; i++)
				Key[i * 4] = Expand[i];
			return Key;
		}

		/// <summary>
		/// Performs Maplestory's AES algo
		/// </summary>
		private void Transform(byte[] buffer)
		{
			int remaining = buffer.Length,
				length = 0x5B0,
				start = 0,
				index;

			byte[] realIV = new byte[sizeof(int) * 4],
				   IVBytes = MapleIV.Bytes;

			while (remaining > 0)
			{
				for (index = 0; index < realIV.Length; ++index)
					realIV[index] = IVBytes[index % 4];

				if (remaining < length) length = remaining;
				for (index = start; index < (start + length); ++index)
				{
					if (((index - start) % realIV.Length) == 0)
						Transformer.TransformBlock(realIV);

					buffer[index] ^= realIV[(index - start) % realIV.Length];
				}
				start += length;
				remaining -= length;
				length = 0x5B4;
			}
			MapleIV.Shuffle();
		}

		/// <summary>
		/// Creates a packet header for outgoing data
		/// </summary>
		private unsafe void WriteHeaderToServer(byte[] data)
		{
			fixed (byte* pData = data)
			{
				*(ushort*)pData = (ushort)(GameVersion ^ MapleIV.HIWORD);
				*((ushort*)pData + 1) = (ushort)(*(ushort*)pData ^ (data.Length - 4));
			}
		}

		/// <summary>
		/// Creates a packet header for incoming data
		/// </summary>
		private unsafe void WriteHeaderToClient(byte[] data)
		{
			fixed (byte* pData = data)
			{
				*(ushort*)pData = (ushort)(-(GameVersion + 1) ^ MapleIV.HIWORD);
				*((ushort*)pData + 1) = (ushort)(*(ushort*)pData ^ (data.Length - 4));
			}
		}

		/// <summary>
		/// Decrypts <paramref name="buffer"/> using the custom MapleStory shanda
		/// </summary>
		private void DecryptShanda(byte[] buffer)
		{
			int length = buffer.Length, i;
			byte xorKey, save, len, temp;
			for (int passes = 0; passes < 3; passes++)
			{
				xorKey = 0;
				save = 0;
				len = (byte)(length & 0xFF);
				for (i = length - 1; i >= 0; --i)
				{
					temp = (byte)(ROL(buffer[i], 3) ^ 0x13);
					save = temp;
					temp = ROR((byte)((xorKey ^ temp) - len), 4);
					xorKey = save;
					buffer[i] = temp;
					--len;
				}

				xorKey = 0;
				len = (byte)(length & 0xFF);
				for (i = 0; i < length; ++i)
				{
					temp = ROL((byte)(~(buffer[i] - 0x48)), len & 0xFF);
					save = temp;
					temp = ROR((byte)((xorKey ^ temp) - len), 3);
					xorKey = save;
					buffer[i] = temp;
					--len;
				}
			}
		}

		/// <summary>
		/// Encrypts <paramref name="buffer"/> using the custom MapleStory shanda
		/// </summary>
		private void EncryptShanda(byte[] buffer)
		{
			int length = buffer.Length;
			byte xorKey, len, temp;
			int i;
			for (int passes = 0; passes < 3; passes++)
			{
				xorKey = 0;
				len = (byte)(length & 0xFF);
				for (i = 0; i < length; i++)
				{
					temp = (byte)((ROL(buffer[i], 3) + len) ^ xorKey);
					xorKey = temp;
					temp = (byte)(((~ROR(temp, len & 0xFF)) & 0xFF) + 0x48);
					buffer[i] = temp;
					len--;
				}
				xorKey = 0;
				len = (byte)(length & 0xFF);
				for (i = length - 1; i >= 0; i--)
				{
					temp = (byte)(xorKey ^ (len + ROL(buffer[i], 4)));
					xorKey = temp;
					temp = ROR((byte)(temp ^ 0x13), 3);
					buffer[i] = temp;
					len--;
				}
			}
		}

		/// <summary>
		/// Bitwise shift left
		/// </summary>
		private byte ROL(byte b, int count)
		{
			int tmp = b << (count & 7);
			return unchecked((byte)(tmp | (tmp >> 8)));
		}

		/// <summary>
		/// Bitwise shift right
		/// </summary>
		private byte ROR(byte b, int count)
		{
			int tmp = b << (8 - (count & 7));
			return unchecked((byte)(tmp | (tmp >> 8)));
		}
	}
}