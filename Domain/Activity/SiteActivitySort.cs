using System;
using System.Collections.Generic;

namespace Idaho {
	public class SiteActivitySort : IComparer<SiteActivity> {

		private Fields _field;
		private Entity.SortDirections _direction = Entity.SortDirections.Ascending;

		public enum Fields {
			Type = 1,
			IpAddress,
			Date,
			User
		}

		public Entity.SortDirections Direction {
			get { return _direction; }
			set { _direction = value; }
		}

		#region Constructors

		public SiteActivitySort(Fields field) { _field = field; }
		public SiteActivitySort(Fields sort, Entity.SortDirections direction) {
			_field = sort;
			_direction = direction;
		}

		#endregion

		/// <summary>
		/// Sort activities according to given field
		/// </summary>
		public int Compare(SiteActivity a1, SiteActivity a2) {
			int result = 0;

			switch (_field) {
				case Fields.IpAddress:
					result = a1.IpAddress.CompareTo(a2.IpAddress);
					if (result == 0) { result = a1.On.CompareTo(a2.On); }
					break;
				/*
				case Fields.User:
					if (a1.User != null && a2.User != null) {
						result = a1.User.CompareTo(a2.User);
					}
					if (result == 0) { result = a1.On.CompareTo(a2.On); }
					break;
				*/
				case Fields.Date:
					result = a1.On.CompareTo(a2.On);
					break;
			}

			if (_direction == Entity.SortDirections.Descending) { result *= -1; }
			return result;
		}
	}
}