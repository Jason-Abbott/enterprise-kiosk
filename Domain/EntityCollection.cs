using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Idaho {
	[Serializable]
	public abstract class EntityCollection<T> : List<T>, IEditableObject,
		ISignalingObject, IListable, IEquatable<EntityCollection<T>> where T : Entity {
		
		// do not serialize entire site graph
		[NoUpdate][NonSerialized] private EntityCollection<T> _copy = null;
		[NoUpdate][NonSerialized] private ReaderWriterLock _lock = null;
		[NoUpdate][NonSerialized] private LockCookie _cookie;
		[NoUpdate] private bool _locked = false;
		[NoUpdate] private DateTime _synchronizedOn;
		private Status _status = Status.Enabled;
		[NoUpdate] private event EventHandler<EventArgs<T>> _subscribers;

		public event EventHandler<EventArgs<T>> ChangeEvent {
			add { _subscribers += value; }   
			remove { _subscribers -= value; }
		}

		public EntityCollection() { }
		
		/// <summary>
		/// Construst collection with initial members
		/// </summary>
		public EntityCollection(List<T> list) {
			if (list != null && list.Count > 0) { this.AddRange(list); }
		}

		#region ISignalingObject

		/// <summary>
		/// Collection of objects depending on this collection
		/// </summary>
		/// <remarks>
		/// This is meant primarily for output cache control. Since it
		/// may reference UI objects, it cannot be serialized.
		/// </remarks>
		[field: NonSerialized()] 
		private event EventHandler _dependencies;

		public event EventHandler Dependency {
			add { _dependencies += value; }
			remove { _dependencies -= value; }
		}

		/// <summary>
		/// Notify dependencies that this collection has changed
		/// </summary>
		public void Changed() {
			if (_dependencies != null) { _dependencies(this, null); }
		}

		#endregion

		#region IEditableObject

		/// <summary>
		/// Make a copy of the object prior to transaction steps
		/// </summary>
		/// <remarks>
		/// Enables rollback, typically during ORM synchronization
		/// </remarks>
		public void BeginEdit(bool writerLock) {
			_copy = this.Clone();
			if (writerLock) { this.BeginLoad(); }
		}
		public void BeginEdit() { this.BeginEdit(false); }

		/// <summary>
		/// Cancel editing and restore prior values
		/// </summary>
		/// <remarks>
		/// Restores prior values by importing internally held cloned copy
		/// </remarks>
		public void CancelEdit() {
			if (_copy != null) { this.Import(_copy); _copy = null; }
			if (_locked) { this.EndLoad(); }
		}

		/// <summary>
		/// Conclude editing
		/// </summary>
		/// <remarks>Remove internal copy used for rollback and mark as synchronized</remarks>
		public void EndEdit() {
			_copy = null;
			_synchronizedOn = DateTime.Now;
			if (_locked) { this.EndLoad(); }
		}

		/// <summary>
		/// Lock entity for loading data
		/// </summary>
		public void BeginLoad() {
			_cookie = this.Lock.UpgradeToWriterLock(Entity.LockPatience);
			_locked = true;
		}
		public void CancelLoad() {
			this.Lock.DowngradeFromWriterLock(ref _cookie);
			_status = Status.Incomplete;
			_locked = false;
		}

		/// <summary>
		/// Unlock entity after finished loading
		/// </summary>
		public void EndLoad() {
			this.Lock.DowngradeFromWriterLock(ref _cookie);
			_status = Status.Enabled;
			_synchronizedOn = DateTime.Now;
			_locked = false;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Return random member
		/// </summary>
		public T Random {
			get {
				if (this.Count > 0) {
					Random random = new Random();
					return this[random.Next(this.Count)];
				} else {
					return null;
				}
			}
		}

		protected ReaderWriterLock Lock {
			get {
				if (_lock == null) { _lock = new ReaderWriterLock(); }
				return _lock;
			}
		}

		/// <summary>
		/// Name/value collection for binding to select lists
		/// </summary>
		public virtual SortedDictionary<string, string> KeysAndValues {
			get {
				var list = new SortedDictionary<string, string>();
				this.ForEach(e => list.Add(e.ID.ToString(), e.ToString()));
				return list;
			}
		}

		public T this[Guid id] {
			get { return this.Find(e => e.ID.Equals(id)); }
		}

		public T this[string id] {
			get {
				if (string.IsNullOrEmpty(id)) { return null; }
				return this[new Guid(id)];
			}
		}

		/// <summary>
		/// All members with GUID in list
		/// </summary>
		public List<T> this[List<Guid> idList] {
			get {
				List<T> matches = new List<T>();
				if (idList != null && idList.Count > 0) {
					foreach (Guid g in idList) {
						matches.AddRange(this.FindAll(e => e.ID.Equals(g)));
					}
				}
				return matches;
			}
		}

		/// <summary>
		/// All members with GUID in string list
		/// </summary>
		public List<T> this[string[] idList] {
			get {
				List<T> matches = new List<T>();
				if (idList != null && idList.Length > 0) {
					Guid g = Guid.Empty;
					foreach (string s in idList) {
						g = new Guid(s);
						matches.AddRange(this.FindAll(e => e.ID.Equals(g)));
					}
				}
				return matches;
			}
		}

		/// <summary>
		/// Comma-delimited list of IDs in collection to pass a stored procedure
		/// </summary>
		public string IdList {
			get {
				StringBuilder sb = new StringBuilder();
				this.ForEach(e => sb.AppendFormat("{0},", e.ID));
				sb.TrimEnd();
				return sb.ToString();
			}
		}

		#endregion

		/// <summary>
		/// Item at given index
		/// </summary>
		/// <remarks>
		/// Provide a means to use index if default int getter is overridden
		/// </remarks>
		protected internal T AtIndex(int index) { return base[index]; }

		/// <summary>
		/// Indicate if the supplied entity is valid
		/// </summary>
		/// <remarks>
		/// For example, see if a Message entity has text and valid dates
		/// </remarks>
		protected bool ValidItem(T entity) { return entity.IsValid; }

		public void Add(T entity, bool distinct) {
			if (entity != null && this.ValidItem(entity) &&
				(!distinct || !this.Contains(entity))) {
				base.Add(entity);
				this.Sort();
				this.Changed(entity, EventType.Added);
			}
		}
		public new void Add(T entity) { this.Add(entity, false); }

		public new void Remove(T entity) {
			if (entity != null) {
				base.Remove(entity);
				this.Changed(entity, EventType.Removed);
			}
		}
		public void Remove(Guid id) { this.Remove(this[id]); }
		//public void Remove(string id) { this.Remove(this.WithID(id)); }
		public void Remove(List<T> entities) {
			if (entities == null) { return; }
			foreach (T e in entities) { this.Remove(e); }
		}
		public void Remove(List<Guid> ids) {
			if (ids == null) { return; }
			foreach (Guid id in ids) { this.Remove(id); }
		}
		public void Remove(string[] ids) {
			if (ids == null || ids.Length == 0) { return; }
			foreach (string id in ids) { this.Remove(new Guid(id)); }
		}
		/// <summary>
		/// Remove all members having given status
		/// </summary>
		public void Remove(Status status) {
			this.RemoveAll(e => e.Status == status);
		}

		/// <summary>
		/// Does this collection contain any member of the given collection
		/// </summary>
		public bool Contains(EntityCollection<T> list) {
			return list.Exists(e => this.Contains(e));
		}
		public new bool Contains(T entity) {
			return this.Exists(e => e.Equals(entity));
		}

		/// <summary>
		/// Copy all members from this collection to another
		/// </summary>
		public virtual void CopyTo(ref EntityCollection<T> target) {
			if (target == null) { return; }
			foreach (T i in this) { target.Add(i); }
		}

		/// <summary>
		/// Called when members are added or removed. May also be called manually
		/// by item editors.
		/// </summary>
		/// <remarks>
		/// This raises the change event if there are any subscribers.
		/// </remarks>
		private void Changed(T entity, EventType type) {
			if (_subscribers != null) {
				_subscribers(this, new EventArgs<T>(entity, type));
			}
		}

		/// <summary>
		/// Get count of members with given status
		/// </summary>
		public int StatusCount(Status status) {
			return this.Count(e => e.Status == status);
		}

		public virtual bool Equals(EntityCollection<T> other) {
			if (other == null) { return false; }
			if (this.Count == 0 && other.Count == 0) { return true; }
			if (this.Count != other.Count) { return false; }

			for (int x = 0; x < this.Count; x++) {
				if (!(this[x]).Equals(other[x])) { return false; }
			}
			return true;
		}
	}
}
