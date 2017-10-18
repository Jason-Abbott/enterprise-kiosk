using Idaho;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Idaho.Data {
	public class File {
		private bool _schemaChanged = false;
		private int _dataFileCount = 0;	

		#region Static Fields

		private static DirectoryInfo _dataFolder;
		private static DirectoryInfo _contentFolder;
		private static FileInfo _fulfillmentFile;
		private static Int16 _maxTempFiles = 5;
		private static string _permissionTemplateName = string.Empty;

		/// <summary>
		/// Mail template describing file and folder permission settings
		/// </summary>
		internal static string PermissionTemplate {
			get { return File.Content(_permissionTemplateName); }
		}
		public static string PermissionTemplateName { set { _permissionTemplateName = value; } }

		public static DirectoryInfo DataFolder {
			set { _dataFolder = value; }
			get { return _dataFolder; }
		}
		public static DirectoryInfo ContentFolder {
			set { _contentFolder = value; }
			internal get { return _contentFolder; }
		}
		public static Int16 MaxTempFiles { set { _maxTempFiles = value; } }
		public static FileInfo FulfillmentFile { set { _fulfillmentFile = value; } }

		#endregion

		#region Properties

		/// <summary>
		/// Becomes true if instance schema differs from persisted schema
		/// </summary>
		public bool SchemaChanged { get { return _schemaChanged; } }

		/// <summary>
		/// Number of data files found by Cleanup method
		/// </summary>
		public int DataFileCount { get { return _dataFileCount; } }
		//public string[] Content { get { return _content; } }

		#endregion

		#region Save

		/// <summary>
		/// Serialize and save object to disk
		/// </summary>
		public FileInfo Save<T>(T entity) {
			Type type = entity.GetType();
			FileInfo file = this.DataFile(type);
			FileStream stream;
			BinaryFormatter bf = new BinaryFormatter();
		   
			stream = file.OpenWrite();
			bf.AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
			try {
				bf.Serialize(stream, entity);
			} catch (System.Exception e) {
				// avoid corrupted file
				if (file.Exists) {
					if (stream != null) { stream.Close(); }
					file.Delete();
				}
				file = null;
				Idaho.Exception.Log(e, Idaho.Exception.Types.Serialization);
			} finally {
				if (stream != null) { stream.Close(); }
			}
			this.Cleanup(type);
			return file;
		}
	   
		#endregion

		/// <summary>
		/// Remove excess serialized entity files, if any
		/// </summary>
		/// <param name="type">The root type of entity files been cleaned</param>
		private void Cleanup(System.Type type) {
			FileInfo[] files = _dataFolder.GetFiles(this.Pattern(type));
			_dataFileCount = files.Length;
			int filesToDelete = _dataFileCount - _maxTempFiles;
			if (filesToDelete < 0) { filesToDelete = 0; }
			if (filesToDelete > 0) {
				for (int x = 0; x < filesToDelete; x++) { files[x].Delete(); }
			}
		}

		/// <summary>
		/// Remove characters not safe or desired for a filename
		/// </summary>
		public static string SafeFileName(string name) {
			Regex re = new Regex("\\s\\/:\\*\\?\"\\|");
			string changed = re.Replace(name, string.Empty);
			changed = changed.Replace('>', '–');
			changed = changed.Replace('<', '–');
			changed = changed.Replace(' ', '_');
			return changed;
		}
	   
		#region Newest()
	   
		/// <summary>
		/// Get newest file in folder
		/// </summary>
		/// <param name="folder">Folder to search in</param>
		/// <param name="pattern">Pattern used for naming file from type</param>
		/// <param name="older">Option to count backwards from newest</param>
		private FileInfo NewestFile(DirectoryInfo folder, string pattern, int older) {
			FileInfo[] files = folder.GetFiles(pattern);
			if (files.Length > 0) {
				int index = files.Length - (1 + older);
				if (index < 0) { index = 0; }
				return files[index];
			} else {
				return null;
			}
		}
		private FileInfo NewestFile(string folder, string pattern) {
			DirectoryInfo path = new DirectoryInfo(string.Format("{0}{1}", HttpRuntime.AppDomainAppPath, folder));
			return NewestFile(path, pattern, 0);
		}
		public FileInfo Newest(string folder, string type) {
			return this.NewestFile(folder, this.Pattern(type));
		}
		public FileInfo Newest(string folder, System.Type type) {
			return this.NewestFile(folder, this.Pattern(type));
		}
		public FileInfo Newest(string folder) {
			return this.NewestFile(folder, "*.*");
		}
		public FileInfo Newest(Type type, int older) {
			return this.NewestFile(_dataFolder, this.Pattern(type), older);
		}
		public FileInfo Newest(Type type) {
			return this.Newest(type, 0);
		}
	   
		#endregion

		/// <summary>
		/// Pattern for serialized entity file names
		/// </summary>
		private string Pattern(System.Type type) { return this.Pattern(type.FullName); }
		private string Pattern(string type) { return string.Format("{0}_*.dat", type); }

		/// <summary>
		/// Create date file for given entity type
		/// </summary>
		private FileInfo DataFile(System.Type type) {
			if (_dataFolder == null) {
				throw new NullReferenceException("No DataFolder has been specified"); }
			return new FileInfo(string.Format("{0}/{1}_{2}.dat",
				_dataFolder.ToString(), type.FullName, DateTime.Now.Ticks));
		}

		/// <summary>
		/// Write bytes to given path and file name
		/// </summary>
		public static FileInfo WriteBytes(byte[] data, DirectoryInfo folder, string fileName) {
			FileInfo file = null;
			try {
				file = new FileInfo(folder.FullName + "\\" + fileName);
				FileStream output = new FileStream(file.FullName, FileMode.Create);
				BinaryWriter writer = new BinaryWriter(output);
				writer.Write(data);
				writer.Close();
				file.Refresh();
				return file;
			} catch {
				return file;
			}
		}
		public static FileInfo WriteBytes(byte[] data, string fileName) {
			return WriteBytes(data, _dataFolder, fileName);
		}
		public static FileInfo WriteBytes(byte[] data) {
			string fileName = DateTime.Now.Ticks + ".tmp";
			return WriteBytes(data, fileName);
		}

		/// <summary>
		/// Read all bytes from given file
		/// </summary>
		public static byte[] ReadBytes(FileInfo file, bool deleteAfterRead) {
			byte[] data = null;
			if (file != null && file.Exists) {
				FileStream stream = file.OpenRead();
				data = new byte[stream.Length];
				stream.Read(data, 0, (int)stream.Length);
				stream.Close();
				if (deleteAfterRead) { file.Delete(); }
			}
			return data;
		}
		public static byte[] ReadBytes(string fileName, bool deleteAfterRead) {
			FileInfo file = new FileInfo(_dataFolder.FullName + "\\" + fileName);
			return ReadBytes(file, deleteAfterRead);
		}
		public static byte[] ReadBytes(FileInfo file) {
			return ReadBytes(file, false);
		}
		public static byte[] ReadBytes(string fileName) {
			return ReadBytes(fileName, false);
		}
	   
		#region Load

		/// <summary>
		/// Load serialized object
		/// </summary>
		/// <typeparam name="T">The type of entity to load</typeparam>
		/// <param name="file">The file containing serialized entity</param>
		/// <param name="convert">True if conversion should be attempted</param>
		/// http://msdn2.microsoft.com/en-us/library/ms229752.aspx
		/// http://msdn2.microsoft.com/en-us/library/ms229752(vs.80).aspx
		/// http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=279028
		/// http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=93814
		public T Load<T>(FileInfo file, bool convert) {
			T entity = default(T);

			_schemaChanged = false;
		   
			if (file.Exists) {
				FileStream stream = file.OpenRead();
				BinaryFormatter bf = new BinaryFormatter();
				bf.Context = new StreamingContext(StreamingContextStates.Persistence);
				try {
					entity = (T)bf.Deserialize(stream);
				} catch (System.Exception ex1) {
					// standard deserialization didn't work so attempt schema migration
					Idaho.Exception.Log(ex1, Exception.Types.Serialization,
						"Will fail over to custom deserialization");
					stream.Position = 0;
					try {
						if (convert) { bf.Binder = new Binder(); }
						bf.SurrogateSelector = new Migration(convert);
						entity = (T)bf.Deserialize(stream);
						_schemaChanged = true;
					} catch (System.Exception ex2) {
						Idaho.Exception.Log(ex2, Idaho.Exception.Types.Serialization);
					}
				} finally {
					stream.Close();
				}
			}
			return entity;
		}
		public T Load<T>(string fileName) {
			return this.Load<T>(new FileInfo(fileName), false);
		}

		#endregion

		#region Content

		/// <summary>
		/// Load file content as string
		/// </summary>
		/// <remarks>Used to load and cache e-mail templates and such</remarks>
		public static string Content(string fileName) {
			if (string.IsNullOrEmpty(fileName)) { return string.Empty; }

			string content = string.Empty;
			HttpContext context = HttpContext.Current;

			fileName = fileName.Replace('/', '\\');
			if (context != null) { content = Utility.NullSafe<string>(context.Cache[fileName]); }

			if (string.IsNullOrEmpty(content) && _contentFolder != null) {
				string fullPath = _contentFolder.FullName + "\\" + fileName;
				if (System.IO.File.Exists(fullPath)) {
					StreamReader file = new StreamReader(fullPath);
					content = file.ReadToEnd();
					file.Close();
					
					if (context != null) {
						// put content in cache
						CacheDependency dependsOn = new CacheDependency(fullPath);
						context.Cache.Insert(fileName, content, dependsOn);
					}
				}
			}
			return content;
		}

		/// <summary>
		/// Get an array of printable bytes
		/// </summary>
		/// <param name="file"></param>
		/// <param name="minLength"></param>
		public static string[] Content(Stream file, int minLength) {
			BinaryReader br = new BinaryReader(file);
			byte[] bytes = br.ReadBytes((int)file.Length);

			List<String> text = null;
			StringBuilder phrase = new StringBuilder();
			int nullCount = 0;
			Regex re = new Regex("^\\W|^list\\/", RegexOptions.IgnoreCase);
			int c;

			for (int x = 0; x <= bytes.Length - 1; x++) {
				c = bytes[x];

				if (c > 31 && c < 128) {
					phrase.Append(Convert.ToChar(c));
					nullCount = 0;
				} else {
					// allow single null byte between printable bytes
					if (c == 0 && nullCount == 0) { nullCount++; } else {
						// terminate phrase
						if (phrase.Length >= minLength && !re.IsMatch(phrase.ToString())) {
							//If phrase.Length >= minLength Then
							text.Add(phrase.ToString());
						}
						phrase.Length = 0;
						nullCount = 0;
					}
				}
			}
			return text.ToArray();
		}
	   
		#endregion
	   
	}
}