using System;

namespace TDHPlugin.Networking
{
	public static class IntExtensions
	{
		public static byte[] ToBytes(this int integer)
		{
			byte[] bytes = new byte[4];

			bytes[0] = (byte) (integer >> 24);
			bytes[1] = (byte) (integer >> 16);
			bytes[2] = (byte) (integer >> 8);
			bytes[3] = (byte) integer;

			return bytes;
		}

		public static int ToInt(this byte[] bytes)
		{
			if (bytes.Length < 4)
				throw new ArgumentException($"{nameof(bytes)} must have at least 4 indices", nameof(bytes));

			return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
		}

		public static int ToInt(this byte[] bytes, int offset)
		{
			if (bytes.Length < 4 + offset)
				throw new ArgumentException($"{nameof(bytes)} must have at least 4 indices", nameof(bytes));

			return (bytes[offset + 0] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
		}
	}
}