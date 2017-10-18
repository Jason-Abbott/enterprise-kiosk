using System;
using System.Collections.Generic;
using System.Text;

namespace Idaho {
	/// <summary>
	/// A pair of values
	/// </summary>
	public class Pair<T> {
		private T _first;
		private T _second;
		private bool _independentSelf = true;

		#region Properties

		/// <summary>
		/// Control relationship between independent/dependent and self/other pairs
		/// </summary>
		/// <remarks>
		/// By default, the self item is the independent item.
		/// </remarks>
		public bool IndependentSelf { set { _independentSelf = value; } }
		
		public T First { get { return _first; } set { _first = value; } }
		public T Second { get { return _second; } set { _second = value; } }

		// in terms of a change
		public T From { get { return this.First; } set { this.First = value; } }
		public T To { get { return this.Second; } set { this.Second = value; } }
		
		// in terms of relationships
		public T Independent { get { return _first; } set { _first = value; } }
		public T Dependent { get { return _second; } set { _second = value; } }

		/// <summary>
		/// Value for "this" object
		/// </summary>
		public T Self {
			get {
				return (_independentSelf) ? this.Independent : this.Dependent; }
			set {
				if (_independentSelf) { this.Independent = value; }
				else { this.Dependent = value; }
			}
		}

		/// <summary>
		/// Value for the "other" object in the pair
		/// </summary>
		public T Other {
			get {
				return (_independentSelf) ? this.Dependent : this.Independent;
			}
			set {
				if (_independentSelf) { this.Dependent = value; } else { this.Independent = value; }
			}
		}

		#endregion

		public Pair(T first, T second) { _first = first; _second = second; }
		public Pair() { }

		/// <summary>
		/// Reverse which item is first/second
		/// </summary>
		public void Reverse() {
			T wasFirst = _first;
			_first = _second;
			_second = wasFirst;
		}

		/// <summary>
		/// Are either of the pair the given item
		/// </summary>
		public bool Contains(T e) {
			return _first.Equals(e) || _second.Equals(e);
		}
	}
}