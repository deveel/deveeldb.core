using System;

namespace Deveel.Data.Storage {
	public static class KnownStorageSystemIds {
		public const string InMemory = "in-memory";
		public const string JournaledFile = "journaled";
		public const string SingleFile = "file";
	}
}