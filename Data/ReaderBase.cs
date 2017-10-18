using Idaho.Network;
using System;
using System.Collections.Generic;
using System.Data;

namespace Idaho.Data {
	/// <summary>
	/// DataReader to normalize values
	/// </summary>
	public abstract class ReaderBase { //<T> : IDataReader where T: IDataReader {

		private IDataReader _reader = null;
		private bool _disposedValue; // to detect redundant calls

        protected IDataReader DataReader { get { return _reader; } }

        public ReaderBase(IDataReader reader) { _reader = reader; }

		public bool Read() { return _reader.Read(); }
		public bool NextResult() { return _reader.NextResult(); }
		public void Close() { _reader.Close(); }
		public int Depth { get { return _reader.Depth; } }
		public int FieldCount { get { return _reader.FieldCount; } }
		public virtual string GetName(int i) { return _reader.GetName(i); }
		public int GetOrdinal(string name) { return _reader.GetOrdinal(name); }
		public DataTable GetSchemaTable() { return _reader.GetSchemaTable(); }
		public bool IsClosed { get { return _reader.IsClosed; } }
		public virtual bool IsDBNull(int i) { return _reader.IsDBNull(i); }
		public int RecordsAffected { get { return _reader.RecordsAffected; } }

		public object this[string name] {
			get {
				object value = _reader[name];
				return value.Equals(DBNull.Value) ? null : value;
			}
		}
		public virtual object this[int i] {
			get { return _reader.IsDBNull(i) ? null : _reader[i]; }
		}

		#region Metadata

		// data
		public IDataReader GetData(string name) {
			return this.GetData(_reader.GetOrdinal(name));
		}
		public virtual IDataReader GetData(int i) {
			return _reader.GetData(i);
		}

		// field type name
		public string GetDataTypeName(string name) {
			return GetDataTypeName(_reader.GetOrdinal(name));
		}
		public virtual string GetDataTypeName(int i) {
			return _reader.GetDataTypeName(i);
		}

		// field type
		public Type GetFieldType(string name) {
			return this.GetFieldType(_reader.GetOrdinal(name));
		}
		public virtual Type GetFieldType(int i) {
			return _reader.GetFieldType(i);
		}

		#endregion

		/// <summary>
		/// Get value converted to given type
		/// </summary>
		public T GetValue<T>(string name) {
			return this.GetValue<T>(_reader.GetOrdinal(name));
		}
		public virtual T GetValue<T>(int i) {
			return _reader.IsDBNull(i) ? default(T) : (T)_reader.GetValue(i);
		}

		public object GetValue(string name) {
			return this.GetValue(_reader.GetOrdinal(name));
		}
		public virtual object GetValue(int i) {
			return _reader.IsDBNull(i) ? null : _reader.GetValue(i);
		}

		/// <param name="values">Array of objects to copy values into</param>
		public int GetValues(object[] values) { return _reader.GetValues(values); }

		#region Properietary Conversion Gets

		// IP address
		public Idaho.Network.IpAddress GetIpAddress(string name) {
			return this.GetIpAddress(_reader.GetOrdinal(name));
		}
		public Idaho.Network.IpAddress GetIpAddress(int i) {
			if (_reader.IsDBNull(i)) {
				return null;
			} else {
				IpAddress ip = null;
				int value = _reader.GetInt32(i);
				try {
					ip = new Idaho.Network.IpAddress(value);
				} catch (System.Exception e) {
					Idaho.Exception.Log(e, Exception.Types.InvalidArgument,
						"Attempting to parse " + value.ToString());
				}
				return ip;
			}
		}

		// DateTime
		public DateTime GetDateTime(string name) {
			return this.GetDateTime(_reader.GetOrdinal(name));
		}
		public DateTime GetDateTime(int i) {
			if (!_reader.IsDBNull(i)) {
				object o = _reader.GetValue(i);
				DateTime dt = DateTime.MinValue;
				if (o != null) {
					if (o is DateTime) {
						dt = (DateTime)o;
					} else if (o is string) {
						dt = DateTime.Parse(o as string);
					}
				}
				if (dt.Year != 9999) { return dt; }
			}
			return DateTime.MinValue;
		}

		// address
		public Address.States GetState(string name) {
			return this.GetState(_reader.GetOrdinal(name));
		}
		public Address.States GetState(int i) {
			if (_reader.IsDBNull(i)) {
				return Address.States.Unknown;
			} else {
				return Address.ParseState(_reader.GetString(i));
			}
		}

		public int GetZipCode(string name) {
			return this.GetZipCode(_reader.GetOrdinal(name));
		}
		public int GetZipCode(int i) {
			return GetInt32(i);
		}

		// phone number
		public PhoneNumber GetPhoneNumber(string name) {
			return this.GetPhoneNumber(_reader.GetOrdinal(name));
		}
		public PhoneNumber GetPhoneNumber(int i) {
			if (_reader.IsDBNull(i)) { return null; }
			return PhoneNumber.Parse(_reader.GetString(i));
		}

		#endregion

		#region IDataReader Conversion Gets

