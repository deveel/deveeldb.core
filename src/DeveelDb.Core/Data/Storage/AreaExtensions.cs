using System;
using System.Threading.Tasks;

namespace Deveel.Data.Storage {
	public static class AreaExtensions {
		public static Task<int> ReadAsync(this IArea area, byte[] buffer) {
			return area.ReadAsync(buffer, 0, buffer.Length);
		}

		public static int Read(this IArea area, byte[] buffer, int offset, int count)
			=> area.ReadAsync(buffer, offset, count).Result;

		public static int Read(this IArea area, byte[] buffer)
			=> area.Read(buffer, 0, buffer.Length);

		public static async Task<byte> ReadByteAsync(this IArea area) {
			var bytes = new byte[1];
			area.Read(bytes, 0, 1);
			return bytes[0];
		}

		public static short ReadInt16(this IArea area) {
			var bytes = new byte[2];
			area.Read(bytes, 0, 2);
			return BitConverter.ToInt16(bytes, 0);
		}
		public static int ReadInt32(this IArea area) {
			var bytes = new byte[4];
			area.Read(bytes, 0, 4);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static long ReadInt64(this IArea area) {
			var bytes = new byte[8];
			area.Read(bytes, 0, 8);
			return BitConverter.ToInt64(bytes, 0);
		}

		public static void Write(this IArea area, byte[] buffer)
			=> area.Write(buffer, 0, buffer.Length);

		public static void Write(this IArea area, byte value) {
			var bytes = new byte[1];
			bytes[0] = value;
			area.Write(bytes, 0, 1);
		}

		public static void Write(this IArea area, short value) {
			var bytes = BitConverter.GetBytes(value);
			area.Write(bytes);
		}

		public static void Write(this IArea area, int value) {
			var bytes = BitConverter.GetBytes(value);
			area.Write(bytes);
		}

		public static void Write(this IArea area, long value) {
			var bytes = BitConverter.GetBytes(value);
			area.Write(bytes);
		}
	}
}