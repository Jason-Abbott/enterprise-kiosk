using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

namespace Idaho {
	/// <summary>
	/// Utility methods to perform cloning and serialization
	/// </summary>
	public static class Serialization {

		/// <summary>
		/// Create a clone (new instance) of this object
		/// </summary>
		public static T Clone<T>(this T o) {
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();

			formatter.Serialize(stream, o);
			stream.Seek(0, SeekOrigin.Begin);
			object copy = formatter.Deserialize(stream);
			stream.Close();
			return (T)copy;
		}
		/// <summary>
		/// Binary serialize this object
		/// </summary>
		public static byte[] ToByteArray(this object o) {
			MemoryStream stream = new MemoryStream();
			BinaryFormatter formatter = new BinaryFormatter();
			byte[] bytes;

			formatter.AssemblyFormat = FormatterAssemblyStyle.Simple;
			formatter.Serialize(stream, o);
			stream.Seek(0, SeekOrigin.Begin);
			bytes = stream.ToArray();
			stream.Close();
			return bytes;
		}

		public static T Deserialize<T>(byte[] source) {
			MemoryStream stream = new MemoryStream(source, false);
			BinaryFormatter formatter = new BinaryFormatter();
			object copy = formatter.Deserialize(stream);
			stream.Close();
			return (T)copy;
		}
	}
}