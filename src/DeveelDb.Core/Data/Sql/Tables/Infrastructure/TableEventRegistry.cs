using System;
using System.Collections;
using System.Collections.Generic;

namespace Deveel.Data.Sql.Tables.Infrastructure {
	class TableEventRegistry : ITableEventRegistry {
	    private readonly List<ITableEvent> events;

	    internal TableEventRegistry(long commitId, IEnumerable<ITableEvent> events, bool readOnly) {
	        CommitId = commitId;

            this.events = new List<ITableEvent>();
            if (events != null)
                this.events.AddRange(events);

	        IsReadOnly = readOnly;
	    }

	    public TableEventRegistry(long commitId)
	        : this(commitId, null, false) {
	        
	    }

        public bool IsReadOnly { get; }

	    public long CommitId { get; }

        public IEnumerator<ITableEvent> GetEnumerator() {
	        lock (this) {
	            return events.GetEnumerator();
	        }
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

	    private void ThrowIfReadOnly() {
	        if (IsReadOnly)
	            throw new InvalidOperationException("The registry is read-only");
	    }

	    public void Register(ITableEvent @event) {
            ThrowIfReadOnly();

            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

	        lock (this) {
	            events.Add(@event);
	        }
	    }
	}
}