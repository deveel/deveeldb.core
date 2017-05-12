using System;
using System.Threading.Tasks;

namespace Deveel.Data.Storage {
	public static class AreaExtensions {
		public static int Read(this IArea area, byte[] buffer, int offset, int length) {
			return area.ReadAsync(buffer, offset, length).Result;
		}

		public static async Task<byte> ReadByteAsync(this IArea area) {
			var bytes = new byte[1];
			await area.ReadAsync(bytes, 0, 1);
			return bytes[0];
		}

		public static byte ReadByte(this IArea area)
			=> area.ReadByteAsync().Result;

		public static async Task<short> ReadInt16Async(this IArea area) {
			var bytes = new byte[2];
			await area.ReadAsync(bytes, 0, 2);
			return BitConverter.ToInt16(bytes, 0);
		}

		public static short ReadInt16(this IArea area)
			=> area.ReadInt16Async().Result;

		public static async Task<int> ReadInt32Async(this IArea area) {
			var bytes = new byte[4];
			await area.ReadAsync(bytes, 0, 4);
			return BitConverter.ToInt32(bytes, 0);
		}

		public static int ReadInt32(this IArea area)
			=> area.ReadInt32Async().Result;

		public static async Task<long> ReadInt64Async(this IArea area) {
			var bytes = new byte[8];
			await area.ReadAsync(bytes, 0, 8);
			return BitConverter.ToInt64(bytes, 0);
		}

		public static long ReadInt64(this IArea area)
			=> area.ReadInt64Async().Result;

		public static Task WriteAsync(this IArea area, byte[] buffer) {
			return area.WriteAsync(buffer, 0, buffer.Length);
		}

		public static void Write(this IArea area, byte[] buffer, int offset, int count)
			=> area.WriteAsync(buffer, offset, count).Wait();

		public static void Write(this IArea area, byte[] buffer)
			=> area.Write(buffer, 0, buffer.Length);

		public static Task WriteAsync(this IArea area, byte value) {
			var bytes = new byte[1];
			bytes[0] = value;
			return area.WriteAsync(bytes, 0, 1);
		}

		public static void Write(this IArea area, byte value)
			=> area.WriteAsync(value).Wait();

		public static Task WriteAsync(this IArea area, short value) {
			var bytes = BitConverter.GetBytes(value);
			return area.WriteAsync(bytes);
		}

		public static void Write(this IArea area, short value)
			=> area.WriteAsync(value).Wait();

		public static Task WriteAsync(this IArea area, int value) {
			var bytes = BitConverter.GetBytes(value);
			return area.WriteAsync(bytes);
		}

		public static void Write(this IArea area, int value)
			=> area.WriteAsync(value).Wait();

		public static Task WriteAsync(this IArea area, long value) {
			var bytes = BitConverter.GetBytes(value);
			return area.WriteAsync(bytes);
		}

		public static void Write(this IArea area, long value)
			=> area.WriteAsync(value).Wait();

		public static void Flush(this IArea area) {
			area.FlushAsync().Wait();
		}
	}
}