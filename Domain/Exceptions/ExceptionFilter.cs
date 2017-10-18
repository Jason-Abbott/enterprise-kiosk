using System;

namespace Idaho {
	public class ExceptionFilter : IFilter {

		private Int64 _id = 0;
		private string _message = string.Empty;
		private string _stack = string.Empty;
		private string _machineName = string.Empty;
		private DateTime _after = DateTime.MinValue;
		private DateTime _before = DateTime.MaxValue;
		private Network.Login _login;
		private Network.IpAddress _ip;
		private string _browser = string.Empty;
		private string _cookies = string.Empty;
		private string _page = string.Empty;
		private bool? _emailed = null;
		private string _queryString = string.Empty;
		private string _notes = string.Empty;
		private User _user;
		private Exception.Types _type;

		#region Properties

		public Int64 ID { get { return _id; } set { _id = value; } }
		public Exception.Types Type { set { _type = value; } get { return _type; } }
		public string Page { get { return _page; } set { _page = value; } }
		public string Notes { get { return _notes; } set { _notes = value; } }
		public string QueryString { get { return _queryString; } set { _queryString = value; } }
		public bool? Emailed { get { return _emailed; } set { _emailed = value; } }
		public string Cookies { get { return _cookies; } set { _cookies = value; } }
		public string Browser { get { return _browser; } set { _browser = value; } }
		public Network.IpAddress IpAddress { get { return _ip; } set { _ip = value; } }
		public Network.Login Login { get { return _login; } set { _login = value; } }
		public string Message { get { return _message; } set { _message = value; } }
		public string Stack { get { return _stack; } set { _stack = value; } }
		public string MachineName { get { return _machineName; } set { _machineName = value; } }
		public DateTime After { get { return _after; } set { _after = value; } }
		public DateTime Before { get { return _before; } set { _before = value; } }
		public User User { get { return _user; } set { _user = value; } }

		#endregion

		public bool IsEmpty {
			get {
				return _message == string.Empty && _stack == string.Empty && _machineName == string.Empty &&
					_after == DateTime.MinValue && _before == DateTime.MaxValue && _login == null &&
					_ip == null && _browser == string.Empty && _cookies == string.Empty &&
					_emailed == null && _queryString == string.Empty && _notes == string.Empty &&
					_id == 0 && _user == null;
			}
		}
	}
}