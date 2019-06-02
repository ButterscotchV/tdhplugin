using System;
using System.Linq;

namespace TDHPlugin.Networking
{
	public static class IntExtensions
	{
		public static byte[] ToBytes(this int integer)
		{
			byte[] bytes = BitConverter.GetBytes(integer);
			return BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes;
		}

		public static int ToInt(this byte[] bytes)
		{
			return BitConverter.ToInt32(BitConverter.IsLittleEndian ? bytes.Reverse().ToArray() : bytes, 0);
		}
	}
}