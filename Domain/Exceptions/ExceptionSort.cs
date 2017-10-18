using System;
using System.Collections.Generic;

namespace Idaho {
	public class ExceptionSort : IComparer<Exception> {

		private Fields _field;
		private Entity.SortDirections _direction = Entity.SortDirections.Ascending;

		public enum Fields {
			Date = 1,
			Machine,
			Application,
			User,
			UserIP,
			Page
		}

		public Entity.SortDirections Direction {
			get { return _direction; } set { _direction = value; }
		}

		#region Constructors

		public ExceptionSort(Fields field) { _field = field; }
		public ExceptionSort(Fields sort, Entity.SortDirections direction) {
			_field = sort;
			_direction = direction;
		}

		#endregion

		/// <summary>
		/// Sort exceptions according to given field
		/// </summary>
		public int Compare(Idaho.Exception e1, Idaho.Exception e2) {
			int result = 0;

			switch (_field) {
				case Fields.Machine:
					result = e1.MachineName.CompareTo(e2.MachineName);
					if (result == 0) result = e1.On.CompareTo(e2.On);
					break;
				case Fields.User:
					//result = e1.User.CompareTo(e2.User);
					if (result == 0) result = e1.On.CompareTo(e2.On);
					break;
				case Fields.Date:
					result = e1.On.CompareTo(e2.On);
					break;
				case Fields.UserIP:
					result = e1.IpAddress.CompareTo(e2.IpAddress);
					if (result == 0) result = e1.On.CompareTo(e2.On);
					break;
				case Fields.Page:
					result = e1.Page.CompareTo(e2.Page);
					if (result == 0) result = e1.On.CompareTo(e2.On);
					break;
			}

			if (_direction == Entity.SortDirections.Descending) { result *= -1; }
			return result;
		}
	}
}