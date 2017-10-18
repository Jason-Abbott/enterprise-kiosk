using Idaho.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace Idaho {

	[Flags]
	public enum Status {
		Complete = 0x1,
		Incomplete = ~Complete,
		Enabled = 0x10,
		Disabled = ~Enabled | Incomplete,
	}

	/// <summary>
	/// Base object implementing cloning, editing, locking
	/// </summary>
	[Serializable()]
	abstract public class Entity : IEditableObject, IEquatable<Entity>, IComparable<Entity> {
		// do not serialize entire site graph
		[NonSerialized] private Entity _copy = null;
		[NonSerialized] private ReaderWriterLock _lock = null;
		[NonSerialized] private LockCookie _cookie;
		[NoUpdate] private bool _locked = false;
		[NoUpdate] private Guid _id;
		[NoUpdate] private bool _changed = false;
		[NoUpdate] private DateTime _modifiedOn;
		[NoUpdate] private DateTime _createdOn;
		[NoUpdate] private DateTime _synchronizedOn;
		private Status _status = Status.Enabled;
		[NoUpdate] private TimeSpan _cacheDuration = TimeSpan.MaxValue;
		
		private int _views = 0;		// track user views of this object

		public enum SortDirections { Ascending = 1, Descending }

		#region Properties

		/// <summary>
		/// Has this entity changed
		/// </summary>
		/// <remarks>
		/// This can be used to determine if the entity needs to be saved again
		/// </remarks>
		protected bool Changed { set { _changed = value; } }

		/// <summary>
		/// How long to wait for a lock
		/// </summary>
		static internal TimeSpan LockPatience { get { return TimeSpan.FromSeconds(5); } }

		public DateTime CreatedOn {
			get { return _createdOn; }
			set { _createdOn = value; }
		}
		public Status Status { get { return _status; } set { _status = value; } }
		public Guid ID { get { return _id; } internal protected set { _id = value; } }

		protected ReaderWriterLock Lock {
			get {
				if (_lock == null) { _lock = new ReaderWriterLock(); }
				return _lock;
			}
		}
	
		/// <summary>
		/// When was the entity last loaded from its persistence medium
		/// </summary>
		private DateTime SynchronizedOn { get { return _synchronizedOn; } }
		
		/// <summary>
		/// Track when the entity was changed
		/// </summary>
		public DateTime ModifiedOn {
			get { return _modifiedOn; } set { _modifiedOn = value; }
		}

		public bool IsChanged {
			get { return _changed || _modifiedOn > _synchronizedOn; }
		}

		/// <summary>
		/// Has entity surpassed expiration
		/// </summary>
		public bool IsExpired {
			get { return _cacheDuration > (DateTime.Now - _synchronizedOn); }
		}

		/// <summary>
		/// Is object complete
		/// </summary>
		public virtual bool IsComplete { get { return _status != Status.Incomplete; } }
		public virtual bool IsEnabled { get { return _status == Status.Enabled; } }

		/// <summary>
		/// Determine if entity is valid
		/// </summary>
		/// <remarks>
		/// For example, are date members coherent or fields populated as required
		/// </remarks>
		public virtual bool IsValid { get { return true; } }

		#endregion

		#region Constructors

		protected Entity(bool createID) {
			if (createID) { _id = Guid.NewGuid(); }
			_createdOn = _modifiedOn = _synchronizedOn = DateTime.Now;
		}
		protected Entity() : this(true) { }
		protected Entity(Guid id) : this(false) { _id = id; }
		protected Entity(TimeSpan cacheDuration) : this(true) {
			_cacheDuration = cacheDuration;
		}
		protected Entity(Guid id, TimeSpan cacheDuration) : this(id) {
			_cacheDuration = cacheDuration;
		}
		protected Entity(DateTime expireOn)	: this(true) {
			Assert.Future(expireOn, "PastExpireDate");
			_cacheDuration = expireOn - DateTime.Now;
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

		/// <summary>
		/// Make a copy of the object prior to transaction steps
		/// </summary>
		/// <remarks>
		/// Enables rollback, typically during ORM synchronization
		/// </remarks>
		public void BeginEdit() { this.BeginEdit(false);  }

		/// <summary>
		/// Cancel editing and restore prior values
		/// </summary>
		/// <remarks>
		/// Restores prior values by importing internally held cloned copy
		/// </remarks>
		public void CancelEdit() {
			if (_copy != null) { this.Import(_copy); _copy = null; }
			if (_locked) { this.CancelLoad(); }
		}

		/// <summary>
		/// Conclude editing
		/// </summary>
		/// <remarks>Remove internal copy used for rollback and mark as synchronized</remarks>
		public void EndEdit() {
			_copy = null;
			_synchronizedOn = DateTime.Now;
			_changed = false;
			if (_locked) { this.EndLoad(); }
		}

		public void BeginLoad() {
			_cookie = this.Lock.UpgradeToWriterLock(LockPatience);
			_locked = true;
		}
		public void CancelLoad() {
			if (this.Lock.IsWriterLockHeld) {
				this.Lock.DowngradeFromWriterLock(ref _cookie);
			}
			_locked = false;
			_status = Status.Incomplete;
		}

		public void EndLoad() {
			if (this.Lock.IsWriterLockHeld) {
				this.Lock.DowngradeFromWriterLock(ref _cookie);
			}
			_locked = false;
			_status = Status.Enabled;
			_synchronizedOn = DateTime.Now;
		}

		#endregion

		#region IEquatable

		/// <summary>
		/// Does other entity have same identifier as this entity
		/// </summary>
		public virtual bool Equals(Entity other) { return _id.Equals(other.ID); }

		#endregion

		#region View Tracking

		/// <summary>
		/// Track user views of this entity
		/// </summary>
		public void AddView() { _views++; }
		public int ViewCount { get { return _views; } }
		public void ResetViews() { _views = 0; }

		#endregion

		public void Modified() { _modifiedOn = DateTime.Now; }

		/// <summary>
		/// Mark object as synchronized with underlying persistence medium
		/// </summary>
		public void Synchronized() {
			_synchronizedOn = DateTime.Now;
			_changed = false;
		}

		/// <summary>
		/// Compare object instances to see which is newer
		/// </summary>
		/// <returns>True if this instance is newer</returns>
		/// <remarks>Compared ModifiedOn property</remarks>
		public bool NewerThan(Entity other) {
			if (_modifiedOn != DateTime.MinValue) {
				if (other.ModifiedOn != DateTime.MinValue) {
					return (_modifiedOn > other.ModifiedOn);
				} else {
					return true;
				}
			} else {
				return false;
			}
		}

		/// <summary>
		/// Copy member values from another object to this one
		/// </summary>
		/// <returns>Whether the imported data is different (a change)</returns>
		/// <param name="source">The object to copy into this one</param>
		public virtual void Import(Entity source, bool writeLock) {
			if (writeLock) { this.BeginLoad(); }
			_changed = this.Import(source);

			if (_changed) { _modifiedOn = DateTime.Now; }
			if (writeLock) { this.EndLoad(); }
		}
		//public void Import(Entity source) { this.Import(source, false); }

		public int CompareTo(Entity other) {
			return string.Compare(this.ToString(), other.ToString());
		}
	}
}