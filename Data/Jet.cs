using System;
using System.Collections;
using System.Web;
using System.Data;
using System.Data.OleDb;
using System.Reflection;

namespace Idaho.Data {
	public class Jet : Relational<OleDbParameter, OleDbType> {
	   
		//Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Book1.xls;Extended Properties="Excel 8.0;HDR=YES;"
		private const string _connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
	   
		#region Constructor
	   
		/// <summary>
		/// Create Jet connection
		/// </summary>
		/// <remarks>
		/// If the data path points to an Excel file, an appropriate connection
		/// string will be generated and used. The base class needs to be
		/// constructed with a delegate to set parameter types. In this case,
		/// a simple anonymous function is the delegate.
		/// </remarks>
		public Jet(string dataPath) :
			base(new OleDbConnection(), new OleDbCommand(), new OleDbDataAdapter(),
				delegate(ref OleDbParameter p, OleDbType t) { p.OleDbType = t; } ) {

			if (dataPath.EndsWith("xls")) { dataPath +=
				";Extended Properties=\"Excel 8.0;HDR=YES\""; }
			this.Connect(string.Format("{0}{1};", _connectionString,
				dataPath.Replace("~", HttpRuntime.AppDomainAppPath)));
		}
		public Jet() : this(null) { }
	   
		#endregion

		/// <summary>
		/// Compact the Jet database
		/// </summary>
		/// <remarks>http://support.microsoft.com/kb/306287/EN-US/</remarks>
		public void Compact() {
			object[] parameters = new object[] { this.Connection.ConnectionString };
			object jro = Activator.CreateInstance(Type.GetTypeFromProgID("JRO.JetEngine"));

			this.Connection.Close();
			jro.GetType().InvokeMember("CompactDatabase",
				BindingFlags.InvokeMethod, null, jro, parameters);

			System.Runtime.InteropServices.Marshal.ReleaseComObject(jro);
			jro = null;

			//System.IO.File.Delete(mdwfilename);
			//System.IO.File.Move("C:\\tempdb.mdb", mdwfilename);


			//JRO.JetEngine jetEngine = new JRO.JetEngine();
		   
			// TODO: complete conversion
			//jetEngine.CompactDatabase(_connectionString, _connectionString);
		   
			//;Jet OLEDB:Engine Type=4
		   
			//sCompactFile = Replace(g_sDB_LOCATION, ".mdb", IIf(v_bBackup, "_" & SafeDate(Date) & ".mdb", "_compacted.mdb"))
			// sCompactFile = Server.Mappath(sCompactFile)
			// sLiveFile = Server.Mappath(g_sDB_LOCATION)
			// oFileSys = Server.CreateObject(g_sFILE_SYSTEM_OBJECT)
			// If oFileSys.FileExists(sCompactFile) Then Call oFileSys.DeleteFile(sCompactFile)
			// oJet = CreateObject("JRO.JetEngine")
			// Call oJet.CompactDatabase(g_sDB_CONNECT & sLiveFile, g_sDB_CONNECT & sCompactFile)
			// oJet = Nothing
			// If Not v_bBackup Then
			// ' replace live db with compacted file
			// sTempFile = Server.Mappath(Replace(g_sDB_LOCATION, ".mdb", "_temp.mdb"))
			// Call oFileSys.CopyFile(sLiveFile, sTempFile, True)
			// Call oFileSys.CopyFile(sCompactFile, sLiveFile, True)
			// Call oFileSys.DeleteFile(sCompactFile) ' delete temporary files
			// Call oFileSys.DeleteFile(sTempFile)
			// End If
		}
	}
}