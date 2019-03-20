using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using RazzleServer.Common.Util;
using RazzleServer.Common.Wz.Util;

//using System;
//using NAudio.Wave;

namespace RazzleServer.Common.Wz.WzProperties
{
    /// <summary>
    /// A property that contains data for an MP3 file
    /// </summary>
    public class WzSoundProperty : WzExtended
    {
        public static readonly ILogger Log = LogManager.CreateLogger<WzSoundProperty>();

        #region Fields

        private byte[] _mp3Bytes;
        private WaveFormat _wavFormat;
        private readonly WzBinaryReader _wzReader;
        private bool _headerEncrypted;
        private readonly long _offs;
        private readonly int _soundDataLen;

        private static readonly byte[] SoundHeader =
        {
            0x02,
            0x83, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70,
            0x8B, 0xEB, 0x36, 0xE4, 0x4F, 0x52, 0xCE, 0x11, 0x9F, 0x53, 0x00, 0x20, 0xAF, 0x0B, 0xA7, 0x70,
            0x00,
            0x01,
            0x81, 0x9F, 0x58, 0x05, 0x56, 0xC3, 0xCE, 0x11, 0xBF, 0x01, 0x00, 0xAA, 0x00, 0x55, 0x59, 0x5A
        };

        #endregion

        #region Inherited Members

        public override WzImageProperty DeepClone()
        {
            var clone = new WzSoundProperty(Name, Length, Header, _mp3Bytes);
            return clone;
        }

        public override object WzValue => GetBytes(false);

        public override void SetValue(object value)
        {
        }

        /// <summary>
        /// The WzPropertyType of the property
        /// </summary>
        public override WzPropertyType PropertyType => WzPropertyType.Sound;

        public override void WriteValue(WzBinaryWriter writer)
        {
            var data = GetBytes(false);
            writer.WriteStringValue("Sound_DX8", 0x73, 0x1B);
            writer.Write((byte) 0);
            writer.WriteCompressedInt(data.Length);
            writer.WriteCompressedInt(Length);
            writer.Write(Header);
            writer.Write(data);
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public override void Dispose()
        {
            Name = null;
            _mp3Bytes = null;
        }

        #endregion

        #region Custom Members

        /// <summary>
        /// The data of the mp3 header
        /// </summary>
        public byte[] Header { get; set; }

        /// <summary>
        /// Length of the mp3 file in milliseconds
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Creates a WzSoundProperty with the specified name
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="reader">The wz reader</param>
        /// <param name="parseNow">Indicating whether to parse the property now</param>
        public WzSoundProperty(string name, WzBinaryReader reader, bool parseNow)
        {
            Name = name;
            _wzReader = reader;
            reader.BaseStream.Position++;

            //note - soundDataLen does NOT include the length of the header.
            _soundDataLen = reader.ReadCompressedInt();
            Length = reader.ReadCompressedInt();

            var headerOff = reader.BaseStream.Position;
            reader.BaseStream.Position += SoundHeader.Length; //skip GUIds
            int wavFormatLen = reader.ReadByte();
            reader.BaseStream.Position = headerOff;

            Header = reader.ReadBytes(SoundHeader.Length + 1 + wavFormatLen);
            ParseHeader();

            //sound file offs
            _offs = reader.BaseStream.Position;
            if (parseNow)
            {
                _mp3Bytes = reader.ReadBytes(_soundDataLen);
            }
            else
            {
                reader.BaseStream.Position += _soundDataLen;
            }
        }

        public WzSoundProperty(string name) => Name = name;

        /// <summary>
        /// Creates a WzSoundProperty with the specified name and data
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="lenMs">The sound length</param>
        /// <param name="header">The sound header</param>
        /// <param name="data">The sound data</param>
        public WzSoundProperty(string name, int lenMs, byte[] header, byte[] data)
        {
            Name = name;
            Length = lenMs;
            Header = header;
            _mp3Bytes = data;
            ParseHeader();
        }

        /// <summary>
        /// Creates a WzSoundProperty with the specified name from a file
        /// </summary>
        /// <param name="name">The name of the property</param>
        /// <param name="file">The path to the sound file</param>
        public WzSoundProperty(string name, string file)
        {
            Name = name;
            using (var reader = new Mp3FileReader(file))
            {
                _wavFormat = reader.Mp3WaveFormat;
                Length = (int) (reader.Length * 1000d / reader.WaveFormat.AverageBytesPerSecond);
                RebuildHeader();
            }

            _mp3Bytes = File.ReadAllBytes(file);
        }

        public static bool Memcmp(byte[] a, byte[] b, int n)
        {
            for (var i = 0; i < n; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public void RebuildHeader()
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(SoundHeader);
                var wavHeader = StructToBytes(_wavFormat);
                if (_headerEncrypted)
                {
                    for (var i = 0; i < wavHeader.Length; i++)
                    {
                        wavHeader[i] ^= _wzReader.WzKey[i];
                    }
                }

                bw.Write((byte) wavHeader.Length);
                bw.Write(wavHeader, 0, wavHeader.Length);
                Header = ms.ToArray();
            }
        }

        private static byte[] StructToBytes<T>(T obj)
        {
            var result = new byte[Marshal.SizeOf(obj)];
            var handle = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
                return result;
            }
            finally
            {
                handle.Free();
            }
        }