		// string
		public string GetString(string name, string ifNull) {
			return this.GetString(_reader.GetOrdinal(name), ifNull);
		}
		public string GetString(string name) { return this.GetString(name, string.Empty); }
		public virtual string GetString(int i, string ifNull) {
			return _reader.IsDBNull(i) ? ifNull : _reader.GetString(i);
		}
		public virtual string GetString(int i) { return this.GetString(i, string.Empty); }

		// int
		public int GetInt32(string name, int ifNull) {
			return this.GetInt32(_reader.GetOrdinal(name), ifNull);
		}
		public int GetInt32(string name) { return this.GetInt32(name, 0); }

		// sure seems retarded that normal GetInt32 chokes on an Int16
		public virtual int GetInt32(int i, int ifNull) {
			if (!_reader.IsDBNull(i)) {
				object value = _reader.GetValue(i);
				if (value is int) { return (int)value; }
				if (value is short) { return (int)((short)value); }
				if (value is double) { return (int)((double)value); }
				if (value is string) { return int.Parse((string)value); }
			}
			return ifNull;
		}
		public virtual int GetInt32(int i) { return this.GetInt32(i, 0); }

		// double
		public double GetDouble(string name) {
			return this.GetDouble(_reader.GetOrdinal(name));
		}
		public virtual double GetDouble(int i) {
			return _reader.IsDBNull(i) ? 0 : _reader.GetDouble(i);
		}
		
		// guid
		public Guid GetGuid(string name) {
			return this.GetGuid(_reader.GetOrdinal(name));
		}
		public virtual System.Guid GetGuid(int i) {
			return _reader.IsDBNull(i) ? Guid.Empty : _reader.GetGuid(i);
		}

		// boolean
		public virtual bool GetBoolean(string name, bool ifNull) {
			return this.GetBoolean(_reader.GetOrdinal(name), ifNull);
		}
		public virtual bool GetBoolean(string name) { return this.GetBoolean(name, false); }

		public virtual bool GetBoolean(int i, bool ifNull) {
			if (_reader.IsDBNull(i)) {
				return ifNull;
			} else {
				int bit = _reader.GetInt32(i);
				return (bit == 1);
			}
		}
		public virtual bool GetBoolean(int i) { return this.GetBoolean(i, false); }

		// byte
		public byte GetByte(string name) {
			return this.GetByte(_reader.GetOrdinal(name));
		}
		public virtual byte GetByte(int i) {
			if (_reader.IsDBNull(i)) { return 0; } else { return _reader.GetByte(i); }
		}

		// bytes
		public Int64 GetBytes(string name, Int64 fieldOffset,
			byte[] buffer, int bufferOffset, int length) {

			return GetBytes(_reader.GetOrdinal(name), fieldOffset, buffer, bufferOffset, length);
		}
		public virtual Int64 GetBytes(int i, Int64 fieldOffset,
			byte[] buffer, int bufferOffset, int length) {
			
			return _reader.IsDBNull(i) ? 0
				: _reader.GetBytes(i, fieldOffset, buffer, bufferOffset, length);
		}

		// char
		public char GetChar(string name) {
			return this.GetChar(_reader.GetOrdinal(name));
		}
		public virtual char GetChar(int i) {
			if (_reader.IsDBNull(i))
				return char.MinValue;
			else {
				char[] myChar = new char[1];
				_reader.GetChars(i, 0, myChar, 0, 1);
				return myChar[0];
			}
		}

		// chars
		public Int64 GetChars(string name, Int64 fieldOffset,
			char[] buffer, int bufferOffset, int length) {

			return this.GetChars(_reader.GetOrdinal(name), fieldOffset, buffer, bufferOffset, length);
		}
		public virtual Int64 GetChars(int i, Int64 fieldOffset,
			char[] buffer, int bufferOffset, int length) {

			return _reader.IsDBNull(i) ? 0
				: _reader.GetChars(i, fieldOffset, buffer, bufferOffset, length);
		}

		// decimal
		public decimal GetDecimal(string name) {
			return this.GetDecimal(_reader.GetOrdinal(name));
		}
		public virtual decimal GetDecimal(int i) {
			return _reader.IsDBNull(i) ? 0 : _reader.GetDecimal(i);
		}

		// float
		public float GetFloat(string name) {
			return this.GetFloat(_reader.GetOrdinal(name));
		}
		public virtual float GetFloat(int i) {
			return _reader.IsDBNull(i) ? 0 : _reader.GetFloat(i);
		}
		
		// short
		public short GetInt16(string name) {
			return this.GetInt16(_reader.GetOrdinal(name));
		}
		public virtual short GetInt16(int i) {
			if (_reader.IsDBNull(i)) { return 0; } else { return _reader.GetInt16(i); }
		}

		// long
		public Int64 GetInt64(string name) {
			return this.GetInt64(_reader.GetOrdinal(name));
		}

		public virtual Int64 GetInt64(int i) {
			return _reader.IsDBNull(i) ? 0 : _reader.GetInt64(i);
		}

		#endregion		

		#region IDisposable

		protected virtual void Dispose(bool disposing) {
			if (!_disposedValue) {
				// free unmanaged resources when explicitly called
				if (disposing) { _reader.Dispose(); }
			}
			_disposedValue = true;
		}
		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		~ReaderBase() { this.Dispose(false); }

		#endregion

	}
}
