using System;
using System.IO;
using System.Text;

namespace Unity.PlayMode.Scenarios.Instances.Editor.MultiplayLocalDevelopment.Services.Service
{
    internal static class MemoryStreamExtensions
    {
        /// <summary>
        /// ReadBigEndianInt16 reads an int16 from the supplied stream in big endian.
        /// </summary>
        /// <param name="s">MemoryStream</param>
        /// <returns>int16 from memory stream.</returns>
        public static short ReadBigEndianInt16(this MemoryStream s)
        {
            var bytes = new byte[2];
            s.Read(bytes, 0, 2); // TODO: Test this well, ReadExactly is not available in .NET 4.6

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// ReadBigEndianUInt16 reads an uint16 from the supplied stream in big endian.
        /// </summary>
        /// <param name="s">MemoryStream</param>
        /// <returns>uint16 from memory stream.</returns>
        public static ushort ReadBigEndianUInt16(this MemoryStream s)
        {
            var bytes = new byte[2];
            s.Read(bytes, 0, 2); // TODO: Test this well, ReadExactly is not available in .NET 4.6

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// ReadBigEndianInt32 reads an int32 from the supplied stream in big endian.
        /// </summary>
        /// <param name="s">MemoryStream</param>
        /// <returns>int32 from memory stream.</returns>
        public static int ReadBigEndianInt32(this MemoryStream s)
        {
            var bytes = new byte[4];
            s.Read(bytes, 0, 4); // TODO: Test this well, ReadExactly is not available in .NET 4.6
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// ReadBigEndianUInt32 reads an uint32 from the supplied stream in big endian.
        /// </summary>
        /// <param name="s">MemoryStream</param>
        /// <returns>uint32 from memory stream.</returns>
        public static uint ReadBigEndianUInt32(this MemoryStream s)
        {
            var bytes = new byte[4];
            s.Read(bytes, 0, 4); // TODO: Test this well, ReadExactly is not available in .NET 4.6

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// ReadString reads a length-prefixed UTF-8 string from the supplied stream. The maximum length of the string is 255.
        /// </summary>
        /// <param name="s">MemoryStream</param>
        /// <returns>String from memory stream.</returns>
        public static string ReadLengthPrefixedString(this MemoryStream s)
        {
            var length = s.ReadByte();
            var buffer = new byte[length];
            s.Read(buffer, 0, length); // TODO: Test this well, ReadExactly is not available in .NET 4.6
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Reads a null-terminated UTF-8 string from the supplied stream.
        /// </summary>
        /// <param name="s">MemoryStream</param>
        /// <returns>String from memory stream.</returns>
        public static string ReadNullTerminatedString(this MemoryStream s)
        {
            long length = s.Length - s.Position;
            ReadOnlySpan<byte> span = s.ToArray().AsSpan((int)s.Position, (int)length);

            int nullIndex = span.IndexOf((byte)0);
            if (nullIndex == -1)
            {
                throw new InvalidOperationException("No null terminator found in the stream.");
            }

            s.Position += nullIndex + 1;

            return Encoding.UTF8.GetString(span.Slice(0, nullIndex));
        }
    }
}