        private static T BytesToStruct<T>(byte[] data) where T : new()
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                return Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            }
            finally
            {
                handle.Free();
            }
        }

        private static T BytesToStructConstructorless<T>(byte[] data)
        {
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var obj = (T) FormatterServices.GetUninitializedObject(typeof(T));
                Marshal.PtrToStructure(handle.AddrOfPinnedObject(), obj);
                return obj;
            }
            finally
            {
                handle.Free();
            }
        }

        private void ParseHeader()
        {
            var wavHeader = new byte[Header.Length - SoundHeader.Length - 1];
            Buffer.BlockCopy(Header, SoundHeader.Length + 1, wavHeader, 0, wavHeader.Length);

            if (wavHeader.Length < Marshal.SizeOf<WaveFormat>())
            {
                return;
            }

            var wavFmt = BytesToStruct<WaveFormat>(wavHeader);

            if (Marshal.SizeOf<WaveFormat>() + wavFmt.ExtraSize != wavHeader.Length)
            {
                //try decrypt
                for (var i = 0; i < wavHeader.Length; i++)
                {
                    wavHeader[i] ^= _wzReader.WzKey[i];
                }

                wavFmt = BytesToStruct<WaveFormat>(wavHeader);

                if (Marshal.SizeOf<WaveFormat>() + wavFmt.ExtraSize != wavHeader.Length)
                {
                    Log.LogCritical("Failed to parse sound header");
                    return;
                }

                _headerEncrypted = true;
            }

            // parse to mp3 header
            if (wavFmt.Encoding == WaveFormatEncoding.MpegLayer3 && wavHeader.Length >= Marshal.SizeOf<Mp3WaveFormat>())
            {
                _wavFormat = BytesToStructConstructorless<Mp3WaveFormat>(wavHeader);
            }
            else if (wavFmt.Encoding == WaveFormatEncoding.Pcm)
            {
                _wavFormat = wavFmt;
            }
            else
            {
                Log.LogError($"Unknown wave encoding: {wavFmt.Encoding}");
            }
        }

        #endregion

        public byte[] GetBytes(bool saveInMemory)
        {
            if (_mp3Bytes != null)
            {
                return _mp3Bytes;
            }

            if (_wzReader == null)
            {
                return null;
            }

            var currentPos = _wzReader.BaseStream.Position;
            _wzReader.BaseStream.Position = _offs;
            _mp3Bytes = _wzReader.ReadBytes(_soundDataLen);
            _wzReader.BaseStream.Position = currentPos;

            if (saveInMemory)
            {
                return _mp3Bytes;
            }

            var result = _mp3Bytes;
            _mp3Bytes = null;
            return result;
        }

        public void SaveToFile(string file) => File.WriteAllBytes(file, GetBytes(false));

        public override byte[] GetBytes() => GetBytes(false);
    }
}
