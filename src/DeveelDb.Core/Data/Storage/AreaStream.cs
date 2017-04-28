using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Deveel.Data.Storage {
	public sealed class AreaStream : Stream {
		private readonly IArea area;

		public AreaStream(IArea area) {
			if (area == null)
				throw new ArgumentNullException("area");

			this.area = area;
		}

		public override void Flush() {
			area.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin) {
			if (origin == SeekOrigin.Begin) {
				if (offset >= Length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				area.Position = (int) offset;
			} else if (origin == SeekOrigin.Current) {
				var pos = area.Position;
				var length = area.Length;
				if (pos + offset >= length)
					throw new ArgumentOutOfRangeException(nameof(offset));

				area.Position = pos + (int) offset;
			} else {
				var length = area.Length;
				var newPos = length - offset;
				if (newPos < 0)
					throw new ArgumentOutOfRangeException(nameof(offset));

				area.Position = (int) newPos;
			}

			return area.Position;
		}

		public override void SetLength(long value) {
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count) {
			return area.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count) {
			area.Write(buffer, offset, count);
		}

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => !area.IsReadOnly;

		public override long Length => area.Length;

		public override long Position {
			get => area.Position;
			set => Seek(value, SeekOrigin.Begin);
		}
	}
}